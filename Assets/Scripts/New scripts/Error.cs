using UnityEngine;

public class Error : MonoBehaviour
{
    private ScoreManager scoreManager;
    

    private void Start()
    {
        scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] ScoreManager not found in scene!");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
       
        if (scoreManager == null)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] ScoreManager not assigned!");
            return;
        }
        if (other.gameObject.CompareTag("Player1"))
        {
           
            scoreManager.DecrementPlayer1Score();
        }
        if (other.gameObject.CompareTag("Player2"))
        {
            scoreManager.DecrementPlayer2Score();
           
        }
    }
}