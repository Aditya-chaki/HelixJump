using UnityEngine;
using Fusion;

public class ScoreManager : NetworkBehaviour
{
    [Networked] public int Player1Score { get; set; } = 0;
    [Networked] public int Player2Score { get; set; } = 0;
    public int scoreToWin = 5;
    private bool isGameEnded = false;

    void FixedUpdate()
    {
        if (GameplayManager.Instance.IsGameStarted && !isGameEnded) // Only update score UI if game started and not ended
        {
            GameplayManager.Instance.UpdateScoreUI();
        }
    }

    public void IncrementPlayer1Score()
    {
        if (!GameplayManager.Instance.IsGameStarted) return;
        if (isGameEnded) return;
        if (Object != null && Object.HasStateAuthority)
        {
            Player1Score++;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 1 scored: {Player1Score}");
            if (Player1Score >= scoreToWin)
            {
                isGameEnded = true;
                RPC_EndGame("Player1");
            }
        }
        else if (Object == null) // Single-player mode
        {
            Player1Score++;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 1 scored: {Player1Score}");
            if (Player1Score >= scoreToWin)
            {
                isGameEnded = true;
                GameplayManager.Instance.EndGame("Player1");
            }
        }
        else
        {
            RPC_IncrementPlayer1Score();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_IncrementPlayer1Score()
    {
        IncrementPlayer1Score();
    }

    public void IncrementPlayer2Score()
    {
        if (!GameplayManager.Instance.IsGameStarted) return;
        if (isGameEnded) return;
        if (Object != null && Object.HasStateAuthority)
        {
            Player2Score++;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 2 scored: {Player2Score}");
            if (Player2Score >= scoreToWin)
            {
                isGameEnded = true;
                RPC_EndGame("Player2");
            }
        }
        else if (Object == null) // Single-player mode
        {
            Player2Score++;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 2 scored: {Player2Score}");
            if (Player2Score >= scoreToWin)
            {
                isGameEnded = true;
                GameplayManager.Instance.EndGame("Player2");
            }
        }
        else
        {
            RPC_IncrementPlayer2Score();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_IncrementPlayer2Score()
    {
        IncrementPlayer2Score();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_EndGame(string winner)
    {
        GameplayManager.Instance.EndGame(winner);
    }

    public void DecrementPlayer1Score()
    {
        if (!GameplayManager.Instance.IsGameStarted) return;
        if (isGameEnded || Player1Score <= 0) return;
        if (Object.HasStateAuthority)
        {
            Player1Score--;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 1 score decremented to {Player1Score}");
        }
        else
        {
            RPC_DecrementPlayer1Score();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DecrementPlayer1Score()
    {
        DecrementPlayer1Score();
    }

    public void DecrementPlayer2Score()
    {
        if (!GameplayManager.Instance.IsGameStarted) return;
        if (isGameEnded || Player2Score <= 0) return;
        if (Object.HasStateAuthority)
        {
            Player2Score--;
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ScoreManager] Player 2 score decremented to {Player2Score}");
        }
        else
        {
            RPC_DecrementPlayer2Score();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DecrementPlayer2Score()
    {
        DecrementPlayer2Score();
    }
}