using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform target;
    public float orbitSpeed = 0.01f;
    public float scrollSpeed = 0.01f;
    public float distance = -2f;
    public float heightOffset = 0.95f;
    public float leftOffset = 0.6f;
    
    private float currentRotationY = 0f;
    private bool isRotating = false;
    private Vector2 lastMousePosition;

    void Start()
    {
        UpdateCameraPosition();
    }

    void Update()
    {
        if (target == null) return;
        
        HandleRotation();
        HandleScroll();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                isRotating = true;
                lastMousePosition = Mouse.current.position.ReadValue();
            }
            
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                isRotating = false;
            }
            
            if (isRotating && Mouse.current.rightButton.isPressed)
            {
                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                Vector2 mouseDelta = currentMousePosition - lastMousePosition;
                
                currentRotationY += mouseDelta.x * orbitSpeed * Time.deltaTime;
                lastMousePosition = currentMousePosition;
            }
        }
    }

    void HandleScroll()
    {
        if (Mouse.current != null)
        {
            float rawScrollValue = Mouse.current.scroll.ReadValue().y;

            if (rawScrollValue != 0)
            {
                float normalizedScrollValue = Mathf.Clamp(rawScrollValue, -1, 1);
                currentRotationY += normalizedScrollValue * scrollSpeed;
            }
        }
    }

    void UpdateCameraPosition()
    {
        Vector3 offset = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(0, currentRotationY, 0);
        offset = rotation * offset;
        
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
        
        transform.position -= transform.right * leftOffset;
    }

    // For disabling rotation when not on main screen
    void OnDisable()
    {
        isRotating = false;
    }

    // Called from SessionManager
    public void ResetCamera()
    {
        currentRotationY = 0f; // reset rotation to front
        UpdateCameraPosition();
    }
}