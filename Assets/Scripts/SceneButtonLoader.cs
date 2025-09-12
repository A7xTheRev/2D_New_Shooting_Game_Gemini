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

        // Trova il player e salva ENTRAMBE le valute della sessione
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            PlayerStats.lastSessionCoins = player.sessionCoins;
            PlayerStats.lastSessionSpecialCurrency = player.sessionSpecialCurrency; // ECCO LA RIGA MANCANTE
            Debug.Log("Pulsante Quit premuto - Valute salvate.");
        }

        // Carica la scena GameOver
        SceneManager.LoadScene(sceneName);
    }
}