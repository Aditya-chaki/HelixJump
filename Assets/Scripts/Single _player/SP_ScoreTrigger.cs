using UnityEngine;

public class SP_ScoreTrigger : MonoBehaviour
{
    public SP_ScoreManager scoreManager;
       


    private void Start()
    {
        
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] SP_ScoreManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        

        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] SP_ScoreManager not assigned!");
            return;
        }

        // Check if the collider is the BALL and has the appropriate tag
        if (other.name == "BALL")
        {
            if (other.CompareTag("Player1"))
            {
                scoreManager.IncrementPlayer1Score();
                
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] Player1 score incremented.");
            }
            else if (other.CompareTag("Player2"))
            {
                scoreManager.IncrementPlayer2Score();
                
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] Player2 score incremented.");
            }
        }
    }
}