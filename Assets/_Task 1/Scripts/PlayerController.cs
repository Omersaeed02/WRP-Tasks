using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Task1Manager Task1Manager { get; private set; }
    
    public Transform mobDeleteTrigger;
    public Transform mobChecker;
    public Transform decal;
    public Animator playerAnimator;
    public GameObject collisionParticle;
    
    public PlayerState playerState = PlayerState.Idle;
    public LayerMask decalMask;
    
    public float speed = 7f;
    
    public static Action OnPlayerDowned;

    private bool _canUpdate;
    private bool _firstRideDone;
    private bool _deathOnce;
    private float _maxSpeed = 7f;

    private Collider _collider;
    private Rigidbody _rb;
    private Task1CameraHandler _task1CameraHandler;
    private MobHandler _ridingMob;
    
    private Transform _decalInstance;
    private GameObject _mobInCheck;
    
    private Vector2 _startTouchPos;
    private Vector2 _currentTouchPos;
    
    private Coroutine _beneathCheckCoroutine;
    
    public void SetTaskManager(Task1Manager tm)
    {
        Task1Manager = tm;
    }

    private void Awake()
    {
        _task1CameraHandler = Task1Manager.task1CameraHandler;
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        playerState = PlayerState.Falling;
        
        Invoke(nameof(EnableUpdate), 0.5f);
    }

    public void EnableUpdate()
    {
        _canUpdate = true;
    }
    
    private void OnEnable()
    {
        OnPlayerDowned += TouchGround;
    }

    private void OnDisable()
    {
        OnPlayerDowned -= TouchGround;
    }
    
    private void Update()
    {
        if (!_canUpdate) return;
        
        _task1CameraHandler.transform.position = Vector3.Lerp(_task1CameraHandler.transform.position, transform.position, Time.deltaTime * 3f); 
        
        SetAnimationState();

        if (Input.touchCount <= 0) return;
        
        var primaryTouch = Input.GetTouch(0);

        if (_ridingMob != null) playerState = PlayerState.Riding;
        
        switch (primaryTouch.phase)
        {
            case TouchPhase.Began:
                Debug.Log("Began");
                if (_mobInCheck != null)
                {
                    CaptureMob();
                }
                else
                {
                    _startTouchPos = primaryTouch.position;
                }
                break;
            
            case TouchPhase.Moved:
                Debug.Log("Move");
                if (playerState == PlayerState.Riding)
                {
                    _currentTouchPos = primaryTouch.position;
                    var delta = _currentTouchPos.x - _startTouchPos.x;
                
                    if (Mathf.Abs(delta) > 10)
                    {
                        var direction = delta switch
                        {
                            > 0 => -1,
                            < 0 => 1,
                            _ => 0
                        };

                        _ridingMob?.StrafeMob(direction);
        
                        _startTouchPos = _currentTouchPos;
                    }
                }
                break;
            
            case TouchPhase.Stationary:
                Debug.Log("Station");
                if (playerState == PlayerState.Riding) _ridingMob?.StrafeMob(0);
                break;
            
            case TouchPhase.Ended:
                Debug.Log("End");
                if (playerState == PlayerState.Riding) PlayerJump();
                break;
        }
    }

    private void PlayerJump()
    {
        DOTween.Kill(transform);
        transform.SetParent(null);

        speed = _ridingMob.GetSpeed();
            
        _ridingMob.StopRunning();
        _ridingMob.RemovePlayerController();

        _ridingMob = null;
        
        _collider.enabled = true;
        _rb.useGravity = true;
            
        _rb.AddForce((transform.up * 2f + transform.forward) * 4f, ForceMode.Impulse);
        playerState = PlayerState.Jumping;
        _beneathCheckCoroutine = StartCoroutine(ActivateBeneathCheckCoroutine());
    }
    
    private void FixedUpdate()
    {
        PlayerMovement();
        LimitVelocity();
    }
    
    private void LimitVelocity()
    {
        var horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > _maxSpeed)
        {
            var clampedHorizontal = horizontalVelocity.normalized * _maxSpeed;
            _rb.linearVelocity = new Vector3(clampedHorizontal.x, _rb.linearVelocity.y, clampedHorizontal.z);
        }
    }

    private void PlayerMovement()
    {
        if (playerState == PlayerState.Jumping || (playerState == PlayerState.Falling && _firstRideDone))
        {
            _rb.MovePosition(_rb.position + transform.forward * (speed * Time.fixedDeltaTime));
        }
    }

    private void DeactivateBeneathCheckCoroutine()
    {
        if (_beneathCheckCoroutine == null) return;
        StopCoroutine(_beneathCheckCoroutine);
        
        if (_decalInstance == null) return;
        Destroy(_decalInstance.gameObject);
    }
    
    private IEnumerator ActivateBeneathCheckCoroutine()
    {
        var yieldReturn = new WaitForSecondsRealtime(0.02f);
        
        _decalInstance = Instantiate(decal);
        
        while (true)
        {
            yield return yieldReturn;
            var hits = Physics.SphereCastAll(mobChecker.position, 2f, Vector3.down, 25f, decalMask);
            
            var sortedHits = hits.OrderBy(h => h.distance).ToArray();
            
            var planeHit = sortedHits.FirstOrDefault(t => t.transform.CompareTag("Floor"));

            if (planeHit.transform != null)
            {
                _decalInstance.gameObject.SetActive(true);
                _decalInstance.position = planeHit.point;
            }
            else
            {
                _decalInstance.gameObject.SetActive(false);
            }
            
            if (hits.Any(t => t.transform.CompareTag("Mob")))
            {
                if (_mobInCheck != null)
                {
                    _mobInCheck.GetComponent<MobHandler>().UnhighlightMob();
                }
                
                _mobInCheck = hits.First(t => t.transform.CompareTag("Mob")).transform.gameObject;
                _mobInCheck.GetComponent<MobHandler>().HighlightMob();
                
                Time.timeScale = 0.5f;
            }
            else
            {
                if (_mobInCheck != null)
                {
                    _mobInCheck.GetComponent<MobHandler>().UnhighlightMob();
                }
                
                _mobInCheck = null;
                Time.timeScale = 1f;
            }
            
        }
    }

    private void SetAnimationState()
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                playerAnimator.Play("Idle");
                break;
            
            case PlayerState.Riding:
                playerAnimator.Play("Ninja Idle");
                break;
            
            case PlayerState.Jumping:
                playerAnimator.Play("Jumping");
                break;
            
            case PlayerState.Falling:
                playerAnimator.Play("Falling Idle");
                break;
            
            case PlayerState.Downed:
                playerAnimator.Play("Falling Flat Impact");
                break;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Plane_End"))
        {
            Task1Manager.PlayerTriggeredPlaneEnd();
            other.enabled = false;
        }

        if (!_deathOnce && (other.transform.CompareTag("Floor") || other.transform.CompareTag("Obstacle") || (other.transform.CompareTag("Mob") && _firstRideDone)))
        {
            _deathOnce = true;
            OnPlayerDowned?.Invoke();
        }
        
        if ((other.transform.CompareTag("Mob") || _ridingMob == null) && !_firstRideDone)
        {
            RideMob(other.gameObject);
            _firstRideDone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Mob"))
        {
            Debug.Log("no mob");
            Time.timeScale = 1f;
            _mobInCheck = null;
        }
    }

    public void PlayerCollided()
    {
        _ridingMob = null;
        DOTween.Kill(transform);
        DeactivateBeneathCheckCoroutine();
        transform.SetParent(null);
        _rb.AddForce((transform.up + transform.forward) * 4f, ForceMode.Impulse);
        _rb.useGravity = true;
        _collider.enabled = true;
        playerState = PlayerState.Downed;
    }

    private void TouchGround()
    {
        collisionParticle.SetActive(true);
        DOTween.Kill(transform);
        DeactivateBeneathCheckCoroutine();
        Time.timeScale = 1f;
        _ridingMob = null;
        playerState = PlayerState.Downed;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;

        transform.DOMove(new Vector3(transform.position.x, 0f, transform.position.z), 0.5f);
    }

    private void CaptureMob()
    {
        DeactivateBeneathCheckCoroutine();
        Time.timeScale = 1f;
        RideMob(_mobInCheck);
        _mobInCheck = null;
    }

    private void RideMob(GameObject mob)
    {
        DeactivateBeneathCheckCoroutine();
        playerState = PlayerState.Riding;
        
        _ridingMob = mob.GetComponent<MobHandler>();
        _ridingMob.SetSpeed(speed);
        _ridingMob.UnhighlightMob();
        _ridingMob.SetPlayerController(this);
        
        speed = 0f;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _collider.enabled = false;
        
        transform.SetParent(_ridingMob.playerPoint);
        
        transform.DOLocalMove(Vector3.zero, 1f);
        transform.localPosition = Vector3.zero;
        _ridingMob.OnStartMoving();

        var _mrb = _ridingMob.AddComponent<Rigidbody>();
        _mrb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        playerState = PlayerState.Riding;
    }
}

public enum PlayerState
{
    Idle,
    Riding,
    Jumping,
    Falling,
    Downed
}