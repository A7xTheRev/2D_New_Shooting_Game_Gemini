using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Player")]
    public Slider playerHealthBar;
    public Slider xpBar;
    public TextMeshProUGUI sessionCoinsText;
    public TextMeshProUGUI specialCurrencyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI levelText;
    public GameObject secondChanceIcon;
    public TextMeshProUGUI notificationText;

    [Header("UI Abilità Speciale")]
    public Slider abilitySlider;
    public Image abilityIconImage;

    [Header("Pannelli")]
    public GameObject optionsPanel; // NUOVO RIFERIMENTO

    private PlayerStats player;
    private StageManager stageManager;
    private AbilityController abilityController;

    void Start()
    {
        player = FindFirstObjectByType<PlayerStats>();
        stageManager = FindFirstObjectByType<StageManager>();
        abilityController = FindFirstObjectByType<AbilityController>();

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
            UpdateSecondChanceUI(ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.SecondChance));
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        
        if (abilityController != null)
        {
            abilityController.OnChargeChanged += UpdateAbilityUI;
            if (abilityController.equippedAbility == null)
            {
                if (abilitySlider != null) abilitySlider.gameObject.SetActive(false);
                if (abilityIconImage != null) abilityIconImage.gameObject.SetActive(false);
            }
        }
        else if (abilitySlider != null)
        {
             abilitySlider.gameObject.SetActive(false);
             if (abilityIconImage != null) abilityIconImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (stageManager != null && stageText != null)
            stageText.text = "STAGE: " + stageManager.stageNumber;
    }

    // --- NUOVO METODO PER LA PAUSA ---
    public void OpenOptionsMenu()
    {
        if (optionsPanel != null)
        {
            // Mette in pausa il gioco
            Time.timeScale = 0f;
            // Mostra il pannello delle opzioni
            optionsPanel.SetActive(true);
        }
    }

    public void ShowNotification(string message, float duration)
    {
        StartCoroutine(NotificationCoroutine(message, duration));
    }

    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        if (notificationText == null) yield break;
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
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
        if (xpBar != null)
        {
            if (xpToLevelUp > 0)
            {
                xpBar.value = (float)currentXP / xpToLevelUp;
            }
        }
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

    public void UpdateAbilityUI(float currentCharge, float maxCharge, Sprite icon)
    {
        if (abilitySlider != null)
        {
            if (maxCharge > 0)
                abilitySlider.value = currentCharge / maxCharge;
        }
        if (abilityIconImage != null)
        {
            abilityIconImage.sprite = icon;
        }
    }
}