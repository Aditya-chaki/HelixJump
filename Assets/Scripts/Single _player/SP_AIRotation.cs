using UnityEngine;

public class SP_AIRotation : MonoBehaviour
{
    public float rotationSpeed = 300f; // Base rotation speed (degrees per second)
    public Transform[] cylinders;
    private float aiTargetRotation = 0f;
    private float aiRotationTimer = 0f;
    private float aiDecisionInterval = 0.2f;
    private float offsetMove = -32f;
    private GameObject ball;
    private float currentRotation;
    private SP_GameManager gameManager;
    private GameObject previousRing;
    private float[] cachedPieceAngles;
    private SP_BallMovement ballMovement; // Reference to ball movement script
    private bool isRotatingToTarget = false; // Tracks if AI is rotating to a target angle
    private float targetRotationAngle; // Target angle for the current rotation
    private const float rotationIncrement = 60f; // Fixed rotation step
    private float rotationSmoothingFactor = 0.15f; // Controls smoothness of rotation (lower = smoother)

    void Start()
    {
        Transform parent = transform.parent;
        cylinders = new Transform[2];
        cylinders[0] = parent?.Find("Cylinder1");
        cylinders[1] = parent?.Find("Cylinder2");

        if (cylinders[0] == null || cylinders[1] == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Failed to find cylinders. Cylinder1: {cylinders[0]}, Cylinder2: {cylinders[1]}");
        }

        RandomRotation();

        ball = GameObject.FindGameObjectWithTag("Player2");
        if (ball == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] AI Ball not found!");
        }
        else
        {
            ballMovement = ball.GetComponent<SP_BallMovement>();
            if (ballMovement == null)
            {
                Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] SP_BallMovement component not found on ball!");
            }
        }

        gameManager = FindObjectOfType<SP_GameManager>();
        if (gameManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] SP_GameManager not found!");
        }
    }

    void Update()
    {
        aiRotationTimer += Time.deltaTime;
        if (aiRotationTimer >= aiDecisionInterval)
        {
            SetAITargetRotation();
            aiRotationTimer = 0f;
        }
        RotateHelix();
    }

    void RotateHelix()
    {
        if (!isRotatingToTarget) return;

        // Get the current rotation as a Quaternion
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, targetRotationAngle, 0);

        // Smoothly interpolate towards the target rotation
        transform.rotation = Quaternion.Lerp(currentRot, targetRot, rotationSmoothingFactor);

        currentRotation = transform.eulerAngles.y;

        // Check if the rotation is close enough to the target
        float delta = Mathf.DeltaAngle(currentRotation, targetRotationAngle);
        if (Mathf.Abs(delta) < 0.1f)
        {
            transform.rotation = targetRot;
            currentRotation = targetRotationAngle;
            isRotatingToTarget = false;
            aiTargetRotation = 0f;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Smoothly reached target rotation at {currentRotation:F2}°");
        }
    }

    void SetAITargetRotation()
    {
        if (ball == null || ballMovement == null)
        {
            aiTargetRotation = Random.Range(-1f, 1f);
            aiDecisionInterval = Random.Range(0.02f, 0.05f);
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] AI ball or ball movement not found, using random rotation");
            return;
        }

        Vector3 ballPos = ball.transform.position;
        GameObject currentRing = FindCurrentRing(ballPos.y);
        if (currentRing == null)
        {
            aiTargetRotation = Random.Range(-1f, 1f);
            aiDecisionInterval = Random.Range(0.02f, 0.05f);
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Current ring not found, using random rotation");
            return;
        }

        if (currentRing != previousRing || cachedPieceAngles == null)
        {
            SP_PiecePositioning piecePositioning = currentRing.GetComponent<SP_PiecePositioning>();
            if (piecePositioning == null)
            {
                aiTargetRotation = Random.Range(-1f, 1f);
                aiDecisionInterval = Random.Range(0.02f, 0.05f);
                Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] SP_PiecePositioning not found, using random rotation");
                return;
            }

            cachedPieceAngles = piecePositioning.GetPieceAngles();
            previousRing = currentRing;
        }

        if (cachedPieceAngles.Length < 2)
        {
            aiTargetRotation = Random.Range(-1f, 1f);
            aiDecisionInterval = Random.Range(0.02f, 0.05f);
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Not enough pieces ({cachedPieceAngles.Length}) to calculate gaps, using random rotation");
            return;
        }

        float ballAngle = Mathf.Atan2(ballPos.z, ballPos.x) * Mathf.Rad2Deg;
        if (ballAngle < 0) ballAngle += 360f;

        int n = cachedPieceAngles.Length;
        float minDistance = float.MaxValue;
        float targetGapAngle = 0f;
        bool isInGap = false;

        // Check gaps on the current ring
        for (int i = 0; i < n; i++)
        {
            float start = cachedPieceAngles[i];
            float end = cachedPieceAngles[(i + 1) % n];
            if (end < start) end += 360f;

            // Check if ball is in this gap
            float ballAngleNormalized = ballAngle >= start && ballAngle <= end ? ballAngle : (ballAngle + 360f) % 360f;
            if (ballAngleNormalized >= start && ballAngleNormalized <= end)
            {
                isInGap = true;
                break;
            }

            // Calculate distance to nearest gap edge
            float distanceToStart = Mathf.Min(Mathf.Abs(ballAngle - start), 360f - Mathf.Abs(ballAngle - start));
            float distanceToEnd = Mathf.Min(Mathf.Abs(ballAngle - end), 360f - Mathf.Abs(ballAngle - end));
            float distanceToGap = Mathf.Min(distanceToStart, distanceToEnd);

            if (distanceToGap < minDistance)
            {
                minDistance = distanceToGap;
                targetGapAngle = (distanceToStart < distanceToEnd) ? start : end;
            }
        }

        // Only confirm gap if ball is not colliding with ring pieces
        if (isInGap && !ballMovement.IsCollidingWithRingPiece)
        {
            aiTargetRotation = 0f;
            isRotatingToTarget = false;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Ball is in a true gap on current ring with no collisions, no rotation needed");
        }
        else
        {
            // Calculate the next rotation toward the nearest gap
            float delta = ballAngle - targetGapAngle;
            float shortestDelta = Mathf.Min(Mathf.Abs(delta), 360f - Mathf.Abs(delta));
            float rotationDirection = (delta >= 0 && delta <= 180f) || (delta < 0 && shortestDelta == Mathf.Abs(delta)) ? -1f : 1f;
            targetRotationAngle = (currentRotation + rotationDirection * rotationIncrement) % 360f;
            if (targetRotationAngle < 0) targetRotationAngle += 360f;
            aiTargetRotation = rotationDirection;
            isRotatingToTarget = true;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] {(ballMovement.IsCollidingWithRingPiece ? "Ball colliding, " : "")}rotating {rotationIncrement}° {(rotationDirection > 0 ? "clockwise" : "counter-clockwise")} to {targetRotationAngle:F2}° toward gap at {targetGapAngle:F2}°");
        }

        aiDecisionInterval = Random.Range(0.02f, 0.05f);
    }

    GameObject FindCurrentRing(float ballY)
    {
        if (gameManager == null || gameManager.Rings == null)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] GameManager or Rings list is null!");
            return null;
        }

        GameObject currentRing = null;
        float minDistance = float.MaxValue;

        foreach (GameObject ring in gameManager.Rings)
        {
            if (ring == null) continue;
            float ringY = ring.transform.position.y;
            float distance = Mathf.Abs(ballY - ringY + 0.3f);
            if (distance < minDistance)
            {
                minDistance = distance;
                currentRing = ring;
            }
        }

        return currentRing;
    }

    public void RandomRotation()
    {
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
        SP_AIRotation otherHelix = other.transform.root.GetComponentInChildren<SP_AIRotation>();
        SP_HelixTowerRotation otherPlayerHelix = other.transform.root.GetComponentInChildren<SP_HelixTowerRotation>();
        if ((otherHelix != null && otherHelix != this) || otherPlayerHelix != null)
        {
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_AIRotation] Repositioning triggered due to collision with another helix.");
            Reposition(offsetMove);
            RandomRotation();
        }
    }
}