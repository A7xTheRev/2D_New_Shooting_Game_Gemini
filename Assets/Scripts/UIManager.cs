using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Aggiunto per usare le Coroutine

public class UIManager : MonoBehaviour
{
    [Header("UI Player")]
    public Slider playerHealthBar;
    public TextMeshProUGUI sessionCoinsText;
    public TextMeshProUGUI specialCurrencyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI levelText;
    public GameObject secondChanceIcon;
    public TextMeshProUGUI notificationText; // Riferimento al nuovo testo

    private PlayerStats player;
    private StageManager stageManager;

    void Start()
    {
        player = FindFirstObjectByType<PlayerStats>();
        stageManager = FindFirstObjectByType<StageManager>();

        if (player != null)
        {
            player.OnLevelUp += UpdateLevelUI;
            player.OnXPChanged += UpdateXPUI;
            player.OnHealthChanged += UpdateHealthUI;
            player.OnSessionCoinsChanged += UpdateSessionCoinsUI;
            player.OnSessionSpecialCurrencyChanged += UpdateSpecialCurrencyUI;

            UpdateSessionCoinsUI(player.sessionCoins);
            UpdateSpecialCurrencyUI(player.sessionSpecialCurrency);
            UpdateLevelUI(player.level);
            UpdateXPUI(player.currentXP, player.xpToLevelUp);
            UpdateHealthUI(player.currentHealth, player.maxHealth);
        }

        if (ProgressionManager.Instance != null && secondChanceIcon != null)
        {
            UpdateSecondChanceUI(ProgressionManager.Instance.IsSpecialUpgradeUnlocked(SpecialUpgradeType.SecondChance));
        }

        // Assicurati che il testo di notifica sia nascosto all'avvio
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (stageManager != null && stageText != null)
            stageText.text = "STAGE: " + stageManager.stageNumber;
    }

    // --- NUOVO METODO PUBBLICO PER LE NOTIFICHE ---
    public void ShowNotification(string message, float duration)
    {
        StartCoroutine(NotificationCoroutine(message, duration));
    }

    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        if (notificationText == null) yield break;

        // Mostra il messaggio
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Attendi per la durata specificata
        yield return new WaitForSeconds(duration);

        // Nascondi il messaggio
        notificationText.gameObject.SetActive(false);
    }

    public void UpdateLevelUI(int newLevel)
    {
        if (levelText != null)
            levelText.text = "Level: " + newLevel;
    }

    public void UpdateXPUI(int currentXP, int xpToLevelUp)
    {
        if (xpText != null)
            xpText.text = $"XP: {currentXP}/{xpToLevelUp}";
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (playerHealthBar != null)
            playerHealthBar.value = (float)currentHealth / maxHealth;
    }

    private void UpdateSessionCoinsUI(int sessionCoins)
    {
        if (sessionCoinsText != null)
            sessionCoinsText.text = "Coins: " + sessionCoins;
    }

    private void UpdateSpecialCurrencyUI(int amount)
    {
        if (specialCurrencyText != null)
            specialCurrencyText.text = "Gemme: " + amount;
    }
    
    public void UpdateSecondChanceUI(bool isAvailable)
    {
        if (secondChanceIcon != null)
        {
            secondChanceIcon.SetActive(isAvailable);
        }
    }
}