using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] private string miniGameID; // Name of the mini-game
    [SerializeField] private bool isAlreadyPlayed; // Flag to check if the mini-game has been played before
    [SerializeField] private int starCount; // Number of stars earned in the mini-game
    private void StartMiniGame()
    {
        StartCountdown();
        // Implement mini-game start logic here
    }

    private void EndMiniGame()
    {
        StopCountdown();
        // Implement mini-game end logic here
    }

    private void ResetMiniGame()
    {
        // Implement mini-game reset logic here
    }

    private void StartCountdown()
    {
        // Implement countdown logic here
    }

    private void StopCountdown()
    {
        // Implement countdown stop logic here
    }


    private void CountStars()
    {
        // Implement star counting logic here
    }

    public void NotifyPlayerWin()
    {
        OnPlayerWin();
    }

    public void NotifyPlayerLose()
    {
        OnPlayerLose();
    }

    protected void OnPlayerWin()
    {
        CountStars();
        EndMiniGame();
    }

    protected void OnPlayerLose()
    {
        CountStars();
        EndMiniGame();
    }

}