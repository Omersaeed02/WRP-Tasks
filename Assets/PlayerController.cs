using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Task1Manager Task1Manager { get; private set; }
    private Collider _collider;
    private Rigidbody _rb;
    private MobHandler _ridingMob;

    public Transform mobDeleteTrigger;
    public Animator playerAnimator;
    
    public PlayerState playerState = PlayerState.Idle;
    
    public float maxSpeed = 5f;
    
    public float speed = 5f;

    // public bool translateForward;

    public void SetTaskManager(Task1Manager tm)
    {
        Task1Manager = tm;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        if (Camera.main != null) Camera.main.transform.SetParent(transform);
        playerState = PlayerState.Falling;
    }

    private void Update()
    {
        // Move the cube forward along the Z-axis
        SetAnimationState();
        
        // PlayerMovement();
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        LimitVelocity();
    }
    
    private void LimitVelocity()
    {
        var horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            var clampedHorizontal = horizontalVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(clampedHorizontal.x, _rb.linearVelocity.y, clampedHorizontal.z);
        }
    }
    
    public void PlayerMovement()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _ridingMob.StrafeMob(1);
        }
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _ridingMob.StrafeMob(-1);
        }
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) && playerState == PlayerState.Riding)
        {
            transform.SetParent(null);
            
            speed = _ridingMob.GetSpeed();
            _ridingMob.SetSpeed(0);
            _ridingMob.RemovePlayerController();
            
            _collider.enabled = true;
            _rb.useGravity = true;
            _rb.AddForce((transform.up * 2f + transform.forward) * 4f, ForceMode.Impulse);
            playerState = PlayerState.Jumping;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            
        }
        
        if (playerState == PlayerState.Jumping)
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
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
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        
        if (other.transform.CompareTag("Plane_End"))
        {
            Task1Manager.PlayerTriggeredPlaneEnd();
        }
        
        if (other.transform.CompareTag("Floor"))
        {
            playerState = PlayerState.Downed;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
        
        if (other.transform.CompareTag("Mob") || _ridingMob == null)
        {
            RideMob(other.gameObject);
        }
    }

    public void RideMob(GameObject mob)
    {
        playerState = PlayerState.Riding;
        _ridingMob = mob.GetComponent<MobHandler>();
        _ridingMob.SetSpeed(speed);
        _ridingMob.SetPlayerController(this);
        speed = 0f;
        
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        
        transform.SetParent(_ridingMob.playerPoint);
        transform.localPosition = Vector3.zero;
        _ridingMob.stopped = false;

        _collider.enabled = false;
        _ridingMob.AddComponent<Rigidbody>();
        _ridingMob.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
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