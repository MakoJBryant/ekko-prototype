using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlanetGravityController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public float gravityAlignSpeed = 10f;
    public float gravityStrengthMultiplier = 1f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Jump Timing")]
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Sprinting")]
    public float sprintMultiplier = 1.8f;

    private Rigidbody rb;
    private Vector3 gravityUp;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;

    private Camera playerCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 0.5f;

        playerCam = Camera.main;
    }

    void Update()
    {
        UpdateGravityDirection();
        AlignToGravity();

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            Jump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        jumpBufferTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        GroundCheck();
        MovePlayer();
    }

    void UpdateGravityDirection()
    {
        Vector3 gravityForce = GravityManager.GetGravityAtPoint(transform.position, out GravityManager source);

        if (source != null)
        {
            gravityUp = -gravityForce.normalized;
        }
        else
        {
            gravityUp = Vector3.up; // fallback
        }
    }

    void AlignToGravity()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, gravityAlignSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        Vector3 gravityForce = GravityManager.GetGravityAtPoint(transform.position, out GravityManager source);
        rb.AddForce(gravityForce * gravityStrengthMultiplier, ForceMode.Acceleration);
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, -gravityUp, out RaycastHit hit, groundCheckDistance, groundMask);
        if (isGrounded)
        {
            float velDot = Vector3.Dot(rb.linearVelocity, -gravityUp);
            if (velDot >= -0.5f)
            {
                coyoteTimer = coyoteTime;
            }
        }
    }

    void MovePlayer()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (input.sqrMagnitude < 0.01f) return;

        Vector3 camForward = Vector3.ProjectOnPlane(playerCam.transform.forward, gravityUp).normalized;
        Vector3 camRight = Vector3.Cross(gravityUp, camForward);
        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        Vector3 verticalVel = Vector3.Project(rb.linearVelocity, gravityUp);
        Vector3 horizontalVel = moveDir * currentSpeed;

        rb.linearVelocity = horizontalVel + verticalVel;
    }

    void Jump()
    {
        Vector3 vel = rb.linearVelocity;
        Vector3 downwardVel = Vector3.Project(vel, -gravityUp);
        if (downwardVel.magnitude > 0)
            vel -= downwardVel;

        vel += -gravityUp * jumpForce;
        rb.linearVelocity = vel;
    }
}
