using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SP_GameManager : MonoBehaviour
{
    [SerializeField] private GameObject Ring1, Ring2, Ring3, Ring4, Ring5;
    [SerializeField] private GameObject Ball;
    public Transform ReferenceCylinder;

    public List<GameObject> Rings = new List<GameObject>();
    private float _newPosition = -10f;
    private const float RING_SPACING = 3f; // Vertical spacing between rings
    private const float TOP_Y_POSITION = 5f; // Starting y position for the topmost ring

    void Start()
    {
        Rings.AddRange(new GameObject[] { Ring1, Ring2, Ring3, Ring4, Ring5 });

        if (ReferenceCylinder == null)
        {
            Debug.LogError("[SP_GameManager] ReferenceCylinder is not assigned!");
            return;
        }
    }

    void Update()
    {
        if (Ball != null)
        {
            float ballY = Ball.transform.position.y;
            for (int i = 0; i < Rings.Count; i++)
            {
                GameObject ring = Rings[i];
                if (ring != null && ring.transform.position.y > ballY + RING_SPACING)
                {
                    Vector3 newPos = new Vector3(ReferenceCylinder.position.x, _newPosition, ReferenceCylinder.position.z);
                    SetRingPosition(i, newPos);
                    _newPosition -= RING_SPACING; // Update to maintain even spacing
                    // Debug.Log($"[SP_GameManager] Repositioned Ring {i + 1} to {newPos}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[SP_GameManager] Ball is not assigned!");
        }
    }

    public void SetRingPosition(int index, Vector3 position)
    {
        if (index >= 0 && index < Rings.Count && Rings[index] != null)
        {
            // Randomly rotate the ring around Y-axis
            float randomYRotation = Random.Range(0f, 360f);
            Rings[index].transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
            Rings[index].transform.position = position;
            // Debug.Log($"[SP_GameManager] Ring {index + 1} rotated to Y={randomYRotation} degrees");
        }
        else
        {
            Debug.LogError($"[SP_GameManager] Invalid ring index {index} or ring is null!");
        }
    }
}