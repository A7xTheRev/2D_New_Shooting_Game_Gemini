using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverTrigger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            PlayerStats.lastSessionCoins = player.sessionCoins;
            PlayerStats.lastSessionSpecialCurrency = player.sessionSpecialCurrency;
        }
        
        SceneManager.LoadScene("GameOver");
    }
}