using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private Collider _collider;
    private Animator _animator;
    public Transform playerPoint;

    private readonly (float lower, float upper) _speedRange = (4.5f, 6.95f); 
    private float _speed = 7f;
    public float strafeSpeed = 10f;
    
    public bool stopped = true;

    private void Awake()
    {
        
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        
        if (!stopped)
            OnStartMoving();
        
        _speed = Random.Range(_speedRange.lower, _speedRange.upper);
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

    public void OnStartMoving()
    {
        stopped = false;
        _animator.Play("Run");
    }
    
    private void FixedUpdate()
    {
        if (stopped) return;
        
        var move = new Vector3(0, 0, transform.forward.z).normalized;
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

        if (other.transform.CompareTag("DeleteTrigger"))
        {
            
            Destroy(gameObject);
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
            _animator.Play("Death");
        }
    }
}
