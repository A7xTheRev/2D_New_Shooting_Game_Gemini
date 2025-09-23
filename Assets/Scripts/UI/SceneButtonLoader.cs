using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonLoader : MonoBehaviour
{
    [Tooltip("Nome della scena da caricare")]
    public string sceneName = "GameOver";

    // Metodo collegato al Button OnClick in inspector
    public void LoadSceneAndEndSession()
    {
        // Riporta il gioco in tempo normale
        Time.timeScale = 1f;

        // Trova il player per ottenere i suoi risultati
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            // --- LOGICA UNIFICATA (copiata da PlayerStats.Die()) ---
            // Troviamo lo StageManager per sapere a che ondata siamo arrivati
            StageManager stageManager = FindFirstObjectByType<StageManager>();
            int currentWave = (stageManager != null) ? stageManager.stageNumber : 1;
            
            // --- MODIFICA QUI ---
            // Prendiamo anche il tempo di sopravvivenza dallo StageManager
            float timeSurvived = (stageManager != null) ? stageManager.GetSurvivalTime() : 0f;
            
            // E lo passiamo al metodo
            GameOverManager.SetEndGameStats(currentWave, player.sessionCoins, player.sessionSpecialCurrency, timeSurvived);
            // --- FINE MODIFICA ---
            
            Debug.Log("Pulsante Quit premuto - Dati salvati correttamente.");
        }

        // Carica la scena GameOver
        SceneManager.LoadScene(sceneName);
    }
}