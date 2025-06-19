using UnityEngine;

public class SP_AIRotation : MonoBehaviour
{
    public float rotationSpeed = 300f; // Rotation speed in degrees per second
    public float rotationIncrement = 120f; // Fixed rotation step in degrees
    public float pauseDuration = 0.5f; // Pause duration in seconds
    public Transform[] cylinders;
    private float offsetMove = -32f;
    private enum RotationState { Rotating, Paused }
    private RotationState currentState = RotationState.Paused;
    private float pauseTimer = 0f;
    private int direction = 1; // 1 for counterclockwise, -1 for clockwise
    private int rotationCount = 0;
    private float totalRotation = 0f;

    void Start()
    {
        // Initialize cylinder references
        Transform parent = transform.parent;
        cylinders = new Transform[2];
        cylinders[0] = parent?.Find("Cylinder1");
        cylinders[1] = parent?.Find("Cylinder2");

        if (cylinders[0] == null || cylinders[1] == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Failed to find cylinders. Cylinder1: {cylinders[0]}, Cylinder2: {cylinders[1]}");
        }

        // Set initial state
        RandomRotation();
        transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
        rotationCount = 0;
        totalRotation = 0f;
        currentState = RotationState.Paused;
        pauseTimer = 0f;
    }

    void Update()
    {
        if (currentState == RotationState.Rotating)
        {
            // Calculate rotation step
            float step = rotationSpeed * Time.deltaTime;
            if (totalRotation + step >= rotationIncrement)
            {
                // Complete the exact remaining rotation
                float exactStep = rotationIncrement - totalRotation;
                transform.Rotate(0, exactStep * direction, 0);
                totalRotation = rotationIncrement;
                currentState = RotationState.Paused;
                pauseTimer = pauseDuration;
                rotationCount++;

                // Check if a full 360-degree rotation is complete
                if (rotationCount >= 6)
                {
                    direction *= -1; // Reverse direction
                    rotationCount = 0;
                }
            }
            else
            {
                // Continue rotating
                transform.Rotate(0, step * direction, 0);
                totalRotation += step;
            }
        }
        else if (currentState == RotationState.Paused)
        {
            // Handle pause duration
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                totalRotation = 0f;
                currentState = RotationState.Rotating;
            }
        }
    }

    public void RandomRotation()
    {
        // Randomly rotate cylinders
        for (int i = 0; i < cylinders.Length; i++)
        {
            if (cylinders[i] != null)
            {
                cylinders[i].rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }
    }

    public void Reposition(float yPos)
    {
        // Reposition helix and cylinders
        transform.position = new Vector3(transform.position.x, transform.position.y + yPos, transform.position.z);

        if (cylinders[0] != null && cylinders[1] != null)
        {
            Vector3 newPos1 = cylinders[0].position + new Vector3(0, yPos, 0);
            Vector3 newPos2 = cylinders[1].position + new Vector3(0, yPos, 0);

            cylinders[0].position = newPos1;
            cylinders[1].position = newPos2;
        }
        else
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Cylinders not found during Reposition");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle collision with another helix
        SP_AIRotation otherHelix = other.transform.root.GetComponentInChildren<SP_AIRotation>();
        SP_HelixTowerRotation otherPlayerHelix = other.transform.root.GetComponentInChildren<SP_HelixTowerRotation>();
        if ((otherHelix != null && otherHelix != this) || otherPlayerHelix != null)
        {
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Repositioning triggered due to collision with another helix.");
            Reposition(offsetMove);
            RandomRotation();
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            direction = Random.Range(0, 2) == 0 ? 1 : -1;
            rotationCount = 0;
            totalRotation = 0f;
            currentState = RotationState.Paused;
            pauseTimer = pauseDuration;
        }
    }
}