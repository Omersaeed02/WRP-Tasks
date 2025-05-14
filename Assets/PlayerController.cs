using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Task1Manager Task1Manager { get; private set; }
    public Collider Collider { get; private set; }
    public Rigidbody Rb { get; private set; }
    
    private MobHandler _ridingMob;

    public Transform mobDeleteTrigger;
    public Transform mobChecker;
    public Transform decal;
    public Animator playerAnimator;
    
    public PlayerState playerState = PlayerState.Idle;
    
    public float maxSpeed = 5f;
    
    public float speed = 7f;

    public static Action OnPlayerDowned;
    // public bool translateForward;

    private Coroutine _beneathCheckCoroutine;
    
    public void SetTaskManager(Task1Manager tm)
    {
        Task1Manager = tm;
    }

    private void Awake()
    {
        Collider = GetComponent<Collider>();
        Rb = GetComponent<Rigidbody>();
        if (Camera.main != null) Camera.main.transform.SetParent(transform);
        playerState = PlayerState.Falling;
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
        SetAnimationState();

        if (Input.GetKeyDown(KeyCode.Space) && _mobInCheck != null)
        {
            CaptureMob();
        }

    }

    private void FixedUpdate()
    {
        PlayerMovement();
        LimitVelocity();
    }
    
    private void LimitVelocity()
    {
        var horizontalVelocity = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            var clampedHorizontal = horizontalVelocity.normalized * maxSpeed;
            Rb.linearVelocity = new Vector3(clampedHorizontal.x, Rb.linearVelocity.y, clampedHorizontal.z);
        }
    }
    
    public void PlayerMovement()
    {
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && playerState == PlayerState.Riding)
        {
            _ridingMob.StrafeMob(1);
        }
        
        if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && playerState == PlayerState.Riding)
        {
            _ridingMob.StrafeMob(-1);
        }
        
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && playerState == PlayerState.Riding)
        {
            transform.SetParent(null);
            
            speed = _ridingMob.GetSpeed();
            _ridingMob.SetSpeed(0);
            _ridingMob.RemovePlayerController();
            
            Collider.enabled = true;
            Rb.useGravity = true;
            Rb.AddForce((transform.up * 2f + transform.forward) * 4f, ForceMode.Impulse);
            playerState = PlayerState.Jumping;
            _beneathCheckCoroutine = StartCoroutine(ActivateBeneathCheckCoroutine());
        }
        
        if (playerState == PlayerState.Jumping || playerState == PlayerState.Falling && _firstRideDone)
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
    }

    private Transform _decalInstance;

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
        
        // var hits = new RaycastHit[4];
        
        _decalInstance = Instantiate(decal);
        
        while (true)
        {
            yield return yieldReturn;
            // Physics.SphereCastNonAlloc(transform.position, 3f, Vector3.down, colliders, out var hit);
            var hits = Physics.SphereCastAll(mobChecker.position, 3f, Vector3.down);

            // hits.ToList().ForEach(t => Debug.Log(t.transform.name));
            
            var planeHit = hits.First(t => t.transform.CompareTag("Floor"));

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
                Debug.Log("Mob Found");
                _mobInCheck = hits.First(t => t.transform.CompareTag("Mob")).transform.gameObject;
                Time.timeScale = 0.5f;
            }
            else
            {
                Debug.Log("No Mob Found");
                _mobInCheck = null;
                Time.timeScale = 1f;
            }
            
            // if (hit.collider != null)
            // {
            //     if (hit.collider.CompareTag("Mob"))
            //     {
            //         Time.timeScale = 0.5f;
            //         break;
            //     }
            //     else
            //     {
            //         Time.timeScale = 1f;
            //     }
            // }
            // else
            // {
            //     Time.timeScale = 1f;
            // }
        }
        // mobChecker.SetActive(true);
        // Time.timeScale = 0.5f;
    }
    
    public void SetAnimationState()
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

    private GameObject _mobInCheck;
    private bool _firstRideDone;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Plane_End"))
        {
            Task1Manager.PlayerTriggeredPlaneEnd();
        }
        
        if (other.transform.CompareTag("Floor"))
        {
            OnPlayerDowned?.Invoke();
        }
        
        if ((other.transform.CompareTag("Mob") || _ridingMob == null) && !_firstRideDone)
        {
            RideMob(other.gameObject);
            _firstRideDone = true;
        }
        
        else if (other.transform.CompareTag("Mob"))
        {
            Debug.Log("mob in check");
            Time.timeScale = 0.4f;
            _mobInCheck = other.gameObject;
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
        DeactivateBeneathCheckCoroutine();
        transform.SetParent(null);
        Rb.AddForce((transform.up + transform.forward) * 4f, ForceMode.Impulse);
        Rb.useGravity = true;
        Collider.enabled = true;
        playerState = PlayerState.Downed;
    }
    
    public void TouchGround()
    {
        DeactivateBeneathCheckCoroutine();
        Time.timeScale = 1f;
        _ridingMob = null;
        playerState = PlayerState.Downed;
        Rb.useGravity = false;
        Rb.linearVelocity = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    public void CaptureMob()
    {
        DeactivateBeneathCheckCoroutine();
        Time.timeScale = 1f;
        // mobChecker.SetActive(false);
        RideMob(_mobInCheck);
        _mobInCheck = null;
    }
    
    public void RideMob(GameObject mob)
    {
        DeactivateBeneathCheckCoroutine();
        playerState = PlayerState.Riding;
        _ridingMob = mob.GetComponent<MobHandler>();
        _ridingMob.SetSpeed(speed);
        _ridingMob.SetPlayerController(this);
        speed = 0f;
        
        Rb.useGravity = false;
        Rb.linearVelocity = Vector3.zero;
        
        transform.SetParent(_ridingMob.playerPoint);
        
        transform.DOLocalMove(Vector3.zero, 1f);
        transform.localPosition = Vector3.zero;
        _ridingMob.OnStartMoving();

        Collider.enabled = false;
        _ridingMob.AddComponent<Rigidbody>();
        _ridingMob.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
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