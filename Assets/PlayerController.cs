using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Task1Manager task1Manager;
    public float speed = 5f;
    public float strafeSpeed = 10f;

    public PlayerState playerState = PlayerState.Idle;
    // public bool traslateForward;
    
    private void Awake()
    {
        Camera.main.transform.SetParent(transform);
    }

    private void Update()
    {
        // Move the cube forward along the Z-axis
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Plane_End"))
        {
            task1Manager.PlayerTriggeredPlaneEnd();
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