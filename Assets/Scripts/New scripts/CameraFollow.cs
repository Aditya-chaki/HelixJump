using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform ball; // Reference to the ball's transform
    public float heightOffset = 3f; // Camera height above the ball's average position
    public float lookDownAngle = 30f; // Angle to tilt camera downward
    public float smoothTimeY = 0.5f; // Smoothing time for vertical movement
    private float targetY; // Target Y position for smooth vertical tracking
    private float velocityY; // Velocity for smooth damping in Y
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Set initial rotation to look down at the specified angle
        transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
        // Initialize targetY to ball's starting Y position
        if (ball != null)
            targetY = ball.position.y + heightOffset;
    }

    void LateUpdate()
    {
        if (ball == null) return;

        // Update target Y position only if the ball is significantly below the current target
        // This prevents the camera from following upward bounces
        float ballY = ball.position.y + heightOffset;
        if (ballY < targetY - 0.1f) // Only follow downward movement (with small threshold)
        {
            targetY = ballY;
        }

        // Smoothly interpolate the camera's Y position
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, smoothTimeY);

        // Calculate camera position: follow ball's X and Z directly, use smoothed Y
        Vector3 targetPosition = new Vector3(
            ball.position.x,
            smoothY,
            ball.position.z - heightOffset / Mathf.Tan(lookDownAngle * Mathf.Deg2Rad)
        );

        // Set camera position
        transform.position = targetPosition;

        // Maintain fixed downward angle for stability
        transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
    }
}