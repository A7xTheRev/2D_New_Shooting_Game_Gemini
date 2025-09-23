using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI di Scena Fissa")]
    public Slider playerHealthBar;
    public Slider xpBar;
    public TextMeshProUGUI sessionCoinsText;
    public TextMeshProUGUI specialCurrencyText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI levelText;
    public GameObject secondChanceIcon;
    public TextMeshProUGUI notificationText;
    public TextMeshProUGUI countdownText;
    public GameObject optionsPanel;
    [Tooltip("L'immagine della vignetta che si trova nel Canvas della GameScene.")]
    public Image slowMotionVignetteImage; // Riferimento alla vignetta della scena

    [Header("UI del Giocatore (collegata dinamicamente)")]
    public Slider abilitySlider;
    public Image abilityIconImage;

    private PlayerStats player;
    private StageManager stageManager;
    private AbilityController abilityController;

    void Awake()
    {
        // In Awake, troviamo solo i manager che sono già presenti nella scena all'inizio
        stageManager = FindFirstObjectByType<StageManager>();
    }

    void Start()
    {
        // Nascondiamo gli elementi che non devono essere visibili subito
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        
        // Avviamo il conto alla rovescia, che si occuperà di inizializzare il resto
        StartCoroutine(StartGameCountdownCoroutine());
    }

    void OnDestroy()
    {
        // È una buona pratica annullare l'iscrizione agli eventi quando l'oggetto viene distrutto
        UnsubscribeFromEvents();
    }

    // Nuovo metodo che si occupa di tutta la logica di inizializzazione della UI
    private void InitializeUI()
    {
        // Ora che il giocatore è stato creato, possiamo trovare i suoi componenti
        player = FindFirstObjectByType<PlayerStats>();
        if (player == null)
        {
            Debug.LogError("InitializeUI: Player non trovato! La UI non può essere inizializzata.");
            return;
        }
        abilityController = player.GetComponent<AbilityController>();

        // --- COLLEGAMENTO DINAMICO DELLA VIGNETTA ---
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null && slowMotionVignetteImage != null)
        {
            pc.slowMotionVignette = slowMotionVignetteImage;
        }
        // --- FINE COLLEGAMENTO ---

        // Iscriviti a tutti gli eventi del giocatore e dell'abilità
        SubscribeToEvents();

        // Aggiorna tutti gli elementi della UI con i valori iniziali
        UpdateSessionCoinsUI(player.sessionCoins);
        UpdateSpecialCurrencyUI(player.sessionSpecialCurrency);
        UpdateLevelUI(player.level);
        UpdateXPUI(player.currentXP, player.xpToLevelUp);
        UpdateHealthUI(player.currentHealth, player.maxHealth);
        
        // Gestisci la visibilità e lo stato iniziale della barra dell'abilità
        bool hasAbility = (abilityController != null && abilityController.equippedAbility != null);
        if (abilitySlider != null) abilitySlider.gameObject.SetActive(hasAbility);
        if (abilityIconImage != null) abilityIconImage.gameObject.SetActive(hasAbility);

        if (hasAbility)
        {
             UpdateAbilityUI(0, abilityController.equippedAbility.maxCharge, abilityController.equippedAbility.icon);
        }

        if (ProgressionManager.Instance != null && secondChanceIcon != null)
        {
            UpdateSecondChanceUI(ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.SecondChance));
        }
    }

    void Update()
    {
        if (stageManager != null && stageText != null)
            stageText.text = "STAGE: " + stageManager.stageNumber;
    }

    private IEnumerator StartGameCountdownCoroutine()
    {
        if (slowMotionVignetteImage != null)
        {
            slowMotionVignetteImage.color = new Color(slowMotionVignetteImage.color.r, slowMotionVignetteImage.color.g, slowMotionVignetteImage.color.b, 0);
        }

        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null) playerController.controlsEnabled = false;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(1f);
            countdownText.text = "GO!";
            yield return new WaitForSecondsRealtime(0.5f);
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(3f);
        }
        
        InitializeUI();
        
        // Ora cerchiamo di nuovo il player controller perché potrebbe essere stato appena spawnato
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null) playerController.controlsEnabled = true;

        if (stageManager != null) stageManager.BeginSpawning();
    }

    private void SubscribeToEvents()
    {
        if (player != null)
        {
            player.OnLevelUp += UpdateLevelUI;
            player.OnXPChanged += UpdateXPUI;
            player.OnHealthChanged += UpdateHealthUI;
            player.OnSessionCoinsChanged += UpdateSessionCoinsUI;
            player.OnSessionSpecialCurrencyChanged += UpdateSpecialCurrencyUI;
        }
        if (abilityController != null)
        {
            abilityController.OnChargeChanged += UpdateAbilityUI;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (player != null)
        {
            player.OnLevelUp -= UpdateLevelUI;
            player.OnXPChanged -= UpdateXPUI;
            player.OnHealthChanged -= UpdateHealthUI;
            player.OnSessionCoinsChanged -= UpdateSessionCoinsUI;
            player.OnSessionSpecialCurrencyChanged -= UpdateSpecialCurrencyUI;
        }
        if (abilityController != null)
        {
            abilityController.OnChargeChanged -= UpdateAbilityUI;
        }
    }
    
    public void OpenOptionsMenu()
    {
        if (optionsPanel != null)
        {
            Time.timeScale = 0f;
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
        if (xpBar != null && xpToLevelUp > 0)
        {
            xpBar.value = (float)currentXP / xpToLevelUp;
        }
    }

    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (playerHealthBar != null && maxHealth > 0)
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
        if (abilitySlider != null && maxCharge > 0)
        {
            abilitySlider.value = currentCharge / maxCharge;
        }
        if (abilityIconImage != null)
        {
            abilityIconImage.sprite = icon;
        }
    }
}