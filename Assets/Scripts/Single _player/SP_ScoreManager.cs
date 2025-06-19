using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SP_ScoreManager : MonoBehaviour
{
    private int player1Score = 0;
    private int player2Score = 0;
    private bool gameEnded = false;
    private const int SCORE_TO_WIN = 50;

    // UI Elements
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private TextMeshProUGUI winnerText;
    public GameObject WinnerCard;

    // Progress Bar Sliders
    [SerializeField] private Slider player1Slider;
    [SerializeField] private Slider player2Slider;

    // Public properties to access scores
    public int Player1Score => player1Score;
    public int Player2Score => player2Score;

    private void Start()
    {
        // Ensure there's only one SP_ScoreManager
        SP_ScoreManager[] scoreManagers = FindObjectsOfType<SP_ScoreManager>();
        if (scoreManagers.Length > 1)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Multiple SP_ScoreManagers found in scene! Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        // Validate UI elements
        if (player1ScoreText == null || player2ScoreText == null || winnerText == null || player1Slider == null || player2Slider == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] One or more UI elements are not assigned!");
        }

        // Initialize Sliders
        player1Slider.minValue = 0;
        player1Slider.maxValue = SCORE_TO_WIN;
        player2Slider.minValue = 0;
        player2Slider.maxValue = SCORE_TO_WIN;

        // Initialize UI
        UpdateScoreUI();
        winnerText.text = "";
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Initial Scores - Player1: {player1Score}, Player2: {player2Score}");
    }

    public void IncrementPlayer1Score()
    {
        if (gameEnded) return;

        player1Score++;
        player1Score++;
        UpdateScoreUI();
        CheckForWinner();
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player1 Score: {player1Score}");
    }

    public void IncrementPlayer2Score()
    {
        if (gameEnded) return;

        player2Score++;
        player2Score++;
        UpdateScoreUI();
        CheckForWinner();
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player2 Score: {player2Score}");
    }

    public void DecrementPlayer1Score()
    {
        if (gameEnded) return;

        player1Score = Mathf.Max(0, player1Score - 1);
        UpdateScoreUI();
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player1 Score: {player1Score}");
    }

    public void DecrementPlayer2Score()
    {
        if (gameEnded) return;

        player2Score = Mathf.Max(0, player2Score - 1);
        UpdateScoreUI();
        Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player2 Score: {player2Score}");
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"{player1Score}";
        }
        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"{player2Score}";
        }
        if (player1Slider != null)
        {
            player1Slider.value = player1Score;
        }
        if (player2Slider != null)
        {
            player2Slider.value = player2Score;
        }
    }

    private void CheckForWinner()
    {
        if (gameEnded) return;

        if (player1Score >= SCORE_TO_WIN)
        {
            gameEnded = true;
            WinnerCard.SetActive(true);
            winnerText.text = "You Wins!";
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player 1 Wins with score: {player1Score}");
            StartCoroutine(PostMatchResultWithDelay("won", player1Score));
        }
        else if (player2Score >= SCORE_TO_WIN)
        {
            gameEnded = true;
            WinnerCard.SetActive(true);
            winnerText.text = "Opponent Wins!";
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SP_ScoreManager] Player 2 Wins with score: {player2Score}");
            StartCoroutine(PostMatchResultWithDelay("lost", player1Score));
        }
    }

    private IEnumerator PostMatchResultWithDelay(string result, int score)
    {
        yield return new WaitForSeconds(5f);
        IFrameBridge.Instance.PostMatchResult(result, score);
    }
}