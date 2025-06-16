using UnityEngine;
using System;
using System.Collections;
using Game.Utility;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class IFrameBridge : Singleton<IFrameBridge>
{
    public static string MatchId { get; private set; }
    public static string PlayerId { get; private set; }
    public static string OpponentId { get; private set; }
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SendMatchResult(string outcome, int score);
    [DllImport("__Internal")] private static extern void SendMatchAbort(string message, string error, string errorCode);
    [DllImport("__Internal")] private static extern void SendScreenshot(string base64);
#endif

    void Start()
    {
        // Parse URL parameters
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[IFrameBridge] Running in Editor, generating random parameters.");
            MatchId = "Room01";
            PlayerId = $"player_{UnityEngine.Random.Range(1000, 9999)}";
            OpponentId =$"player_{UnityEngine.Random.Range(1000, 9999)}" ; // Default to a9 for testing AI scene in Editor
            // Ensure unique OpponentId
            while (OpponentId == PlayerId)
            {
                OpponentId = $"player_{UnityEngine.Random.Range(1000, 9999)}";
            }
        }
        else
        {
            Uri uri = new Uri(url);
            string query = uri.Query;
            if (!string.IsNullOrEmpty(query))
            {
                string[] pairs = query.TrimStart('?').Split('&');
                foreach (var pair in pairs)
                {
                    string[] kv = pair.Split('=');
                    if (kv.Length == 2)
                    {
                        string key = Uri.UnescapeDataString(kv[0]);
                        string value = Uri.UnescapeDataString(kv[1]);
                        if (key == "matchId") MatchId = value;
                        else if (key == "playerId") PlayerId = value;
                        else if (key == "opponentId") OpponentId = value;
                    }
                }
            }
        }

        Debug.Log($"[IFrameBridge] MatchId: {MatchId}, PlayerId: {PlayerId}, OpponentId: {OpponentId}");

        if (string.IsNullOrEmpty(MatchId) || string.IsNullOrEmpty(PlayerId))
        {
            PostMatchAbort("Invalid match parameters", "Missing URL parameters", "1004");
            return;
        }

        // Check for bot or multiplayer
        string botType = PlayerUtils.GetBotType(OpponentId);
        if (botType != null)
        {
            string sceneToLoad = botType == "a9" ? "AI" : "HardAI"; 
            Debug.Log($"[IFrameBridge] Bot detected (type: {botType}), loading scene: {sceneToLoad}");
            try
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[IFrameBridge] Failed to load scene {sceneToLoad}: {ex.Message}");
                PostMatchAbort("Scene load failed", $"Cannot load scene {sceneToLoad}", "1005");
            }
        }
        else
        {
            Debug.Log("[IFrameBridge] Starting multiplayer session.");
            Connector.Instance.ConnectToServer(MatchId);
        }

        StartCoroutine(CaptureAndSendScreenshotRoutine());
    }

    public void PostMatchResult(string outcome, int score = 0)
    {
        Debug.Log($"[IFrameBridge] Sending match_result: outcome={outcome}, score={score}");

#if UNITY_WEBGL && !UNITY_EDITOR
        SendMatchResult(outcome, score);
#else
        Debug.Log($"[IFrameBridge] [Editor] match_result: {{ matchId: {MatchId}, playerId: {PlayerId}, opponentId: {OpponentId}, outcome: {outcome}, score: {score} }}");
#endif
    }

    public void PostMatchAbort(string message, string error = "", string errorCode = "")
    {
        Debug.Log($"[IFrameBridge] Sending match_abort: message={message}, error={error}, errorCode={errorCode}");

#if UNITY_WEBGL && !UNITY_EDITOR
        SendMatchAbort(message, error, errorCode);
#else
        Debug.Log($"[IFrameBridge] [Editor] match_abort: {{ message: {message}, error: {error}, errorCode: {errorCode} }}");
#endif
    }

    private IEnumerator CaptureAndSendScreenshotRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(CaptureAndSendScreenshot());
        }
    }

    private IEnumerator CaptureAndSendScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        string base64 = Convert.ToBase64String(pngData);

#if UNITY_WEBGL && !UNITY_EDITOR
        SendScreenshot(base64);
#endif
        Destroy(tex);
    }

    public static class PlayerUtils
    {
        public static string GetBotType(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                Debug.LogWarning($"[PlayerUtils] GetBotType: playerId is null or empty");
                return null;
            }

            if (playerId.StartsWith("a9"))
            {
                Debug.Log($"[PlayerUtils] GetBotType: playerId {playerId} is Normal AI (a9)");
                return "a9";
            }
            else if (playerId.StartsWith("b9"))
            {
                Debug.Log($"[PlayerUtils] GetBotType: playerId {playerId} is Hard AI (b9)");
                return "b9";
            }

            Debug.Log($"[PlayerUtils] GetBotType: playerId {playerId} is not a bot");
            return null;
        }
    }
}