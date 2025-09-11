using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Player")]
    public Slider playerHealthBar;
    public TextMeshProUGUI sessionCoinsText;
    public TextMeshProUGUI specialCurrencyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI levelText;

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
    }

    void Update()
    {
        if (stageManager != null && stageText != null)
            stageText.text = "STAGE: " + stageManager.stageNumber;
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
}