using UnityEngine;

public class SP_HelixTowerRotation : MonoBehaviour
{
    public float rotationSpeed = 100f; // Rotation speed for player
    public Transform[] cylinders; // Reference to Cylinder1 and Cylinder2
    private float offsetMove = -32f;
    private float currentRotation;

    void Start()
    {
        // Initialize cylinders array
        Transform parent = transform.parent;
        cylinders = new Transform[2];
        cylinders[0] = parent?.Find("Cylinder1");
        cylinders[1] = parent?.Find("Cylinder2");

        if (cylinders[0] == null || cylinders[1] == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_HelixTowerRotation] Failed to find cylinders. Cylinder1: {cylinders[0]}, Cylinder2: {cylinders[1]}");
        }

        // Set initial random rotation for cylinders
        RandomRotation();
    }

    void Update()
    {
        float rotationInput = Input.GetAxisRaw("Horizontal");
        RotateHelix(rotationInput);
    }

    void RotateHelix(float rotationInput)
    {
        float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotationAmount, 0);
        currentRotation = transform.eulerAngles.y;
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
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_HelixTowerRotation] Cylinders not found during Reposition");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SP_HelixTowerRotation otherHelix = other.transform.root.GetComponentInChildren<SP_HelixTowerRotation>();
        if (otherHelix != null && otherHelix != this)
        {
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_HelixTowerRotation] Repositioning triggered due to collision with another helix.");
            Reposition(offsetMove);
            RandomRotation();
        }
    }
}