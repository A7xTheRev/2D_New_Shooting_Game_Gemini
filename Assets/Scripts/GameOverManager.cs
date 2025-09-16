using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI gemsEarnedText; // NUOVO RIFERIMENTO

    void Start()
    {
        // Mostra le monete guadagnate nella sessione
        if (coinsEarnedText != null)
        {
            coinsEarnedText.text = "Monete Ottenute: " + PlayerStats.lastSessionCoins;
        }

        // --- NUOVA LOGICA ---
        // Mostra le gemme guadagnate nella sessione
        if (gemsEarnedText != null)
        {
            gemsEarnedText.text = "Gemme Ottenute: " + PlayerStats.lastSessionSpecialCurrency;
        }
        // --- FINE NUOVA LOGICA ---


        // Somma le valute al totale persistente
        if (ProgressionManager.Instance != null)
        {
            if (PlayerStats.lastSessionCoins > 0)
            {
                ProgressionManager.Instance.AddCoins(PlayerStats.lastSessionCoins);
            }
            if (PlayerStats.lastSessionSpecialCurrency > 0)
            {
                ProgressionManager.Instance.AddSpecialCurrency(PlayerStats.lastSessionSpecialCurrency);
            }
        }

        // Resetta le valute della sessione per la prossima partita
        PlayerStats.lastSessionCoins = 0;
        PlayerStats.lastSessionSpecialCurrency = 0;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}