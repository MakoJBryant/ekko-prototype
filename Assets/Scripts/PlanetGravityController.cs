using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetGravityController : MonoBehaviour
{
    [Header("References")]
    public Transform gravityCenter;
    public Transform groundCheckOrigin; // Optional: point this to your model or feet

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

        // Optional default for groundMask
        if (groundMask.value == 0)
        {
            groundMask = LayerMask.GetMask("Ground", "Default");
        }
    }

    void Update()
    {
        if (!gravityCenter) return;

        gravityUp = (transform.position - gravityCenter.position).normalized;

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            PerformJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        jumpBufferTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
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
        Vector3 origin = groundCheckOrigin ? groundCheckOrigin.position : transform.position;
        Vector3 rayOrigin = origin - gravityUp * 0.1f;

        isGrounded = Physics.Raycast(rayOrigin, -gravityUp, out RaycastHit hit, groundCheckDistance, groundMask);

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
        vel -= downwardVel;
        vel += gravityUp * jumpForce;
        rb.linearVelocity = vel;
    }
}
