using System;
using UnityEngine;

public class MobHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private Collider _collider;
    
    public Transform playerPoint;

    private float _speed = 7f;
    public float strafeSpeed = 10f;
    
    public bool stopped = true;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void SetPlayerController(PlayerController pc)
    {
        _playerController = pc;
    }

    public void RemovePlayerController()
    {
        _collider.enabled = false;
        _playerController = null;
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            Destroy(rb);
        }
    }

    public void SetSpeed(float s)
    {
        _speed = s;
    }

    public float GetSpeed()
    {
        return _speed;
    }

    private void FixedUpdate()
    {
        if (stopped) return;
        
        Vector3 move = new Vector3(0, 0, transform.forward.z).normalized;
        transform.Translate(move * (_speed * Time.deltaTime), Space.World);
    }

    public void StrafeMob(int direction)
    {
        switch (direction)
        {
            case > 0:
                transform.Translate(Vector3.left * (strafeSpeed * Time.deltaTime));
                break;
            case < 0:
                transform.Translate(Vector3.right * (strafeSpeed * Time.deltaTime));
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Plane_End"))
        {
            _playerController.Task1Manager.PlayerTriggeredPlaneEnd();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Obstacle"))
        {
            _speed = 0;
            var temp = _playerController;
            RemovePlayerController();
            temp.PlayerCollided();
        }
    }
}
