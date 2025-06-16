using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BaseControl : NetworkBehaviour
{
    public float speed = 10;
    public float offsetMove = -32;
    [Networked] public string PlayerId { get; set; }
    [Networked] public bool isPlayer2 { get; set; }
    public bool isAI = false;
    public Transform[] cylinders;

    private float aiTargetRotation = 0f;
    private float aiRotationTimer = 0f;
    private float aiDecisionInterval = 1f;

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

        // Log initial tags
        Transform root = transform.parent != null ? transform.parent : transform;
        Transform ball = root.Find("Ball");
        Transform camera = root.Find("Camera");
        Debug.Log($"[BaseControl] Spawned {PlayerId}, Root Tag: {root.gameObject.tag}, Ball Tag: {ball?.gameObject.tag}, BaseControl Tag: {gameObject.tag}, Camera Tag: {camera?.gameObject.tag}, InputAuthority: {(Object != null ? Object.InputAuthority.ToString() : "null")}");

        // Fallback: Set tags locally if not already set
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
                Debug.Log($"[BaseControl] Input processed for {PlayerId}: rotationDeltaX={input.rotationDeltaX}");
                transform.Rotate(new Vector3(0, input.rotationDeltaX * -1, 0) * speed * Runner.DeltaTime);
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
        float rotationStep = speed * Runner.DeltaTime;
        transform.Rotate(new Vector3(0, aiTargetRotation * rotationStep, 0));
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
            cylinders[i].rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
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
        foreach (var cylinder in cylinders)
        {
            Debug.Log($"[BaseControl] Cylinder layer after reposition: {LayerMask.LayerToName(cylinder.gameObject.layer)}");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SetPlayerProperties(string playerId, RpcInfo info = default)
    {
        // Set networked PlayerId
        PlayerId = playerId;

        // Set tags
        SetTagsLocally(playerId);

        // Log for debugging
        Transform root = transform.parent != null ? transform.parent : transform;
        NetworkObject networkObject = root.GetComponent<NetworkObject>();
        Debug.Log($"[BaseControl] RPC_SetPlayerProperties called for {playerId}, Source: {info.Source}, Root.Tag: {root.gameObject.tag}, Ball.Tag: {root.Find("Ball")?.tag}, BaseControl.Tag: {root.Find("BaseControl")?.tag}, Camera.Tag: {root.Find("Camera")?.tag}, InputAuthority: {(networkObject != null && networkObject.InputAuthority != PlayerRef.None ? playerId : "none")}");
    }

    public void SetTagsLocally(string playerId)
    {
        Transform root = transform.parent != null ? transform.parent : transform;
        Transform ball = root.Find("Ball");
        Transform baseControl = root.Find("BaseControl");
        Transform camera = root.Find("Camera");

        root.gameObject.tag = playerId;
        if (ball != null) ball.tag = playerId;
        else Debug.LogWarning($"[BaseControl] Ball not found for {playerId}!");
        if (baseControl != null) baseControl.tag = playerId;
        else Debug.LogWarning($"[BaseControl] BaseControl not found for {playerId}!");
        if (camera != null) camera.tag = playerId;
        else Debug.LogWarning($"[BaseControl] Camera not found for {playerId}!");
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseControl otherBase = other.transform.root.GetComponentInChildren<BaseControl>();
        Debug.Log($"[BaseControl] TriggerEnter: Collider tag={other.tag}, Parent PlayerId={(otherBase != null ? otherBase.PlayerId : "null")}");
        if (otherBase != null && (otherBase.PlayerId == "Player1" || otherBase.PlayerId == "Player2"))
        {
            Debug.Log($"[BaseControl] Repositioning triggered for {PlayerId} due to collision with {otherBase.PlayerId}");
            if (Object == null || Runner == null)
            {
                // Non-networked mode (single-player)
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