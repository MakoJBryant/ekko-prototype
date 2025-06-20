using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemCarryController : MonoBehaviour
{
    [Header("References")]
    public Transform carryPoint;
    public Camera playerCamera;
    public float pickupRange = 3f;
    public LayerMask pickupLayer;

    [Header("Physics")]
    public float dropPushForce = 2f;

    private Rigidbody carriedRb;
    private Transform carriedTransform;

    private Vector3 originalScale;

    void Start()
    {
        // Ensure pickupLayer includes Default layer (layer 0)
        if ((pickupLayer.value & (1 << 0)) == 0)
        {
            pickupLayer |= (1 << 0); // Add Default layer
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedTransform == null)
                TryPickup();
            else
                Drop();
        }
    }

    void FixedUpdate()
    {
        if (carriedTransform != null)
        {
            carriedTransform.position = carryPoint.position;
            carriedTransform.rotation = carryPoint.rotation;
        }
    }

    void TryPickup()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayer))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && carriedTransform == null)
            {
                carriedRb = rb;
                carriedTransform = rb.transform;

                // Store original scale before parenting
                originalScale = carriedTransform.localScale;

                // Set physics state
                carriedRb.isKinematic = true;
                carriedRb.useGravity = false;
                carriedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Set layer to avoid re-hit immediately
                carriedTransform.gameObject.layer = LayerMask.NameToLayer("HeldObject");

                // Parent to carry point (retain world position and rotation)
                carriedTransform.SetParent(carryPoint, true);

                // Restore scale (protect against parent scaling)
                carriedTransform.localScale = originalScale;

                // Snap to carry point
                carriedTransform.position = carryPoint.position;
                carriedTransform.rotation = carryPoint.rotation;
            }
        }
    }

    void Drop()
    {
        if (carriedTransform == null) return;

        // Unparent
        carriedTransform.SetParent(null);

        // Restore scale
        carriedTransform.localScale = originalScale;

        // Reset physics
        carriedRb.isKinematic = false;
        carriedRb.useGravity = true;

        // Reset layer so it can be picked up again
        carriedTransform.gameObject.layer = LayerMask.NameToLayer("Default");

        // Clear motion
        carriedRb.linearVelocity = Vector3.zero;
        carriedRb.angularVelocity = Vector3.zero;

        // Add push
        carriedRb.AddForce(playerCamera.transform.forward * dropPushForce, ForceMode.VelocityChange);

        // Clear references
        carriedTransform = null;
        carriedRb = null;
    }
}
