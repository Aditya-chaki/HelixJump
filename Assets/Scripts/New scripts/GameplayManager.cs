using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Linq;
using Game.Utility;

public class GameplayManager : Singleton<GameplayManager>
{
    public ScoreManager scoreManager; // Reference to networked ScoreManager
    public TextMeshProUGUI scoreText;
    public GameObject scoreboardPanel;
    public GameObject GameOver;
    public TextMeshProUGUI leftLabel;  // Label for the left side of the split screen
    public TextMeshProUGUI rightLabel; // Label for the right side of the split screen
    public TextMeshProUGUI endGameText; // Reference to the end game text UI element
    public GameObject waitingForPlayerImage; // Reference to the "Waiting for Player" UI element

    // Progress Bar Sliders
    [SerializeField] private Slider player1Slider; // Slider for Player1's score
    [SerializeField] private Slider player2Slider; // Slider for Player2's score
    private readonly Color localPlayerColor = Color.blue; // Color for local player's handle
    private readonly Color opponentColor = Color.red;     // Color for opponent's handle

    public AudioSource gameAudioSource;     // AudioSource for the game music
    public AudioSource gameOverAudioSource; // AudioSource for the game over sound
    private bool isGameEnded = false;
    private bool isOnePlayerMode = false;
    private bool isGameStarted = false;
    public Transform player1Pos, player2Pos;
    public NetworkObject playerPrefab;

    public string LocalPlayerId { get; private set; }

    private HelixTowerRotation player1Helix;
    private HelixTowerRotation player2Helix;
    private AudioListener audioListener; // Reference to the AudioListener component
    public bool IsGameStarted => isGameStarted;
    // Existing fields...
    private int localPlayer1Score = 0; // Cached Player1 score
    private int localPlayer2Score = 0; // Cached Player2 score

    // Add a method to update local scores
    public void UpdateLocalScores(int player1Score, int player2Score)
    {
        localPlayer1Score = player1Score;
        localPlayer2Score = player2Score;
        
    }

    public override void Awake()
    {
        base.Awake();
        DontDestroyObjectOnLoad = true;

        // Get the AudioListener component and disable it initially
        audioListener = GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] AudioListener disabled at Awake");
        }
        else
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] AudioListener component not found on GameObject!");
        }

        if (player1Pos == null)
        {
            GameObject player1Spawn = new GameObject("Player1Spawn");
            player1Spawn.transform.position = new Vector3(-10, 0, 0);
            player1Pos = player1Spawn.transform;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Created Player1Spawn at (-10, 0, 0)");
        }

        if (player2Pos == null)
        {
            GameObject player2Spawn = new GameObject("Player2Spawn");
            player2Spawn.transform.position = new Vector3(10, 0, 0);
            player2Pos = player2Spawn.transform;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Created Player2Spawn at (10, 0, 0)");
        }
    }

    internal IEnumerator SpawnPlayer(NetworkRunner runner)
    {
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawning Player for LocalPlayer {runner.LocalPlayer}");

        if (player1Pos == null || player2Pos == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player1Pos or Player2Pos is not assigned!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Spawn positions not set", "1014");
            yield break;
        }

        yield return new WaitUntil(() => runner != null && runner.IsRunning);

        if (FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None).Any(p => p.Object != null && p.Object.InputAuthority == runner.LocalPlayer))
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player already spawned for this client!");
            yield break;
        }

        string playerId;
        Vector3 spawnPosition;
        int playerCount = runner.ActivePlayers.Count();

        if (playerCount <= 1)
        {
            playerId = "Player1";
            spawnPosition = player1Pos.position;
        }
        else
        {
            playerId = "Player2";
            spawnPosition = player2Pos.position;
        }

        NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, runner.LocalPlayer);
        if (playerObject == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Failed to spawn {playerId} NetworkObject!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Spawn failed", "1016");
            yield break;
        }

        GameManager gm = playerObject.GetComponentInChildren<GameManager>();
        if (gm != null)
        {
            gm.PlayerId = playerId;
        }
        else
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] GameManager component not found in {playerId} prefab!");
        }

        HelixTowerRotation helix = playerObject.transform.Find("RINGS")?.GetComponent<HelixTowerRotation>();
        if (helix == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] HelixTowerRotation component not found in {playerId} prefab on RINGS!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "HelixTowerRotation not found", "1013");
            runner.Despawn(playerObject);
            yield break;
        }

        helix.PlayerId = playerId;
        helix.isPlayer2 = playerId == "Player2";
        helix.enabled = false;

        LocalPlayerId = playerId;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Set LocalPlayerId to {LocalPlayerId} for runner.LocalPlayer {runner.LocalPlayer}");

        GameObject playerBall = playerObject.transform.Find("BALL")?.gameObject;
        GameObject playerCameraObj = playerObject.transform.Find("Camera")?.gameObject;
        GameObject cylinder1 = playerObject.transform.Find("Cylinder1")?.gameObject;
        GameObject cylinder2 = playerObject.transform.Find("Cylinder2")?.gameObject;
        GameObject rings = playerObject.transform.Find("RINGS")?.gameObject;

        if (playerBall == null || playerCameraObj == null || cylinder1 == null || cylinder2 == null || rings == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] {playerId} missing components: BALL={playerBall}, Camera={playerCameraObj}, Cylinder1={cylinder1}, Cylinder2={cylinder2}, RINGS={rings}");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", $"{playerId} components missing", "1014");
            runner.Despawn(playerObject);
            yield break;
        }

        foreach (Transform child in rings.transform)
        {
            child.gameObject.tag = playerId;
        }

        playerObject.gameObject.tag = playerId;
        playerBall.tag = playerId;
        playerCameraObj.tag = playerId;
        cylinder1.tag = playerId;
        cylinder2.tag = playerId;
        rings.tag = playerId;

        Camera playerCamera = playerCameraObj.GetComponent<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Camera component not found in {playerId} prefab!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Camera not found", "1015");
            runner.Despawn(playerObject);
            yield break;
        }
        playerCamera.rect = playerId == "Player1" ? new Rect(0, 0, 0.5f, 1) : new Rect(0.5f, 0, 0.5f, 1);
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Initial camera setup for {playerId}: Rect={playerCamera.rect}, Camera Tag={playerCameraObj.tag}");

        helix.RPC_SetPlayerProperties(playerId);

        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawned {playerId} at {spawnPosition}, PlayerId: {helix.PlayerId}, isPlayer2: {helix.isPlayer2}, tags: Root={playerObject.tag}, BALL={playerBall.tag}, Camera={playerCameraObj.tag}, InputAuthority: {playerObject.InputAuthority}");

        // Show "Waiting for Player" image if Player 1 spawns
        if (playerId == "Player1" && waitingForPlayerImage != null)
        {
            waitingForPlayerImage.SetActive(true);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Showing waiting for player image for Player1");
        }

        // Set slider handle colors based on LocalPlayerId
        if (player1Slider != null && player2Slider != null)
        {
            Image player1HandleImage = player1Slider.GetComponentInChildren<Image>();
            Image player2HandleImage = player2Slider.GetComponentInChildren<Image>();
            if (player1HandleImage != null && player2HandleImage != null)
            {
                if (playerId == "Player1")
                {
                    player1HandleImage.color = localPlayerColor;
                    player2HandleImage.color = opponentColor;
                }
                else
                {
                    player1HandleImage.color = opponentColor;
                    player2HandleImage.color = localPlayerColor;
                }
            }
        }

        StartCoroutine(WaitForPlayers());
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (Connector.Instance != null && Connector.Instance.NetworkRunner.IsServer)
        {
            GameObject scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<NetworkObject>();
            scoreManager = scoreManagerObj.AddComponent<ScoreManager>();
            Connector.Instance.NetworkRunner.Spawn(scoreManagerObj);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawned ScoreManager");
        }

        if (Connector.Instance != null)
        {
            StartCoroutine(SpawnPlayer(Connector.Instance.NetworkRunner));
        }
        else
        {
            StartGame(true);
        }

        // Validate slider UI elements
        if (player1Slider == null || player2Slider == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Slider UI elements not assigned!");
        }
        else
        {
            // Initialize sliders
            player1Slider.minValue = 0;
            player1Slider.maxValue = scoreManager != null ? scoreManager.scoreToWin : 50;
            player2Slider.minValue = 0;
            player2Slider.maxValue = scoreManager != null ? scoreManager.scoreToWin : 50;
        }
    }

    public void StartGameUI()
    {
        if (scoreboardPanel != null)
        {
            scoreboardPanel.SetActive(true);
            // Rotate scoreboard for Player 2
            if (LocalPlayerId == "Player2")
            {
                RectTransform scoreboardRect = scoreboardPanel.GetComponent<RectTransform>();
                if (scoreboardRect != null)
                {
                    scoreboardRect.rotation = Quaternion.Euler(0, 180, 0);
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Rotated scoreboard 180 degrees for Player2");
                }
            }
            else
            {
                RectTransform scoreboardRect = scoreboardPanel.GetComponent<RectTransform>();
                if (scoreboardRect != null)
                {
                    scoreboardRect.rotation = Quaternion.Euler(0, 0, 0); // Ensure Player1 has no rotation
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Set scoreboard rotation to 0 for Player1");
                }
            }
        }
        if (scoreboardPanel != null) scoreboardPanel.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        // Configure "You" and "Opponent" labels based on the local player
        if (leftLabel != null && rightLabel != null)
        {
            if (LocalPlayerId == "Player1")
            {
                leftLabel.text = "You";
                rightLabel.text = "Opponent";
            }
            else if (LocalPlayerId == "Player2")
            {
                leftLabel.text = "Opponent";
                rightLabel.text = "You";
            }
        }
        else
        {
            Debug.LogError("leftLabel or rightLabel is not assigned!");
        }

        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Game UI started");
    }

    public void StartGame(bool isOnePlayer)
    {
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Starting {(isOnePlayer ? "single-player" : "multiplayer")} game");
        isOnePlayerMode = isOnePlayer;
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        if (isOnePlayer)
        {
            if (scoreManager == null)
            {
                scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager == null)
                {
                    Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] ScoreManager not found in single-player mode!");
                    IFrameBridge.Instance.PostMatchAbort("Game setup failed", "ScoreManager not found", "1017");
                    return;
                }
                if (gameAudioSource != null) gameAudioSource.Play();
            }

            foreach (var helix in FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None))
            {
                Destroy(helix.gameObject);
            }

            SpawnLocalPlayers();
           HelixTowerRotation[] helixes = FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None);
            if (helixes.Length < 2)
            {
                Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Only {helixes.Length} helixes found!");
                IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Insufficient helixes", "1011");
                return;
            }
            player1Helix = helixes.FirstOrDefault(p => p.PlayerId == "Player1");
            player2Helix = helixes.FirstOrDefault(p => p.PlayerId == "Player2");
            if (player1Helix == null || player2Helix == null)
            {
                Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Failed to find Player1 or Player2 helix in single-player mode!");
                IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Helixes not found", "1011");
                return;
            }

            player1Helix.gameObject.SetActive(true);
            player2Helix.gameObject.SetActive(true);
            player1Helix.enabled = true;
            player2Helix.enabled = true;
            player2Helix.isAI = true;

            isGameStarted = true;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Game started in single-player mode");

            // Enable AudioListener for single-player mode
            if (audioListener != null)
            {
                audioListener.enabled = true;
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] AudioListener enabled for single-player mode");
            }

            ConfigureCameras();
            StartGameUI();
            UpdateScoreUI();
            player1Helix.transform.position = new Vector3(-10, 0, 0);
            player2Helix.transform.position = new Vector3(10, 0, 0);
        }
    }

    public void SpawnLocalPlayers()
    {
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawning local players for single-player mode");
        var prefab = Resources.Load<GameObject>("Player2");
        if (prefab == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] PlayerPrefab not found in Resources!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "PlayerPrefab not found", "1012");
            return;
        }

        GameObject player1Obj = Instantiate(prefab, new Vector3(-10, 0, 0), Quaternion.identity);
        player1Obj.tag = "Player1";
        GameObject player1Ball = player1Obj.transform.Find("BALL")?.gameObject;
        GameObject player1CameraObj = player1Obj.transform.Find("Camera")?.gameObject;
        GameObject player1Cylinder1 = player1Obj.transform.Find("Cylinder1")?.gameObject;
        GameObject player1Cylinder2 = player1Obj.transform.Find("Cylinder2")?.gameObject;
        GameObject player1Rings = player1Obj.transform.Find("RINGS")?.gameObject;

        if (player1Ball == null || player1CameraObj == null || player1Cylinder1 == null || player1Cylinder2 == null || player1Rings == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player1 missing components!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Player1 components missing", "1014");
            Destroy(player1Obj);
            return;
        }

        player1Ball.tag = "Player1";
        player1CameraObj.tag = "Player1";
        player1Cylinder1.tag = "Player1";
        player1Cylinder2.tag = "Player1";
        player1Rings.tag = "Player1";

        foreach (Transform child in player1Rings.transform)
        {
            child.gameObject.tag = "Player1";
        }

        HelixTowerRotation player1Ctrl = player1Rings.GetComponent<HelixTowerRotation>();
        player1Ctrl.PlayerId = "Player1";
        player1Ctrl.isPlayer2 = false;
        player1Ctrl.RandomRotation();

        GameObject player2Obj = Instantiate(prefab, new Vector3(10, 0, 0), Quaternion.identity);
        player2Obj.tag = "Player2";
        GameObject player2Ball = player2Obj.transform.Find("BALL")?.gameObject;
        GameObject player2CameraObj = player2Obj.transform.Find("Camera")?.gameObject;
        GameObject player2Cylinder1 = player2Obj.transform.Find("Cylinder1")?.gameObject;
        GameObject player2Cylinder2 = player2Obj.transform.Find("Cylinder2")?.gameObject;
        GameObject player2Rings = player2Obj.transform.Find("RINGS")?.gameObject;

        if (player2Ball == null || player2CameraObj == null || player2Cylinder1 == null || player2Cylinder2 == null || player2Rings == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player2 missing components!");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Player2 components missing", "1014");
            Destroy(player1Obj);
            Destroy(player2Obj);
            return;
        }

        player2Ball.tag = "Player2";
        player2CameraObj.tag = "Player2";
        player2Cylinder1.tag = "Player2";
        player2Cylinder2.tag = "Player2";
        player2Rings.tag = "Player2";

        foreach (Transform child in player2Rings.transform)
        {
            child.gameObject.tag = "Player2";
        }

        HelixTowerRotation player2Ctrl = player2Rings.GetComponent<HelixTowerRotation>();
        player2Ctrl.PlayerId = "Player2";
        player2Ctrl.isPlayer2 = true;
        player2Ctrl.isAI = true;
        player2Ctrl.RandomRotation();

        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawned Player1 at {player1Obj.transform.position}, PlayerId: {player1Ctrl.PlayerId}, tags: Root={player1Obj.tag}, BALL={player1Ball.tag}, Camera={player1CameraObj.tag}");
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Spawned Player2 at {player2Obj.transform.position}, PlayerId: {player2Ctrl.PlayerId}, isAI: {player2Ctrl.isAI}, tags: Root={player2Obj.tag}, BALL={player2Ball.tag}, Camera={player2CameraObj.tag}");

        LocalPlayerId = "Player1";
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Set LocalPlayerId to {LocalPlayerId} for single-player mode");
    }

    public void StartMultiplayerGame()
    {
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Starting multiplayer game");
        if (gameAudioSource != null) gameAudioSource.Play();

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager == null)
            {
                Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] ScoreManager not found in multiplayer mode!");
                IFrameBridge.Instance.PostMatchAbort("Game setup failed", "ScoreManager not found", "1017");
                return;
            }
        }

        HelixTowerRotation[] helixes =FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None);
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Found {helixes.Length} helixes");
        foreach (var h in helixes)
        {
            var ball = h.transform.parent?.Find("BALL")?.gameObject;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Helix for {h.PlayerId}, Position: {h.transform.position}, Tag: {h.gameObject.tag}, Ball Tag: {ball?.tag}, InputAuthority: {(h.Object != null ? h.Object.InputAuthority.ToString() : "null")}");
        }

        player1Helix = helixes.FirstOrDefault(p => p.PlayerId == "Player1");
        player2Helix = helixes.FirstOrDefault(p => p.PlayerId == "Player2");

        if (player1Helix == null || player2Helix == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Failed to find Player1 or Player2 helix! P1: {player1Helix}, P2: {player2Helix}");
            IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Helixes not found", "1011");
            return;
        }

        player1Helix.transform.position = player1Pos.position;
        player2Helix.transform.position = player2Pos.position;

        player1Helix.enabled = true;
        player2Helix.enabled = true;
        isGameStarted = true;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Game started in multiplayer mode");

        // Enable AudioListener for multiplayer mode
        if (audioListener != null)
        {
            audioListener.enabled = true;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] AudioListener enabled for multiplayer mode");
        }

        NetworkRunner runner = Connector.Instance.NetworkRunner;
        if (runner.IsServer)
        {
            runner.SessionInfo.IsOpen = false;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Session closed by host");
        }

        StartGameUI();
        UpdateScoreUI();

        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Multiplayer game started successfully");
    }

    private void ConfigureCameras()
    {
        if (player1Helix != null)
        {
            Camera player1Camera = player1Helix.transform.parent?.Find("Camera")?.GetComponent<Camera>();
            if (player1Camera != null)
            {
                player1Camera.rect = new Rect(0, 0, 0.5f, 1);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Configured camera for Player1: Rect={player1Camera.rect}, Camera Tag={player1Camera.gameObject.tag}, InputAuthority={player1Helix.Object?.InputAuthority}");
            }
            else
            {
                Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player1 camera not found!");
            }
        }

        if (player2Helix != null)
        {
            Camera player2Camera = player2Helix.transform.parent?.Find("Camera")?.GetComponent<Camera>();
            if (player2Camera != null)
            {
                player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Configured camera for Player2: Rect={player2Camera.rect}, Camera Tag={player2Camera.gameObject.tag}, InputAuthority={player2Helix.Object?.InputAuthority}");
            }
            else
            {
                Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player2 camera not found!");
            }
        }
    }

    private IEnumerator WaitForPlayers()
    {
        float timeout = 200f;
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            HelixTowerRotation[] helixes = FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None);
            var player1 = helixes.FirstOrDefault(p => p.PlayerId == "Player1");
            var player2 = helixes.FirstOrDefault(p => p.PlayerId == "Player2");

            if (player1 != null && player1.gameObject.tag == "Untagged")
            {
                Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player1 tags are Untagged, attempting to set locally");
                player1.SetTagsLocally("Player1");
            }
            if (player2 != null && player2.gameObject.tag == "Untagged")
            {
                Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player2 tags are Untagged, attempting to set locally");
                player2.SetTagsLocally("Player2");
            }

            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Waiting for 2 helixes... Found {helixes.Length}, P1: {(player1 != null ? $"Found (PlayerId={player1.PlayerId}, tag={player1.gameObject.tag}, Ball Tag={player1.transform.parent?.Find("BALL")?.tag}, InputAuthority={(player1.Object != null ? player1.Object.InputAuthority.ToString() : "null")})" : "null")}, P2: {(player2 != null ? $"Found (PlayerId={player2.PlayerId}, tag={player2.gameObject.tag}, Ball Tag={player2.transform.parent?.Find("BALL")?.tag}, InputAuthority={(player2.Object != null ? player2.Object.InputAuthority.ToString() : "null")})" : "null")}");

            if (player1 != null && player2 != null && player1.gameObject.tag == "Player1" && player2.gameObject.tag == "Player2")
            {
                player1Helix = player1;
                player2Helix = player2;
                ConfigureCameras();
                StartMultiplayerGame();

                // Hide "Waiting for Player" image when both players are present
                if (waitingForPlayerImage != null)
                {
                    waitingForPlayerImage.SetActive(false);
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Hiding waiting for player image");
                }


                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Timed out waiting for 2 helixes with correct tags!");
        IFrameBridge.Instance.PostMatchAbort("Game setup failed", "Timed out waiting for helixes", "1011");
    }

    public void UpdateScoreUI()
    {
        if (isGameEnded)
        {
            return;
        }

        int p1Score, p2Score;
        if (scoreManager != null && scoreManager.Object != null && scoreManager.Object.Id.IsValid && scoreManager.Object.Runner != null)
        {
            p1Score = scoreManager.Player1Score;
            p2Score = scoreManager.Player2Score;
            UpdateLocalScores(p1Score, p2Score); // Cache scores
        }
        else
        {
            p1Score = localPlayer1Score;
            p2Score = localPlayer2Score;
           
        }

        if (scoreText != null)
        {
            scoreText.text = $"{p1Score}  :  {p2Score}";
            
        }
        else
        {
           
        }

        if (player1Slider != null && player2Slider != null)
        {
            player1Slider.value = p1Score;
            player2Slider.value = p2Score;
            
        }
    }

    public void EndGame(string winner)
    {
        if (isGameEnded)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Game already ended, ignoring EndGame call for {winner}");
            return;
        }
        isGameEnded = true;
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] {winner} Won!");
        if (gameAudioSource != null && gameAudioSource.isPlaying) gameAudioSource.Stop();
        if (gameOverAudioSource != null) gameOverAudioSource.Play();

        // Disable AudioListener when game ends
        if (audioListener != null)
        {
            audioListener.enabled = false;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] AudioListener disabled at game end");
        }

        if (isOnePlayerMode)
        {
            if (player1Helix != null) player1Helix.enabled = false;
            if (player2Helix != null) player2Helix.enabled = false;
        }
        else
        {
            if (player1Helix != null) player1Helix.enabled = false;
            if (player2Helix != null) player2Helix.enabled = false;
        }

        if (scoreboardPanel != null) scoreboardPanel.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);

        // Determine winner text and scores
        string winnerText = winner == LocalPlayerId ? "You won!" : "Opponent wins!";
        int localScore, opponentScore;
        if (scoreManager != null && scoreManager.Object != null && scoreManager.Object.Id.IsValid && scoreManager.Object.Runner != null)
        {
            localScore = LocalPlayerId == "Player1" ? scoreManager.Player1Score : scoreManager.Player2Score;
            opponentScore = LocalPlayerId == "Player1" ? scoreManager.Player2Score : scoreManager.Player1Score;
            UpdateLocalScores(scoreManager.Player1Score, scoreManager.Player2Score);
        }
        else
        {
            localScore = LocalPlayerId == "Player1" ? localPlayer1Score : localPlayer2Score;
            opponentScore = LocalPlayerId == "Player1" ? localPlayer2Score : localPlayer1Score;
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Using cached scores for EndGame: Local={localScore}, Opponent={opponentScore}");
        }

        string message = $"{winnerText}\nYou: {localScore}\nOpponent: {opponentScore}";

        if (endGameText == null || GameOver == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] UI components missing: endGameText={(endGameText == null ? "null" : "assigned")}, GameOver={(GameOver == null ? "null" : "assigned")}");
        }
        else
        {
            // Ensure GameOver canvas is properly configured
            Canvas canvas = GameOver.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Assign Player2's camera if LocalPlayerId is Player2
                Camera playerCamera = LocalPlayerId == "Player2" ?
                    player2Helix?.transform.parent?.Find("Camera")?.GetComponent<Camera>() :
                    player1Helix?.transform.parent?.Find("Camera")?.GetComponent<Camera>();

                if (playerCamera != null)
                {
                    canvas.worldCamera = playerCamera;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.planeDistance = 1f;
                    Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Assigned {LocalPlayerId}'s camera to GameOver canvas: {playerCamera.name}");
                }
                else
                {
                    Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Could not find camera for {LocalPlayerId}");
                }
            }

            endGameText.text = message;
            GameOver.SetActive(true);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] GameOver set to active, text: {message}, Canvas active: {GameOver.activeSelf}");
        }

        // Post match result with local player's score
        string outcome = isOnePlayerMode
            ? (winner == "Player1" ? "won" : "lost")
            : (winner == "Player1" && LocalPlayerId == "Player1") ||
              (winner == "Player2" && LocalPlayerId == "Player2") ? "won" : "lost";
              

        StartCoroutine(EndMultiplayerGameCoroutine(outcome, localScore, opponentScore));
    }

    private IEnumerator EndMultiplayerGameCoroutine(string outcome, int score, int score2)
    {
        // Wait for 5 seconds to display the win sprite
        yield return new WaitForSeconds(5f);
        IFrameBridge.Instance.PostMatchResult(outcome, score, score2);

        // Find the local player
        HelixTowerRotation localPlayer = FindObjectsByType<HelixTowerRotation>(FindObjectsSortMode.None).FirstOrDefault(p => p.Object.HasInputAuthority);
        if (localPlayer != null)
        {
            // Explicitly despawn the local player's network object
            Connector.Instance.NetworkRunner.Despawn(localPlayer.Object);
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GameplayManager] Player despawned.");
        }

        // Shutdown the network runner and load the main menu scene after completion
        Connector.Instance.NetworkRunner.Shutdown();

    }
}