using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlanetGravityController : MonoBehaviour
{
    [Header("References")]
    public Transform gravityCenter;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public float groundCheckDistance = 0.35f;
    public LayerMask groundMask;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    private Rigidbody rb;
    private Vector3 gravityUp;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        cam = Camera.main;
    }

    void Update()
    {
        if (!gravityCenter) return;

        gravityUp = (transform.position - gravityCenter.position).normalized;

        // Handle Jump Input
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            PerformJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        if (coyoteTimer > 0) coyoteTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!gravityCenter) return;

        ApplyGravity();
        CheckGround();
        HandleMovement();
    }

    void ApplyGravity()
    {
        rb.AddForce(-gravityUp * 20f, ForceMode.Acceleration);
        AlignToGravity();
    }

    void AlignToGravity()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
    }

    void CheckGround()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position - gravityUp * 0.1f;  // Offset downwards slightly to avoid detecting self
        isGrounded = Physics.Raycast(rayOrigin, -gravityUp, out hit, groundCheckDistance, groundMask);

        if (isGrounded)
        {
            float dot = Vector3.Dot(rb.linearVelocity, -gravityUp);
            if (dot >= -0.5f)
                coyoteTimer = coyoteTime;
        }
    }

    void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, gravityUp).normalized;
        Vector3 camRight = Vector3.Cross(gravityUp, camForward);
        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        Vector3 verticalVel = Vector3.Project(rb.linearVelocity, gravityUp);
        Vector3 horizontalVel = moveDir * speed;

        rb.linearVelocity = horizontalVel + verticalVel;
    }

    void PerformJump()
    {
        Vector3 vel = rb.linearVelocity;
        Vector3 downwardVel = Vector3.Project(vel, -gravityUp);
        if (downwardVel.magnitude > 0)
            vel -= downwardVel;

        vel += gravityUp * jumpForce;
        rb.linearVelocity = vel;
    }
}
