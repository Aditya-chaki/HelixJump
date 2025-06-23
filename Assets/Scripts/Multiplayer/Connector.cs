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
   [SerializeField] private float swipeSensitivity = 0.1f;
    [SerializeField] private float rotationSmoothing = 0.1f; // Controls Lerp speed (0.0f to 1.0f)
    [SerializeField] private int touchDeltaBufferSize = 5; // Number of frames to average touch deltas
    private List<float> touchDeltaBuffer = new List<float>(); // Buffer for averaging touch deltas
    private float smoothedRotationInput = 0f; // Current smoothed rotation value
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

        if (GameplayManager.Instance == null)
        {
            Debug.LogWarning("[Connector] GameplayManager instance not initialized, skipping input processing.");
            return;
        }

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

        float rotationInput = 0f;

        if (IFrameBridge.Instance.IsMobile())
        {
            // Mobile input: handle touch swipe
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0); // Use the first touch
                if (touch.phase == TouchPhase.Moved)
                {
                    // Add current touch delta to buffer
                    float touchDelta = touch.deltaPosition.x * swipeSensitivity;
                    touchDeltaBuffer.Add(touchDelta);

                    // Keep buffer size limited
                    if (touchDeltaBuffer.Count > touchDeltaBufferSize)
                    {
                        touchDeltaBuffer.RemoveAt(0);
                    }

                    // Calculate average touch delta
                    if (touchDeltaBuffer.Count > 0)
                    {
                        rotationInput = touchDeltaBuffer.Average();
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Clear buffer when touch ends to prevent residual input
                    touchDeltaBuffer.Clear();
                    rotationInput = 0f;
                }
            }
            else
            {
                // No touch input, clear buffer
                touchDeltaBuffer.Clear();
                rotationInput = 0f;
            }

            // Smooth the rotation input using Lerp
            smoothedRotationInput = Mathf.Lerp(smoothedRotationInput, rotationInput, rotationSmoothing);
            inputData.rotationDeltaX = smoothedRotationInput;
        }
        else
        {
            // PC input: handle arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rotationInput -= 1f; // Rotate left
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rotationInput += 1f; // Rotate right
            }

            // Smooth the rotation input for PC as well
            smoothedRotationInput = Mathf.Lerp(smoothedRotationInput, rotationInput, rotationSmoothing);
            inputData.rotationDeltaX = smoothedRotationInput;
        }

        if (inputData.rotationDeltaX != 0)
        {
            // Debug.Log($"[Connector] Input collected for Player {inputData.playerId}: rotationDeltaX={inputData.rotationDeltaX}");
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
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connector] Player left: {player}, LocalPlayer: {runner.LocalPlayer}, ActivePlayers: {runner.ActivePlayers.Count()}");

        if (GameplayManager.Instance != null && GameplayManager.Instance.IsGameStarted)
        {
            var remainingPlayers = runner.ActivePlayers.ToList();
            if (remainingPlayers.Count == 1)
            {
                PlayerRef remainingPlayer = remainingPlayers[0];
                HelixTowerRotation[] helixes = UnityEngine.Object.FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None);
                HelixTowerRotation remainingHelix = helixes.FirstOrDefault(h =>
                    h.Object != null && h.Object.InputAuthority == remainingPlayer);

                if (remainingHelix != null)
                {
                    string winnerId = remainingHelix.PlayerId;
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connector] Declaring {winnerId} as the winner.");
                    GameplayManager.Instance.EndGame(winnerId);
                }
                else
                {
                    Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connector] Could not find remaining player's HelixTowerRotation");
                    IFrameBridge.Instance.PostMatchAbort("Error determining winner", "Helix not found", "1020");
                }
            }
            else
            {
                Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connector] Unexpected number of remaining players: {remainingPlayers.Count}");
                IFrameBridge.Instance.PostMatchAbort("Unexpected player count", "", "");
            }
        }
        else
        {
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connector] Player left before game start or GameplayManager not initialized");
            IFrameBridge.Instance.PostMatchAbort("Player left before game start", "", "");
        }
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