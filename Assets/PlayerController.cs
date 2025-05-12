using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Task1Manager _task1Manager;
    private Rigidbody _rb;
    
    public float speed = 5f;
    public float strafeSpeed = 10f;

    public PlayerState playerState = PlayerState.Idle;

    public Animator playerAnimator;

    // public bool translateForward;

    public void SetTaskManager(Task1Manager tm)
    {
        _task1Manager = tm;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        Camera.main.transform.SetParent(transform);
        playerState = PlayerState.Falling;
    }

    private void Update()
    {
        // Move the cube forward along the Z-axis
        SetAnimationState();
        
        PlayerMovement();
    }

    public void PlayerMovement()
    {
        if (playerState == PlayerState.Riding)
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
        else
        {
            Debug.Log("is not riding");
        }
            
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * (strafeSpeed * Time.deltaTime));
        }
        
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * (strafeSpeed * Time.deltaTime));
        }
        
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)|| Input.GetKeyDown(KeyCode.UpArrow))
        {
            // transform.For(Vector3.up * (strafeSpeed * Time.deltaTime));
            _rb.AddForce((transform.up + transform.forward) * 8f, ForceMode.Impulse);
            playerState = PlayerState.Jumping;
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
        if (other.transform.CompareTag("Plane_End"))
        {
            _task1Manager.PlayerTriggeredPlaneEnd();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Floor"))
        {
            playerState = PlayerState.Riding;
        }
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