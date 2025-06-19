using UnityEngine;

public class SphericalCameraRig : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // Should be PlayerRoot (scale 1,1,1)
    public Transform gravityCenter;     // Planet or object exerting gravity
    public Transform pitchPivot;        // Child of camera rig that rotates vertically

    [Header("Camera Control")]
    public float yawSpeed = 240f;
    public float pitchSpeed = 180f;
    public float minPitch = -60f;
    public float maxPitch = 60f;
    public float mouseSensitivity = 1f;

    [Header("Smoothing")]
    public float upSmoothing = 12f;

    private float yaw;
    private float pitch = 20f;
    private Vector3 smoothedUp = Vector3.up;

    void LateUpdate()
    {
        if (!player || !gravityCenter || !pitchPivot) return;

        // STEP 1: Calculate target up vector (planet gravity)
        Vector3 targetUp = (player.position - gravityCenter.position).normalized;
        smoothedUp = Vector3.Slerp(smoothedUp, targetUp, upSmoothing * Time.deltaTime);

        // STEP 2: Follow player position
        transform.position = player.position;

        // STEP 3: Apply yaw around gravity-up
        yaw += Input.GetAxis("Mouse X") * yawSpeed * mouseSensitivity * Time.deltaTime;

        Quaternion yawRotation = Quaternion.AngleAxis(yaw, smoothedUp);
        transform.rotation = yawRotation;

        // STEP 4: Pitch only affects pivot (vertical look)
        pitch -= Input.GetAxis("Mouse Y") * pitchSpeed * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // STEP 5: Align rig's up to planet gravity
        transform.rotation = Quaternion.LookRotation(transform.forward, smoothedUp);

        // Optional Debug
        Debug.DrawRay(player.position, smoothedUp * 2f, Color.magenta);
        Debug.DrawRay(pitchPivot.position, pitchPivot.forward * 2f, Color.blue);
        Debug.DrawRay(pitchPivot.position, pitchPivot.right * 2f, Color.red);
        Debug.DrawRay(pitchPivot.position, pitchPivot.up * 2f, Color.green);
    }
}
