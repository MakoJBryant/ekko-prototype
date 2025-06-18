using UnityEngine;

public class SphericalCameraRig : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // The player to follow
    public Transform gravityCenter;     // Planet center (usually GravityManager object)
    public Transform pitchPivot;        // The camera pivot holding the actual Camera component

    [Header("Camera Control")]
    public float yawSpeed = 240f;       // Increased for snappier rotation
    public float pitchSpeed = 180f;     // Increased for snappier rotation
    public float minPitch = -60f;
    public float maxPitch = 60f;
    public float mouseSensitivity = 1f; // Optional sensitivity multiplier

    [Header("Smoothing")]
    public float upSmoothing = 12f;     // Reduced smoothing for quicker up vector follow

    private float yaw;
    private float pitch = 20f;
    private Vector3 smoothedUp = Vector3.up;

    void LateUpdate()
    {
        if (!player || !gravityCenter || !pitchPivot) return;

        // Step 1: Calculate 'up' based on gravity
        Vector3 targetUp = (player.position - gravityCenter.position).normalized;
        smoothedUp = Vector3.Slerp(smoothedUp, targetUp, upSmoothing * Time.deltaTime);

        // Step 2: Place rig at player position, aligned to smoothed 'up'
        transform.position = player.position;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, smoothedUp), smoothedUp);

        // Step 3: Apply yaw around up
        yaw += Input.GetAxis("Mouse X") * yawSpeed * mouseSensitivity * Time.deltaTime;
        Quaternion yawRotation = Quaternion.AngleAxis(yaw, smoothedUp);
        transform.rotation = yawRotation * Quaternion.LookRotation(Vector3.ProjectOnPlane(player.forward, smoothedUp), smoothedUp);

        // Step 4: Apply pitch to pivot
        pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Optional Debug
        Debug.DrawRay(player.position, smoothedUp * 2f, Color.magenta);
        Debug.DrawRay(pitchPivot.position, pitchPivot.forward * 2f, Color.blue);
        Debug.DrawRay(pitchPivot.position, pitchPivot.right * 2f, Color.red);
        Debug.DrawRay(pitchPivot.position, pitchPivot.up * 2f, Color.green);
    }
}
