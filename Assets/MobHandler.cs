using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class MobHandler : MonoBehaviour
{
    public Vector3 vel;
    
    private PlayerController _playerController;
    private Collider _collider;
    private Animator _animator;
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    public Transform playerPoint;

    private readonly (float lower, float upper) _speedRange = (4.5f, 6.95f); 
    private float _speed = 7f;
    public float strafeSpeed = 10f;
    
    public bool stopped = true;

    public Material normalMaterial;
    public Material highlightedMaterial;

    private Rigidbody _rb;
    
    private void Awake()
    {
        
        
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _skinnedMeshRenderer.material = normalMaterial;
        
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
    
    public void StopRunning()
    {
        _speed = 0;
        _animator.Play("Idle");
    }
    
    private void FixedUpdate()
    {
        if (stopped) return;

        TryGetComponent<Rigidbody>(out _rb);
        if (_rb)
            vel = _rb.linearVelocity;
        
        // var move = new Vector3(0, 0, Vector3.forward.z).normalized;
        // transform.Translate(move * (_speed * Time.deltaTime), Space.World);
        transform.Translate(Vector3.forward * (_speed * Time.deltaTime), Space.World);
    }

    public void StrafeMob(int direction)
    {
        Debug.Log(transform.rotation.y);
        switch (direction)
        {
            case 0:
                transform.DORotateQuaternion(Quaternion.Euler(0,0,0), 0.1f);
                break;
            case > 0:
                transform.Translate(Vector3.left * (strafeSpeed * Time.deltaTime));
                if (transform.rotation.y > -0.15)
                {
                    transform.Rotate(transform.up, -2);
                    // transform.DORotateQuaternion(Quaternion.Euler(0,-30,0), 0.1f);
                }
                break;
            case < 0:
                transform.Translate(Vector3.right * (strafeSpeed * Time.deltaTime));
                if (transform.rotation.y < 0.15)
                {
                    transform.Rotate(transform.up, 2);
                    // transform.DORotateQuaternion(Quaternion.Euler(0,30,0), 0.1f);
                }
                // transform.rotation = Quaternion.Euler(0,30,0);
                break;
            
        }
    }

    public void HighlightMob()
    {
        _skinnedMeshRenderer.material = highlightedMaterial;
    }
    
    public void UnhighlightMob()
    {
        _skinnedMeshRenderer.material = normalMaterial;
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
