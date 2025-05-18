using UnityEngine;
using UnityEngine.EventSystems;

public class Task2CameraManager : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float basePanSpeed = 0.01f;  // Renamed to basePanSpeed
    [SerializeField] private float smoothingSpeed = 10.0f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 3.15f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeedX = 0.3f;
    [SerializeField] private float rotationSpeedY = 0.3f;

    // Private variables for transformations
    public Vector3 targetPosition;
    private float currentVerticalAngle;
    private float currentHorizontalAngle;

    // Touch handling variables
    private Vector2 lastTouchPosition;
    private Vector2[] lastTwoTouchPositions = new Vector2[2];

    public bool isCameraLocked;
    
    [Header("Camera Movement Constraints")]
    [SerializeField] public float cameraMinX;
    [SerializeField] public float cameraMaxX;
    [SerializeField] public float cameraMinZ;
    [SerializeField] public float cameraMaxZ;

    
    [SerializeField] private float minPerspectiveZoom = 15f; // Min FOV for perspective
    [SerializeField] private float maxPerspectiveZoom = 60f; // Max FOV for perspective
    [SerializeField] private float referenceFieldOfView = 60f;  // Reference FOV for perspective camera
    
    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() || isCameraLocked) return;
        HandleTouchInput();
        UpdateCameraTransforms();
    }
    
    private void HandleTouchInput()
    {
        switch (Input.touchCount)
        {
            case 1:
            {
                var touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastTouchPosition = touch.position;
                        break;

                    case TouchPhase.Moved:
                        var delta = touch.position - lastTouchPosition;
                        HandlePanning(delta);
                        lastTouchPosition = touch.position;
                        break;
                }

                break;
            }
            case 2:
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);

                // Handle touch begin
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    lastTwoTouchPositions[0] = touch0.position;
                    lastTwoTouchPositions[1] = touch1.position;
                    return;
                }

                var touch0Prev = lastTwoTouchPositions[0];
                var touch1Prev = lastTwoTouchPositions[1];
                var touch0Current = touch0.position;
                var touch1Current = touch1.position;

                var prevMidPoint = (touch0Prev + touch1Prev) / 2;
                var currentMidPoint = (touch0Current + touch1Current) / 2;

                var delta = currentMidPoint - prevMidPoint;

                HandleRotation(delta);
                HandleZoom(touch0Prev, touch1Prev, touch0Current, touch1Current);

                lastTwoTouchPositions[0] = touch0Current;
                lastTwoTouchPositions[1] = touch1Current;
                break;
            }
        }
    }
    
    private void HandlePanning(Vector2 delta)
    {
        // Get zoom-adjusted pan speed
        var adjustedPanSpeed = GetZoomAdjustedPanSpeed();

        // Convert screen delta into world movement
        var right = mainCamera.transform.right; // X direction (horizontal)
        var up = mainCamera.transform.up;      // Z direction (vertical in orthographic mode)

        // Adjust movement direction
        var moveDirection = right * -delta.x + up * -delta.y;
        moveDirection.y = 0; // Keep movement in the X-Z plane (ignore Y-axis)

        // Apply movement with zoom-adjusted speed
        targetPosition += moveDirection * adjustedPanSpeed;

        // ✅ Clamp movement within scene boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, cameraMinX, cameraMaxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, cameraMinZ, cameraMaxZ);

    }
    
    private void HandleRotation(Vector2 delta)
    {
        var horizontalRotation = delta.x * rotationSpeedY;
        var verticalRotation = -delta.y * rotationSpeedX;

        currentHorizontalAngle += horizontalRotation;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle + verticalRotation, -45f, 45f);

        transform.rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0f);
    }

    private void HandleZoom(Vector2 touch0Prev, Vector2 touch1Prev, Vector2 touch0Current, Vector2 touch1Current)
    {
        var prevDistance = Vector2.Distance(touch0Prev, touch1Prev);
        var currentDistance = Vector2.Distance(touch0Current, touch1Current);
        var deltaDistance = currentDistance - prevDistance;

        var zoomDelta = deltaDistance * zoomSpeed * Time.deltaTime;

        var newFOV = Mathf.Clamp(Camera.main.fieldOfView - zoomDelta, minPerspectiveZoom, maxPerspectiveZoom);
        Camera.main.fieldOfView = newFOV;

        // 🔥 Ensure camera position stays within constraints
        targetPosition.x = Mathf.Clamp(targetPosition.x, -63f, 63f);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -63f, 63f);
    }
    
    private void UpdateCameraTransforms()
    {
        if (isCameraLocked) return;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothingSpeed);

        // 🔥 Final Clamping (Ensure Camera Stays in Scene)
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, -63f, 63f),
            transform.position.y,
            Mathf.Clamp(transform.position.z, -63f, 63f)
        );
    }
    
    private float GetZoomAdjustedPanSpeed()
    {
        return basePanSpeed * (mainCamera.fieldOfView / referenceFieldOfView);
    }

}
