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

        // Salva i coin della sessione per GameOver
        PlayerStats player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (player != null)
        {
            PlayerStats.lastSessionCoins = player.sessionCoins;
            Debug.Log("Button premuto - Coins salvati: " + PlayerStats.lastSessionCoins);
        }

        // Carica la scena GameOver
        SceneManager.LoadScene(sceneName);
    }
}
