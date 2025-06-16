using UnityEngine;
using Fusion;

public class HelixTowerRotation : NetworkBehaviour
{
    public float rotationSpeed = 100f;
    [Networked] public string PlayerId { get; set; }
    [Networked] public bool isPlayer2 { get; set; }
    public bool isAI = false;
    public Transform[] cylinders; // Reference to Cylinder1 and Cylinder2

    private float aiTargetRotation = 0f;
    private float aiRotationTimer = 0f;
    private float aiDecisionInterval = 1f;
    private float offsetMove = -32f;

    void Start()
    {
        // Initialize cylinders array by finding Cylinder1 and Cylinder2 in the parent hierarchy
        Transform parent = transform.parent;
        cylinders = new Transform[2];
        cylinders[0] = parent?.Find("Cylinder1");
        cylinders[1] = parent?.Find("Cylinder2");

        if (cylinders[0] == null || cylinders[1] == null)
        {
            Debug.LogError($"[HelixTowerRotation] Failed to find cylinders for {PlayerId}. Cylinder1: {cylinders[0]}, Cylinder2: {cylinders[1]}");
        }
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            int initialSeed = UnityEngine.Random.Range(0, int.MaxValue);
            RPC_RandomRotation(initialSeed);
        }

        if (isAI && isPlayer2)
        {
            SetAITargetRotation();
        }

        if (!string.IsNullOrEmpty(PlayerId))
        {
            SetTagsLocally(PlayerId);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (isAI && isPlayer2)
        {
            AIRotate();
        }
        else if (GetInput(out NetworkInputData input))
        {
            if ((input.playerId == 1 && !isPlayer2) || (input.playerId == 2 && isPlayer2))
            {
                transform.Rotate(0, input.rotationDeltaX * -1 * rotationSpeed * Runner.DeltaTime, 0);
            }
        }
    }

    void AIRotate()
    {
        aiRotationTimer += Runner.DeltaTime;
        if (aiRotationTimer >= aiDecisionInterval)
        {
            SetAITargetRotation();
            aiRotationTimer = 0f;
        }
        float rotationStep = rotationSpeed * Runner.DeltaTime;
        transform.Rotate(0, aiTargetRotation * rotationStep, 0);
    }

    void SetAITargetRotation()
    {
        aiTargetRotation = Random.Range(-1f, 1f);
        aiDecisionInterval = Random.Range(0.5f, 1.5f);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RandomRotation(int seed)
    {
        var oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed);
        RandomRotation();
        UnityEngine.Random.state = oldState;
    }

    public void RandomRotation()
    {
        for (int i = 0; i < cylinders.Length; i++)
        {
            if (cylinders[i] != null)
            {
                cylinders[i].rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestReposition(float yPos)
    {
        if (Object.HasStateAuthority)
        {
            Reposition(yPos);
            int newSeed = UnityEngine.Random.Range(0, int.MaxValue);
            RPC_RandomRotation(newSeed);
        }
    }

    public void Reposition(float yPos)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + yPos, transform.position.z);
        
        if (cylinders[0] != null && cylinders[1] != null)
        {
            Vector3 newPos1 = cylinders[0].position + new Vector3(0, yPos, 0);
            Vector3 newPos2 = cylinders[1].position + new Vector3(0, yPos, 0);
            
            if (Runner != null && Object.HasStateAuthority)
            {
                RPC_SetCylinderPositions(newPos1, newPos2);
            }
            else
            {
                cylinders[0].position = newPos1;
                cylinders[1].position = newPos2;
            }
        }
        else
        {
            Debug.LogWarning("[HelixTowerRotation] Cylinders not found during Reposition");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetCylinderPositions(Vector3 pos1, Vector3 pos2)
    {
        if (cylinders[0] != null)
        {
            cylinders[0].position = pos1;
        }
        if (cylinders[1] != null)
        {
            cylinders[1].position = pos2;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetPlayerProperties(string playerId, RpcInfo info = default)
    {
        PlayerId = playerId;
        SetTagsLocally(playerId);
    }

    public void SetTagsLocally(string playerId)
    {
        Transform root = transform.parent != null ? transform.parent : transform;
        Transform ball = root.Find("BALL");
        Transform camera = root.Find("Camera");
        Transform cylinder1 = root.Find("Cylinder1");
        Transform cylinder2 = root.Find("Cylinder2");
        Transform rings = root.Find("RINGS");

        root.gameObject.tag = playerId;
        if (ball != null) ball.tag = playerId;
        else Debug.LogWarning($"[HelixTowerRotation] BALL not found for {playerId}!");
        if (camera != null) camera.tag = playerId;
        else Debug.LogWarning($"[HelixTowerRotation] Camera not found for {playerId}!");
        if (cylinder1 != null) cylinder1.tag = playerId;
        else Debug.LogWarning($"[HelixTowerRotation] Cylinder1 not found for {playerId}!");
        if (cylinder2 != null) cylinder2.tag = playerId;
        else Debug.LogWarning($"[HelixTowerRotation] Cylinder2 not found for {playerId}!");
        if (rings != null)
        {
            rings.tag = playerId;
            foreach (Transform child in rings.transform)
            {
                child.gameObject.tag = playerId;
            }
        }
        else
        {
            Debug.LogWarning($"[HelixTowerRotation] RINGS not found for {playerId}!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HelixTowerRotation otherHelix = other.transform.root.GetComponentInChildren<HelixTowerRotation>();
        if (otherHelix != null && (otherHelix.PlayerId == "Player1" || otherHelix.PlayerId == "Player2"))
        {
            Debug.Log($"[HelixTowerRotation] Repositioning triggered for {PlayerId} due to collision with {otherHelix.PlayerId}");
            if (Object == null || Runner == null)
            {
                Reposition(offsetMove);
                RandomRotation();
            }
            else if (Object.HasStateAuthority)
            {
                Reposition(offsetMove);
                int newSeed = UnityEngine.Random.Range(0, int.MaxValue);
                RPC_RandomRotation(newSeed);
            }
            else
            {
                RPC_RequestReposition(offsetMove);
            }
        }
    }
}