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

        // StartCoroutine(SetInitialPositionsAfterDelay());
    }

    // private IEnumerator SetInitialPositionsAfterDelay()
    // {
    //     Debug.Log("First rings instantiated");
    //     yield return new WaitForSeconds(0.5f);

    //     if (Rings.Exists(r => r == null))
    //     {
    //         Debug.LogError("[SP_GameManager] One or more rings are not assigned!");
    //         yield break;
    //     }

    //     float cylinderX = ReferenceCylinder.position.x;
    //     float cylinderZ = ReferenceCylinder.position.z;

    //     // Evenly space rings starting from TOP_Y_POSITION
    //     Vector3 pos1 = new Vector3(cylinderX, TOP_Y_POSITION, cylinderZ);
    //     Vector3 pos2 = new Vector3(cylinderX, TOP_Y_POSITION - RING_SPACING, cylinderZ);
    //     Vector3 pos3 = new Vector3(cylinderX, TOP_Y_POSITION - 2 * RING_SPACING, cylinderZ);
    //     Vector3 pos4 = new Vector3(cylinderX, TOP_Y_POSITION - 3 * RING_SPACING, cylinderZ);
    //     Vector3 pos5 = new Vector3(cylinderX, TOP_Y_POSITION - 4 * RING_SPACING, cylinderZ);

    //     SetInitialRingPositions(pos1, pos2, pos3, pos4, pos5);

    //     _newPosition = TOP_Y_POSITION - 5 * RING_SPACING; // Set _newPosition to below the lowest ring
    // }

    // public void SetInitialRingPositions(Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 pos4, Vector3 pos5)
    // {
    //     if (Ring1 != null)
    //         Ring1.transform.position = pos1;
    //     else
    //         Debug.LogError("[SP_GameManager] Ring1 is not assigned!");
        
    //     if (Ring2 != null)
    //         Ring2.transform.position = pos2;
    //     else
    //         Debug.LogError("[SP_GameManager] Ring2 is not assigned!");
        
    //     if (Ring3 != null)
    //         Ring3.transform.position = pos3;
    //     else
    //         Debug.LogError("[SP_GameManager] Ring3 is not assigned!");
        
    //     if (Ring4 != null)
    //         Ring4.transform.position = pos4;
    //     else
    //         Debug.LogError("[SP_GameManager] Ring4 is not assigned!");
        
    //     if (Ring5 != null)
    //         Ring5.transform.position = pos5;
    //     else
    //         Debug.LogError("[SP_GameManager] Ring5 is not assigned!");
    // }

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
                    Debug.Log($"[SP_GameManager] Repositioned Ring {i + 1} to {newPos}");
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
            Rings[index].transform.position = position;
        }
        else
        {
            Debug.LogError($"[SP_GameManager] Invalid ring index {index} or ring is null!");
        }
    }
}