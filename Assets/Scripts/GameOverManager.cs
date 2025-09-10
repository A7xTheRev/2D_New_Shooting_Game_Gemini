using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI coinsEarnedText;

    void Start()
    {
        // Mostra i coin guadagnati nella sessione
        if (coinsEarnedText != null)
        {
            coinsEarnedText.text = "Coins obtained: " + PlayerStats.lastSessionCoins;
        }

        // ✅ Somma ai coin permanenti usando ProgressionManager
        if (ProgressionManager.Instance != null && PlayerStats.lastSessionCoins > 0)
        {
            ProgressionManager.Instance.AddCoins(PlayerStats.lastSessionCoins);
            Debug.Log("Aggiunti " + PlayerStats.lastSessionCoins + " ai coins persistenti!");
        }

        // ✅ Resetta i coin della sessione (già salvati nei persistenti)
        PlayerStats.lastSessionCoins = 0;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
