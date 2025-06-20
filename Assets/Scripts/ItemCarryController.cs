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

    void Update()
    {
        if (carriedTransform != null)
        {
            // Snap position and rotation directly every frame
            carriedTransform.position = carryPoint.position;
            carriedTransform.rotation = carryPoint.rotation;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedTransform == null)
                TryPickup();
            else
                Drop();
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

                // Make kinematic and disable gravity so physics don't affect it
                carriedRb.isKinematic = true;
                carriedRb.useGravity = false;
                carriedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Change layer to avoid player collisions
                carriedTransform.gameObject.layer = LayerMask.NameToLayer("HeldObject");

                // Parent to carry point (keep world position)
                carriedTransform.SetParent(carryPoint, true);

                // Restore scale (in case parenting scales object)
                carriedTransform.localScale = originalScale;

                // Snap to carry point immediately
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

        // Restore physics
        carriedRb.isKinematic = false;
        carriedRb.useGravity = true;

        // Reset layer so it can be picked up again
        carriedTransform.gameObject.layer = LayerMask.NameToLayer("Default");

        // Reset velocities to avoid unwanted movement
        carriedRb.linearVelocity = Vector3.zero;
        carriedRb.angularVelocity = Vector3.zero;

        // Add a small push forward on drop
        carriedRb.AddForce(playerCamera.transform.forward * dropPushForce, ForceMode.VelocityChange);

        // Clear references
        carriedTransform = null;
        carriedRb = null;
    }
}
