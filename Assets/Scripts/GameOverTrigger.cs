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
            // --- LOGICA CORRETTA ---
            // Troviamo lo StageManager per sapere a che ondata siamo arrivati
            StageManager stageManager = FindFirstObjectByType<StageManager>();
            int currentWave = (stageManager != null) ? stageManager.stageNumber : 1;
            
            // Usiamo il metodo statico corretto per passare tutti i dati al GameOverManager
            GameOverManager.SetEndGameStats(currentWave, player.sessionCoins, player.sessionSpecialCurrency);
            // --- FINE LOGICA CORRETTA ---
        }
        
        // Riporta il tempo alla normalità prima di caricare la scena
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
}