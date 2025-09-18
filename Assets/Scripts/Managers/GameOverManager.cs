using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Riferimenti Risultati")]
    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI gemsEarnedText;
    public TextMeshProUGUI waveReachedText; // NUOVO RIFERIMENTO
    
    [Header("UI Riferimenti Record")]
    public GameObject newRecordIndicator; // NUOVO RIFERIMENTO (es. un'immagine "NEW RECORD!")

    // --- NUOVE VARIABILI PER SALVARE I DATI DELLA PARTITA ---
    private static int lastSessionWave = 1;
    private static int lastSessionCoins = 0;
    private static int lastSessionGems = 0;
    // --- FINE NUOVE VARIABILI ---


    // --- NUOVO METODO STATICO DA CHIAMARE PRIMA DI CARICARE LA SCENA ---
    // Altri script (come PlayerStats) useranno questo metodo per passare i dati
    public static void SetEndGameStats(int wave, int coins, int gems)
    {
        lastSessionWave = wave;
        lastSessionCoins = coins;
        lastSessionGems = gems;
    }


    void Start()
    {
        // Mostra i risultati della sessione appena conclusa
        if (waveReachedText != null)
        {
            waveReachedText.text = "Ondata Raggiunta: " + lastSessionWave;
        }
        if (coinsEarnedText != null)
        {
            coinsEarnedText.text = "Monete Ottenute: " + lastSessionCoins;
        }
        if (gemsEarnedText != null)
        {
            gemsEarnedText.text = "Gemme Ottenute: " + lastSessionGems;
        }
        
        // Controlla se abbiamo stabilito un nuovo record
        if (ProgressionManager.Instance != null)
        {
            bool isNewRecord = ProgressionManager.Instance.CheckForNewHighScores(lastSessionWave, lastSessionCoins);

            // Se sì, mostra l'indicatore di nuovo record
            if (newRecordIndicator != null)
            {
                newRecordIndicator.SetActive(isNewRecord);
            }

            // Somma le valute al totale persistente
            if (lastSessionCoins > 0)
            {
                ProgressionManager.Instance.AddCoins(lastSessionCoins);
            }
            if (lastSessionGems > 0)
            {
                ProgressionManager.Instance.AddSpecialCurrency(lastSessionGems);
            }
        }

        // Resetta le valute della sessione per la prossima partita
        lastSessionCoins = 0;
        lastSessionGems = 0;
        lastSessionWave = 1;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}