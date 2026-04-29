using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a 2D object rotatable using a handle with rotation around pivot point.
/// The object rotates based on mouse/touch drag on the handle.
/// </summary>
public class RotatableObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject handle;  // Reference to the handle object for rotation
    [SerializeField] private GameObject pivot;   // Reference to the pivot point for rotation

    [Header("Rotation Settings")]
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSmoothness = 10f;

    [Header("Handle Settings")]
    [SerializeField] private float handleDetectionRadius = 30f;  // pixels
    [SerializeField] private float handleScale = 1.5f;           // scales when grabbed
    [SerializeField] private Color handleActiveColor = Color.yellow;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private bool checkCollisions = true;

    // Private variables
    private Rigidbody2D rb;
    private bool isRotating = false;
    private float targetRotation;
    private float originalRotation;
    private Collider2D[] objectColliders;
    private Vector3 originalHandleScale;
    private Color originalHandleColor;
    private SpriteRenderer handleRenderer;

    private void Awake()
    {
        // Cache components
        rb = GetComponent<Rigidbody2D>();
        objectColliders = GetComponentsInChildren<Collider2D>();

        // Setup collision layer mask
        collisionLayers = LayerMask.GetMask("collision", "border");

        // Cache handle visuals if it has a renderer
        if (handle != null)
        {
            handleRenderer = handle.GetComponent<SpriteRenderer>();
            if (handleRenderer != null)
            {
                originalHandleColor = handleRenderer.color;
                originalHandleScale = handle.transform.localScale;
            }
        }

        targetRotation = transform.eulerAngles.z;
        originalRotation = targetRotation;

        ValidateSetup();
    }

    private void ValidateSetup()
    {
        if (handle == null)
        {
            Debug.LogError($"RotatableObject '{gameObject.name}' requires a Handle reference!", gameObject);
            enabled = false;
            return;
        }

        if (pivot == null)
        {
            Debug.LogWarning($"RotatableObject '{gameObject.name}' Pivot is not assigned, using self as pivot", gameObject);
        }

        if (rb == null)
        {
            Debug.LogWarning($"RotatableObject '{gameObject.name}' has no Rigidbody2D, adding one", gameObject);
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        if (!gameObject.CompareTag("Rotatable"))
        {
            Debug.LogWarning($"RotatableObject '{gameObject.name}' should have tag 'Rotatable'", gameObject);
        }
    }

    private void Update()
    {
        HandleInput();

        // Smooth rotation application
        if (smoothRotation && isRotating)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0, 0, targetRotation),
                rotationSmoothness * Time.deltaTime
            );
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOnHandle(Input.mousePosition))
            {
                StartDrag();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (isRotating)
            {
                UpdateDrag(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isRotating)
            {
                EndDrag();
            }
        }
    }

    private void StartDrag()
    {
        isRotating = true;
        originalRotation = transform.eulerAngles.z;

        // Disable physics during rotation
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            rb.isKinematic = true;
        }

        // Visual feedback on handle
        HighlightHandle(true);
    }

    private void UpdateDrag(Vector2 pointerPosition)
    {
        // Get handle world position
        Vector3 handleWorldPos = handle.transform.position;
        Vector3 handleScreenPos = Camera.main.WorldToScreenPoint(handleWorldPos);

        // Calculate angle from handle to mouse
        Vector2 mouseDir = (pointerPosition - (Vector2)handleScreenPos).normalized;
        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

        // Adjust for where rotation starts (handle is on the side)
        targetRotation = angle - 90f;

        // Apply rotation immediately or smoothly
        if (!smoothRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, targetRotation);
        }
    }

    private void EndDrag()
    {
        isRotating = false;

        // Check for collisions at the final rotation
        if (checkCollisions && IsCollidingWithBarriers())
        {
            // Revert to original rotation
            Debug.LogWarning($"Cannot rotate '{gameObject.name}' to this angle - collision detected!");
            targetRotation = originalRotation;
            transform.rotation = Quaternion.Euler(0, 0, originalRotation);
        }

        // Reset handle visuals
        HighlightHandle(false);
    }

    private bool IsPointerOnHandle(Vector2 pointerPosition)
    {
        if (handle == null) return false;

        Vector3 handleWorldPos = handle.transform.position;
        Vector3 handleScreenPos = Camera.main.WorldToScreenPoint(handleWorldPos);

        float distance = Vector2.Distance(pointerPosition, handleScreenPos);
        return distance < handleDetectionRadius;
    }

    private void HighlightHandle(bool highlight)
    {
        if (handle == null) return;

        if (handleRenderer != null)
        {
            handleRenderer.color = highlight ? handleActiveColor : originalHandleColor;
        }

        // Scale the handle for visual feedback
        Vector3 targetScale = highlight ?
            originalHandleScale * handleScale :
            originalHandleScale;

        handle.transform.localScale = Vector3.Lerp(
            handle.transform.localScale,
            targetScale,
            10f * Time.deltaTime
        );
    }

    private bool IsCollidingWithBarriers()
    {
        foreach (Collider2D collider in objectColliders)
        {
            // Skip the handle collider
            if (collider.gameObject == handle) continue;
            if (pivot != null && collider.gameObject == pivot) continue;

            List<Collider2D> overlappingColliders = new List<Collider2D>();
            collider.OverlapCollider(new ContactFilter2D().NoFilter(), overlappingColliders);

            foreach (Collider2D other in overlappingColliders)
            {
                // Skip self
                if (other.gameObject == gameObject) continue;

                // Check if colliding object is on collision or border layers
                if (((1 << other.gameObject.layer) & collisionLayers) != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Rotates the object to a specific angle
    /// </summary>
    public void RotateTo(float angle)
    {
        targetRotation = angle;
        if (!smoothRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// Gets current rotation angle in Z axis (0-360)
    /// </summary>
    public float GetRotationAngle()
    {
        return transform.eulerAngles.z;
    }

    /// <summary>
    /// Gets current rotation angle normalized to -180 to 180 range
    /// </summary>
    public float GetRotationAngleNormalized()
    {
        float angle = transform.eulerAngles.z;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    /// <summary>
    /// Allows you to toggle rotation capability
    /// </summary>
    public void SetRotationEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    /// <summary>
    /// Set handle reference at runtime
    /// </summary>
    public void SetHandle(GameObject newHandle)
    {
        handle = newHandle;
        if (handle != null && handleRenderer == null)
        {
            handleRenderer = handle.GetComponent<SpriteRenderer>();
            if (handleRenderer != null)
            {
                originalHandleColor = handleRenderer.color;
                originalHandleScale = handle.transform.localScale;
            }
        }
    }

    /// <summary>
    /// Set pivot reference at runtime
    /// </summary>
    public void SetPivot(GameObject newPivot)
    {
        pivot = newPivot;
    }

    /// <summary>
    /// Toggle collision checking
    /// </summary>
    public void SetCollisionCheckEnabled(bool enabled)
    {
        checkCollisions = enabled;
    }
}
