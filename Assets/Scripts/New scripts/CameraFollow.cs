using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform ball; // Reference to the ball's transform
    public float heightOffset = 3f; // Camera height above the ball
    public float lookDownAngle = 30f; // Angle to tilt camera downward
    public float smoothTime = 0.3f; // Time for smooth damping
    public Vector2 frameMargin = new Vector2(2f, 2f); // Margin to keep ball within frame
    private Vector3 velocity = Vector3.zero; // Velocity for smooth damping
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Set initial rotation to look down at the specified angle
        transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
    }

    void LateUpdate()
    {
        if (ball == null) return;

        // Get ball's position in viewport coordinates
        Vector3 viewportPos = cam.WorldToViewportPoint(ball.position);

        // Check if ball is outside the frame margins
        bool isOutOfFrame = viewportPos.x < frameMargin.x / cam.pixelWidth || 
                           viewportPos.x > 1f - frameMargin.x / cam.pixelWidth ||
                           viewportPos.y < frameMargin.y / cam.pixelHeight || 
                           viewportPos.y > 1f - frameMargin.y / cam.pixelHeight;

        if (isOutOfFrame || Vector3.Distance(transform.position, ball.position) > 0.1f)
        {
            // Calculate target position (above and centered on ball)
            Vector3 targetPosition = new Vector3(
                ball.position.x,
                ball.position.y + heightOffset,
                ball.position.z - heightOffset / Mathf.Tan(lookDownAngle * Mathf.Deg2Rad)
            );

            // Smoothly move camera to target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );

            // Ensure camera maintains the downward angle
            transform.rotation = Quaternion.Euler(lookDownAngle, 0f, 0f);
        }
    }
}