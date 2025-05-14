using System;
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
    public Animator playerAnimator;
    public GameObject mobChecker;
    
    public PlayerState playerState = PlayerState.Idle;
    
    public float maxSpeed = 5f;
    
    public float speed = 5f;

    public static Action OnPlayerDowned;
    // public bool translateForward;

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
            Invoke(nameof(ActivateBeneathCheck), 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.Space) && _mobInCheck != null)
        {
            CaptureMob();
        }
        
        if (playerState == PlayerState.Jumping)
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
    }

    private void ActivateBeneathCheck()
    {
        mobChecker.SetActive(true);
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
            Time.timeScale = 0.25f;
            _mobInCheck = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Mob"))
        {
            Time.timeScale = 1f;
            _mobInCheck = null;
        }
    }

    public void PlayerCollided()
    {
        transform.SetParent(null);
        Rb.AddForce((transform.up + transform.forward) * 4f, ForceMode.Impulse);
        Rb.useGravity = true;
        Collider.enabled = true;
        playerState = PlayerState.Downed;
    }
    
    public void TouchGround()
    {
        Time.timeScale = 1f;
        _ridingMob = null;
        playerState = PlayerState.Downed;
        Rb.useGravity = false;
        Rb.linearVelocity = Vector3.zero;
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    public void CaptureMob()
    {
        Time.timeScale = 1f;
        mobChecker.SetActive(false);
        RideMob(_mobInCheck);
    }
    
    public void RideMob(GameObject mob)
    {
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
        _ridingMob.stopped = false;

        Collider.enabled = false;
        _ridingMob.AddComponent<Rigidbody>();
        _ridingMob.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
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