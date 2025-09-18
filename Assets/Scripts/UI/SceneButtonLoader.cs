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
            
            // Usiamo il metodo statico corretto per passare tutti i dati alla schermata di Game Over
            GameOverManager.SetEndGameStats(currentWave, player.sessionCoins, player.sessionSpecialCurrency);
            // --- FINE LOGICA UNIFICATA ---
            
            Debug.Log("Pulsante Quit premuto - Dati salvati correttamente.");
        }

        // Carica la scena GameOver
        SceneManager.LoadScene(sceneName);
    }
}