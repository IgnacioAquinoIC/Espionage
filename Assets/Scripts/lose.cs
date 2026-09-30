using UnityEngine;
public class Lose : MonoBehaviour
{
    public static Lose Instance;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private bool gameOver = false;

    void Awake()
    {
        Instance = this;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        // Show the Game Over screen.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Stop everything.
        Time.timeScale = 0f;

        Debug.Log("GAME OVER");
    }
}