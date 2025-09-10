using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverTrigger : MonoBehaviour
{
    void Update()
    {
        // Controlla se il giocatore preme ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        // Trova il player e salva i coin della sessione
        PlayerStats player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (player != null)
        {
            PlayerStats.lastSessionCoins = player.sessionCoins;
            Debug.Log("Fine partita forzata - coins salvati: " + PlayerStats.lastSessionCoins);
        }

        // Carica la scena GameOver
        SceneManager.LoadScene("GameOver");
    }
}
