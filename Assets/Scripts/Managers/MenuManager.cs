using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    // Nuova classe interna per collegare facilmente pulsanti e dati nell'Inspector
    [System.Serializable]
    public class WeaponSelectionButton
    {
        public WeaponData weaponData;
        public Button button;
    }

    [Header("Pannelli Schermate")]
    public GameObject mainPanel;
    public GameObject storePanel;
    public GameObject hangarPanel;
    public GameObject backgroundPanel;
    public GameObject shipPanel;

    [Header("UI Generale")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI specialCurrencyText;

    [Header("UI Record Personali")]
    public TextMeshProUGUI maxWaveText;
    public TextMeshProUGUI maxCoinsText;

    [Header("Pulsanti Arma")]
    // Sostituiamo i vecchi riferimenti singoli con una lista configurabile
    public List<WeaponSelectionButton> weaponButtons;

    [Header("Selezione Abilità Speciale (Hangar)")]
    public Image hangarAbilityIcon;
    public TextMeshProUGUI hangarAbilityName;
    public TextMeshProUGUI hangarAbilityDescription;
    public Button hangarPrevButton;
    public Button hangarNextButton;

    [Header("Pannelli Potenziamenti Normali")]
    public List<UpgradeUIPanel> normalUpgradePanels = new List<UpgradeUIPanel>();

    [Header("Pannelli Potenziamenti Speciali")]
    public List<SpecialAbilityUIPanel> specialUpgradePanels = new List<SpecialAbilityUIPanel>();

    [System.Serializable]
    public class UpgradeUIPanel 
    { 
        public PermanentUpgradeType upgradeType; 
        public TextMeshProUGUI levelText; 
        public TextMeshProUGUI costText; 
        public Button buyButton; 
    }
    
    [System.Serializable]
    public class SpecialAbilityUIPanel 
    { 
        public AbilityID abilityID; 
        public TextMeshProUGUI descriptionText; 
        public TextMeshProUGUI costText; 
        public Button buyButton; 
        public GameObject unlockedIndicator; 
    }
    
    private int currentAbilityIndex = 0;
    private List<SpecialAbility> unlockedAbilities;
    private bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void Awake()
    {
        int targetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }

    void Start()
    {
        // Collega i listener per i pannelli di upgrade
        foreach (var panel in normalUpgradePanels)
        {
            PermanentUpgradeType type = panel.upgradeType;
            panel.buyButton.onClick.RemoveAllListeners();
            panel.buyButton.onClick.AddListener(() => OnBuyUpgradeButtonPressed(type));
        }
        foreach (var panel in specialUpgradePanels)
        {
            AbilityID id = panel.abilityID;
            panel.buyButton.onClick.RemoveAllListeners();
            panel.buyButton.onClick.AddListener(() => OnBuySpecialUpgradeButtonPressed(id));
        }

        // Collega i listener per i pulsanti delle armi in modo dinamico
        foreach (var weaponButton in weaponButtons)
        {
            weaponButton.button.onClick.AddListener(() => SelectWeapon(weaponButton.weaponData));
        }

        ShowMainPanel();
        LoadAndHighlightSavedWeapon();
        UpdateAllUI();
        UpdateRecordUI();
    }

    void OnEnable() 
    { 
        if (ProgressionManager.Instance != null) { ProgressionManager.OnValuesChanged += UpdateAllUI; } 
        SetupAbilitySelection(); 
        UpdateAllUI(); 
        UpdateRecordUI();
    }

    void OnDisable() 
    { 
        if (isQuitting) return;
        if (ProgressionManager.Instance != null) { ProgressionManager.OnValuesChanged -= UpdateAllUI; } 
    }

    private void UpdateRecordUI()
    {
        if (ProgressionManager.Instance != null)
        {
            if (maxWaveText != null) maxWaveText.text = "Max Wave: " + ProgressionManager.Instance.GetMaxWave();
            if (maxCoinsText != null) maxCoinsText.text = "Max Coins: " + ProgressionManager.Instance.GetMaxCoins();
        }
    }
    
    public void ShowMainPanel() 
    { 
        mainPanel.SetActive(true); 
        storePanel.SetActive(false); 
        hangarPanel.SetActive(false); 
        backgroundPanel.SetActive(false);
        shipPanel.SetActive(false); // Aggiunto
    }
    public void ShowStorePanel() 
    { 
        mainPanel.SetActive(false); 
        storePanel.SetActive(true); 
        hangarPanel.SetActive(false); 
        backgroundPanel.SetActive(false);
        shipPanel.SetActive(false); // Aggiunto
    }
    public void ShowHangarPanel() 
    { 
        mainPanel.SetActive(false); 
        storePanel.SetActive(false); 
        hangarPanel.SetActive(true); 
        backgroundPanel.SetActive(false);
        shipPanel.SetActive(false); // Aggiunto
    }
    public void ShowBackgroundPanel()
    {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        hangarPanel.SetActive(false);
        backgroundPanel.SetActive(true);
        shipPanel.SetActive(false); // Aggiunto
    }

    // --- NUOVO METODO PER IL PANNELLO NAVICELLE ---
    public void ShowShipPanel()
    {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        hangarPanel.SetActive(false);
        backgroundPanel.SetActive(false);
        shipPanel.SetActive(true);
    }
    
    public void OnBuyUpgradeButtonPressed(PermanentUpgradeType type) { ProgressionManager.Instance.BuyUpgrade(type); }
    public void OnBuySpecialUpgradeButtonPressed(AbilityID id) { ProgressionManager.Instance.BuySpecialUpgrade(id); }
    public void OnResetButtonPressed() { if (ProgressionManager.Instance != null) { ProgressionManager.Instance.ResetProgress(); } }
    
    public void StartGame() 
    {
        string selectedWeaponName = PlayerPrefs.GetString("SelectedWeapon", "Standard");
        WeaponData dataToPass = null;
        
        foreach(var wb in weaponButtons)
        {
            if(wb.weaponData.weaponName == selectedWeaponName)
            {
                dataToPass = wb.weaponData;
                break;
            }
        }
        
        if (dataToPass != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedWeapon = dataToPass;
            Debug.Log("AVVIO PARTITA CON: " + dataToPass.weaponName); // Messaggio di Debug
        }
        else
        {
            Debug.LogError("Impossibile trovare i dati per l'arma selezionata ("+ selectedWeaponName +") o il GameDataManager!");
            // Come fallback, potremmo caricare la prima arma della lista
            if(weaponButtons.Count > 0) GameDataManager.Instance.selectedWeapon = weaponButtons[0].weaponData;
        }

        SceneManager.LoadScene("GameScene"); 
    }

    // Metodo di selezione arma aggiornato
    public void SelectWeapon(WeaponData weaponData)
    {
        Debug.Log("ARMA SELEZIONATA: " + weaponData.weaponName); // Messaggio di Debug
        PlayerPrefs.SetString("SelectedWeapon", weaponData.weaponName);
        PlayerPrefs.Save();
        HighlightSelectedWeapon(weaponData.weaponName);
    }
    
    private void LoadAndHighlightSavedWeapon()
    {
        string savedWeaponName = PlayerPrefs.GetString("SelectedWeapon", "Standard");
        HighlightSelectedWeapon(savedWeaponName);
    }
    
    private void HighlightSelectedWeapon(string weaponName)
    {
        Color selectedColor = Color.green;
        Color normalColor = Color.white;

        foreach (var wb in weaponButtons)
        {
            if (wb.weaponData.weaponName == weaponName)
            {
                wb.button.image.color = selectedColor;
            }
            else
            {
                wb.button.image.color = normalColor;
            }
        }
    }

    public void CycleAbility(int direction)
    {
        if (unlockedAbilities == null || unlockedAbilities.Count <= 1) return;
        currentAbilityIndex += direction;
        currentAbilityIndex = Mathf.Clamp(currentAbilityIndex, 0, unlockedAbilities.Count - 1);
        UpdateHangarAbilityUI();
    }

    private void UpdateAllUI()
    {
        if (ProgressionManager.Instance == null) return;
        if (coinsText != null) coinsText.text = "Coins: " + ProgressionManager.Instance.GetCoins();
        if (specialCurrencyText != null) specialCurrencyText.text = "Gemme: " + ProgressionManager.Instance.GetSpecialCurrency();
        
        foreach(var panel in normalUpgradePanels) { UpdateSingleUpgradeUI(panel); }
        foreach(var panel in specialUpgradePanels) { UpdateSingleSpecialUpgradeUI(panel); }
        
        UpdateHangarAbilityUI();
    }

    private void UpdateSingleUpgradeUI(UpgradeUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.levelText == null || panel.costText == null) return;
        PermanentUpgrade upgrade = ProgressionManager.Instance.GetUpgrade(panel.upgradeType);
        if (upgrade == null) { if(panel.buyButton != null) panel.buyButton.gameObject.SetActive(false); return; }
        
        panel.levelText.text = $"Liv. {upgrade.currentLevel}/{upgrade.maxLevel}";
        if (upgrade.currentLevel >= upgrade.maxLevel) 
        { 
            panel.costText.text = "MAX"; 
            panel.buyButton.interactable = false; 
        }
        else 
        { 
            int cost = upgrade.GetNextLevelCost(); 
            panel.costText.text = cost.ToString(); 
            panel.buyButton.interactable = ProgressionManager.Instance.CanAfford(upgrade); 
        }
    }
    
    private void UpdateSingleSpecialUpgradeUI(SpecialAbilityUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.descriptionText == null || panel.costText == null) return;
        SpecialAbility ability = ProgressionManager.Instance.allSpecialAbilities.Find(a => a.abilityID == panel.abilityID);
        if (ability == null) { if(panel.buyButton != null) panel.buyButton.gameObject.SetActive(false); return; }

        panel.descriptionText.text = ability.description;
        if (ProgressionManager.Instance.IsSpecialUpgradeUnlocked(panel.abilityID))
        {
            panel.buyButton.interactable = false;
            panel.costText.gameObject.SetActive(false);
            if (panel.unlockedIndicator != null) panel.unlockedIndicator.SetActive(true);
        }
        else
        {
            panel.buyButton.interactable = ProgressionManager.Instance.CanAfford(ability);
            panel.costText.gameObject.SetActive(true);
            panel.costText.text = ability.cost.ToString();
            if (panel.unlockedIndicator != null) panel.unlockedIndicator.SetActive(false);
        }
    }
    
    private void SetupAbilitySelection()
    {
        if (ProgressionManager.Instance == null) return;
        unlockedAbilities = new List<SpecialAbility>();
        foreach (var ability in ProgressionManager.Instance.allSpecialAbilities)
        {
            if (ProgressionManager.Instance.IsSpecialUpgradeUnlocked(ability.abilityID))
            {
                unlockedAbilities.Add(ability);
            }
        }
        SpecialAbility equipped = ProgressionManager.Instance.GetEquippedAbility();
        if (equipped != null)
        {
            currentAbilityIndex = unlockedAbilities.IndexOf(equipped);
            if (currentAbilityIndex == -1) currentAbilityIndex = 0;
        }
        UpdateHangarAbilityUI();
    }

    private void UpdateHangarAbilityUI()
    {
        if (unlockedAbilities == null || unlockedAbilities.Count == 0) return;
        if (currentAbilityIndex < 0 || currentAbilityIndex >= unlockedAbilities.Count) return;

        SpecialAbility abilityToShow = unlockedAbilities[currentAbilityIndex];
        
        if (hangarAbilityIcon != null) hangarAbilityIcon.sprite = abilityToShow.icon;
        if (hangarAbilityName != null) hangarAbilityName.text = abilityToShow.abilityName;
        if (hangarAbilityDescription != null) hangarAbilityDescription.text = abilityToShow.description;

        ProgressionManager.Instance.SetEquippedAbility(abilityToShow);

        if (hangarPrevButton != null) hangarPrevButton.interactable = currentAbilityIndex > 0;
        if (hangarNextButton != null) hangarNextButton.interactable = currentAbilityIndex < unlockedAbilities.Count - 1;
    }
}