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
            
            // --- MODIFICA QUI ---
            // Prendiamo anche il tempo di sopravvivenza dallo StageManager
            float timeSurvived = (stageManager != null) ? stageManager.GetSurvivalTime() : 0f;

            // E lo passiamo al metodo
            GameOverManager.SetEndGameStats(currentWave, player.sessionCoins, player.sessionSpecialCurrency, timeSurvived);
            // --- FINE MODIFICA ---
        }
        
        // Riporta il tempo alla normalità prima di caricare la scena
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
}