using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private ScoreManager scoreManager;
    

    private void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] ScoreManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] ScoreManager not assigned!");
            return;
        }
        if (other.CompareTag("Player1"))
        {
            scoreManager.IncrementPlayer1Score();
           
        }
        if (other.CompareTag("Player2"))
        {
            scoreManager.IncrementPlayer2Score();
           
        }
    }
     
}