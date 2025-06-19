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
            if (rb != null)
            {
                carriedTransform = rb.transform;
                carriedRb = rb;

                carriedRb.isKinematic = true;
                carriedRb.useGravity = false;
                carriedRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Change layer to avoid collisions if needed
                carriedTransform.gameObject.layer = LayerMask.NameToLayer("HeldObject");

                // Preserve world scale before parenting
                Vector3 originalScale = carriedTransform.localScale;
                carriedTransform.SetParent(carryPoint, true);
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

        carriedTransform.SetParent(null);

        carriedRb.isKinematic = false;
        carriedRb.useGravity = true;
        carriedTransform.gameObject.layer = LayerMask.NameToLayer("Default");

        carriedRb.linearVelocity = Vector3.zero;
        carriedRb.angularVelocity = Vector3.zero;

        carriedRb.AddForce(playerCamera.transform.forward * dropPushForce, ForceMode.VelocityChange);

        carriedTransform = null;
        carriedRb = null;
    }
}
