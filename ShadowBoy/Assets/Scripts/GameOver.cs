using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public float countdown = 3f;
    public bool countdownStart = false;

    private void Update()
    {
        if (countdownStart == true)
        {
            countdown -= Time.deltaTime;
            ShowGameOver();
        }
    }
    public void ShowGameOver()
    {
        countdownStart = true;
        if(countdown <= 0)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;
        }
        
        
    }

    public void RestartLevel()
    {
        countdownStart = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        countdownStart = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("StartStop");
    }
}
