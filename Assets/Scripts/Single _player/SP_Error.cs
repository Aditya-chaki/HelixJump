using UnityEngine;

public class SP_Error : MonoBehaviour
{
    private SP_ScoreManager scoreManager;
    [SerializeField] private DynamicTextData data;

    private void Start()
    {
        scoreManager = FindObjectOfType<SP_ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] SP_ScoreManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
         Vector3 pos = new Vector3(other.transform.position.x + Random.Range(-0.5f, 0.5f), other.transform.position.y +  1, other.transform.position.z);
        if (scoreManager == null)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] SP_ScoreManager not assigned!");
            return;
        }

        // Check if the collided object is the BALL and has the appropriate tag
        if (other.name == "BALL")
        {
            if (other.CompareTag("Player1"))
            {
                scoreManager.DecrementPlayer1Score();
                DynamicTextManager.CreateText(pos, "-1", data);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] Player1 score decremented.");
            }
            else if (other.CompareTag("Player2"))
            {
                scoreManager.DecrementPlayer2Score();
                DynamicTextManager.CreateText(pos, "-1", data);
                Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] Player2 score decremented.");
            }
        }
    }
}