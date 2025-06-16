using System.Collections.Generic;
using Fusion;
using UnityEngine;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject Ring1, Ring2, Ring3, Ring4, Ring5;
    [SerializeField] private GameObject Ball;
    [SerializeField] private Transform ReferenceCylinder;
    [Networked] public string PlayerId { get; set; }

    private List<GameObject> Rings = new List<GameObject>();
    private float _newPosition = -10f;

    public override void Spawned()
    {
        Rings.AddRange(new GameObject[] { Ring1, Ring2, Ring3, Ring4, Ring5 });

        if (Object.HasStateAuthority)
        {
            if (ReferenceCylinder == null)
            {
                Debug.LogError($"[GameManager] ReferenceCylinder is not set for {PlayerId}!");
                return;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (Ball != null)
            {
                float ballY = Ball.transform.position.y;
                for (int i = 0; i < Rings.Count; i++)
                {
                    GameObject ring = Rings[i];
                    if (ring != null && ring.transform.position.y > ballY + 3f)
                    {
                        Vector3 newPos = new Vector3(ReferenceCylinder.position.x, _newPosition, ReferenceCylinder.position.z);
                        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                        RPC_SetRingPosition(PlayerId, i, newPos, randomRotation);
                        _newPosition -= 3f;
                    }
                }
            }
            else
            {
                Debug.LogError($"[GameManager] BALL reference is not set for {PlayerId}!");
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetRingPosition(string targetPlayerId, int index, Vector3 position, Quaternion rotation)
    {
        if (PlayerId == targetPlayerId && index >= 0 && index < Rings.Count && Rings[index] != null)
        {
            // Set the rotation and position
            Rings[index].transform.rotation = rotation;
            Rings[index].transform.position = position;

            Debug.Log($"[GameManager] Position and rotation set for PlayerId={PlayerId}");
        }
    }
}