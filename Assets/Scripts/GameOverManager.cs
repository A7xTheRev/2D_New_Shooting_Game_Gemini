using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI coinsEarnedText;

    void Start()
    {
        if (coinsEarnedText != null)
        {
            coinsEarnedText.text = "Coins obtained: " + PlayerStats.lastSessionCoins;
        }

        if (ProgressionManager.Instance != null)
        {
            // Aggiunge le monete normali al totale persistente
            if (PlayerStats.lastSessionCoins > 0)
            {
                ProgressionManager.Instance.AddCoins(PlayerStats.lastSessionCoins);
            }

            // Aggiunge le gemme al totale persistente
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