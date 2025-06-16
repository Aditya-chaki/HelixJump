using System.Collections.Generic;
using UnityEngine;

public class HelixControl : MonoBehaviour
{
    [SerializeField] private List<GameObject> rings = new List<GameObject>();
    [SerializeField] private GameObject ball;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float cylinderShiftOffset = 0.1f;
    [SerializeField] private GameObject colliderBelowR3;
    [SerializeField] private GameObject colliderBelowR6;

    private float ringSpacing;
    private bool hasShiftedBelowR3 = false;

    private enum PlatformType { Platform, Trap, Invisible }

    void Start()
    {
        // Auto-populate rings if not assigned
        if (rings.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name.StartsWith("RING"))
                {
                    rings.Add(child);
                }
            }
        }

        if (rings.Count != 6)
        {
            Debug.LogError("Exactly 6 rings are required.");
            return;
        }

        // Calculate ring spacing
        ringSpacing = rings[0].transform.localPosition.y - rings[1].transform.localPosition.y;

        // Initial randomization
        foreach (GameObject ring in rings)
        {
            RandomizeRingPlatforms(ring);
        }

        // Ensure colliders are set up
        if (!colliderBelowR3 || !colliderBelowR6)
        {
            Debug.LogError("Please assign both colliders in the Inspector.");
        }
        else
        {
            colliderBelowR3.GetComponent<Collider>().isTrigger = true;
            colliderBelowR6.GetComponent<Collider>().isTrigger = true;
        }
    }

    void Update()
    {
        // Rotate cylinder with A/D keys
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            rotationInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotationInput = 1f;
        }
        transform.Rotate(0f, rotationInput * rotationSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ball)
        {
            if (other.transform.position.y < colliderBelowR3.transform.position.y && !hasShiftedBelowR3)
            {
                ShiftBelowR3();
                hasShiftedBelowR3 = true;
            }
            else if (other.transform.position.y < colliderBelowR6.transform.position.y && hasShiftedBelowR3)
            {
                ShiftBelowR6();
                hasShiftedBelowR3 = false;
            }
        }
    }

    void ShiftBelowR3()
    {
        // Current order: R1, R2, R3, R4, R5, R6
        // Target order: R4, R5, R6, R1, R2, R3
        Vector3[] initialPositions = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            initialPositions[i] = rings[i].transform.localPosition;
        }

        // Reposition rings
        rings[0].transform.localPosition = initialPositions[3]; // R1 to R4's position
        rings[1].transform.localPosition = initialPositions[4]; // R2 to R5's position
        rings[2].transform.localPosition = initialPositions[5]; // R3 to R6's position
        rings[3].transform.localPosition = initialPositions[0]; // R4 to R1's position
        rings[4].transform.localPosition = initialPositions[1]; // R5 to R2's position
        rings[5].transform.localPosition = initialPositions[2]; // R6 to R3's position

        // Reorder list
        List<GameObject> newRings = new List<GameObject> { rings[3], rings[4], rings[5], rings[0], rings[1], rings[2] };
        rings = newRings;

        // Randomize shifted rings (R1, R2, R3 now at bottom)
        RandomizeRingPlatforms(rings[3]);
        RandomizeRingPlatforms(rings[4]);
        RandomizeRingPlatforms(rings[5]);

        // Shift cylinder down
        transform.position = new Vector3(transform.position.x, transform.position.y - cylinderShiftOffset, transform.position.z);
    }

    void ShiftBelowR6()
    {
        // Current order: R4, R5, R6, R1, R2, R3
        // Target order: R1, R2, R3, R4, R5, R6
        Vector3[] initialPositions = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            initialPositions[i] = rings[i].transform.localPosition;
        }

        // Reposition rings
        rings[0].transform.localPosition = initialPositions[3]; // R4 to R1's position
        rings[1].transform.localPosition = initialPositions[4]; // R5 to R2's position
        rings[2].transform.localPosition = initialPositions[5]; // R6 to R3's position
        rings[3].transform.localPosition = initialPositions[0]; // R1 to R4's position
        rings[4].transform.localPosition = initialPositions[1]; // R2 to R5's position
        rings[5].transform.localPosition = initialPositions[2]; // R3 to R6's position

        // Reorder list
        List<GameObject> newRings = new List<GameObject> { rings[3], rings[4], rings[5], rings[0], rings[1], rings[2] };
        rings = newRings;

        // Randomize shifted rings (R4, R5, R6 now at bottom)
        RandomizeRingPlatforms(rings[3]);
        RandomizeRingPlatforms(rings[4]);
        RandomizeRingPlatforms(rings[5]);

        // Shift cylinder down
        transform.position = new Vector3(transform.position.x, transform.position.y - cylinderShiftOffset, transform.position.z);
    }

    void RandomizeRingPlatforms(GameObject ring)
    {
        if (ring.transform.childCount != 8)
        {
            Debug.LogError($"Ring {ring.name} does not have exactly 8 children.");
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            GameObject platform = ring.transform.GetChild(i).gameObject;
            PlatformType type = GetRandomPlatformType();
            switch (type)
            {
                case PlatformType.Platform:
                    platform.SetActive(true);
                    // Add platform behavior if needed
                    break;
                case PlatformType.Trap:
                    platform.SetActive(true);
                    // Add trap behavior if needed
                    break;
                case PlatformType.Invisible:
                    platform.SetActive(false);
                    break;
            }
        }
    }

    PlatformType GetRandomPlatformType()
    {
        float rand = Random.Range(0f, 1f);
        if (rand < 0.6f) return PlatformType.Platform;
        else if (rand < 0.8f) return PlatformType.Trap;
        else return PlatformType.Invisible;
    }
}