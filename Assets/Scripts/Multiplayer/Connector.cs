using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Fusion.Sockets;
using System;
using Game.Utility;

public class Connector : Singleton<Connector>, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner networkRunner;
    // Public property to expose NetworkRunner
    public NetworkRunner NetworkRunner => networkRunner;
    internal async void ConnectToServer(string sessionName)
    {
        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            Debug.Log("[Connector] Created new NetworkRunner component");
        }
        networkRunner.ProvideInput = true;
        var sceneRef = SceneRef.FromIndex(1);
        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);

        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = Fusion.GameMode.Shared,
            SessionName = sessionName,
            Scene = sceneInfo,
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("Fusion: Game started successfully.");
        }
        else
        {
            Debug.LogError($"Fusion: Failed to start game. Reason: {result.ShutdownReason}");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!runner.IsRunning)
        {
            Debug.Log("[Connector] NetworkRunner not running, skipping input processing.");
            return;
        }

        // Check if GameplayManager is initialized
        if (GameplayManager.Instance == null)
        {
            Debug.LogWarning("[Connector] GameplayManager instance not initialized, skipping input processing.");
            return;
        }

        // Check if LocalPlayerId is set
        if (string.IsNullOrEmpty(GameplayManager.Instance.LocalPlayerId))
        {
            Debug.LogWarning("[Connector] LocalPlayerId not set, skipping input processing.");
            return;
        }

        NetworkInputData inputData = new NetworkInputData();

        // Set playerId based on LocalPlayerId
        if (GameplayManager.Instance.LocalPlayerId == "Player1")
        {
            inputData.playerId = 1;
        }
        else if (GameplayManager.Instance.LocalPlayerId == "Player2")
        {
            inputData.playerId = 2;
        }
        else
        {
            Debug.LogError($"[Connector] Invalid LocalPlayerId: {GameplayManager.Instance.LocalPlayerId}");
            return;
        }

        // Use arrow keys for both players
        float rotationInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotationInput = -1f; // Rotate left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rotationInput = 1f; // Rotate right
        }

        inputData.rotationDeltaX = rotationInput;
        if (rotationInput != 0)
        {
            Debug.Log($"[Connector] Input collected for Player {inputData.playerId}: rotationDeltaX={inputData.rotationDeltaX}");
        }

        input.Set(inputData);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log($"Photon Callback - Connected to server");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Photon Callback - Connect failed: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log($"Photon Callback - Connect request: {request}");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log($"Photon Callback - Custom authentication response: {data}");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Photon Callback - Disconnected from server: {reason}");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log($"Photon Callback - Host migration: {hostMigrationToken}");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log($"Photon Callback - Input missing for player {player}: {input}");
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log($"Photon Callback - Object entered AOI: {obj} for player {player}");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        Debug.Log($"Photon Callback - Object exited AOI: {obj} for player {player}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Connector] Player joined: {player}, Total players: {runner.ActivePlayers.Count()}, LocalPlayer: {runner.LocalPlayer}");
        if (player == runner.LocalPlayer)
        {
            // Delegate spawning to GameplayManager
            StartCoroutine(GameplayManager.Instance.SpawnPlayer(runner));
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Photon Callback - Player left: {player}");
        IFrameBridge.Instance.PostMatchAbort("Player left the game", "", "");
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        Debug.Log($"Photon Callback - Reliable data progress: {progress} for player {player}");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        Debug.Log($"Photon Callback - Reliable data received for player {player}: {data}");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log($"Photon Callback - Scene load done");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log($"Photon Callback - Scene load start");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Photon Callback - Session list updated");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Photon Callback - Shutdown: {shutdownReason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log($"Photon Callback - User simulation message: {message}");
    }
}