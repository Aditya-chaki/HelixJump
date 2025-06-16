using UnityEngine;

public class SP_ScoreTrigger : MonoBehaviour
{
    private SP_ScoreManager scoreManager;
      [SerializeField] private DynamicTextData data;


    private void Start()
    {
        scoreManager = FindObjectOfType<SP_ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] SP_ScoreManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
          Vector3 pos = new Vector3(other.transform.position.x + Random.Range(-0.5f, 0.5f), other.transform.position.y - 1f, other.transform.position.z);
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
                DynamicTextManager.CreateText(pos, "+2", data);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] Player1 score incremented.");
            }
            else if (other.CompareTag("Player2"))
            {
                scoreManager.IncrementPlayer2Score();
                DynamicTextManager.CreateText(pos, "+2", data);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreTrigger] Player2 score incremented.");
            }
        }
    }
}