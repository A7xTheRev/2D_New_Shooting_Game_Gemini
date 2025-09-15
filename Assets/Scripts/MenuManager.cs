using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Pannelli Schermate")]
    public GameObject mainPanel;
    public GameObject storePanel;
    public GameObject hangarPanel;

    [Header("UI Generale")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI specialCurrencyText;

    [Header("Pulsanti Arma")]
    public Button laserButton;
    public Button standardButton;
    public Button missileButton;

    [Header("Selezione Abilità Speciale (Hangar)")]
    public Image hangarAbilityIcon;
    public TextMeshProUGUI hangarAbilityName;
    public TextMeshProUGUI hangarAbilityDescription;
    public Button hangarPrevButton;
    public Button hangarNextButton;

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown;

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
    
    public static event Action<string> OnWeaponChanged;
    private static string selectedWeapon = "Standard";
    
    private int currentAbilityIndex = 0;
    private List<SpecialAbility> unlockedAbilities;

    void Start()
    {
        // Collega i pulsanti di acquisto dinamicamente per evitare errori
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

        ShowMainPanel();
        string savedWeapon = PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
        HighlightSelectedWeapon(savedWeapon);
        SetupFPSDropdown();
        UpdateAllUI();
    }

    void OnEnable()
    {
        if (ProgressionManager.Instance != null) { ProgressionManager.OnValuesChanged += UpdateAllUI; }
        SetupAbilitySelection();
        UpdateAllUI();
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null) { ProgressionManager.OnValuesChanged -= UpdateAllUI; }
    }
    
    public void ShowMainPanel() { mainPanel.SetActive(true); storePanel.SetActive(false); hangarPanel.SetActive(false); }
    public void ShowStorePanel() { mainPanel.SetActive(false); storePanel.SetActive(true); hangarPanel.SetActive(false); }
    public void ShowHangarPanel() { mainPanel.SetActive(false); storePanel.SetActive(false); hangarPanel.SetActive(true); }

    public void OnBuyUpgradeButtonPressed(PermanentUpgradeType type) { ProgressionManager.Instance.BuyUpgrade(type); }
    public void OnBuySpecialUpgradeButtonPressed(AbilityID id) { ProgressionManager.Instance.BuySpecialUpgrade(id); }
    public void OnResetButtonPressed() { if (ProgressionManager.Instance != null) { ProgressionManager.Instance.ResetProgress(); } }
    
    public void StartGame() { SceneManager.LoadScene("GameScene"); }

    public void SelectWeapon(string weaponName)
    {
        selectedWeapon = weaponName;
        PlayerPrefs.SetString("SelectedWeapon", weaponName);
        PlayerPrefs.Save();
        HighlightSelectedWeapon(weaponName);
        OnWeaponChanged?.Invoke(weaponName);
    }
    
    public void CycleAbility(int direction)
    {
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
            // --- LOGICA MODIFICATA ---
            // Aggiungi alla lista solo se l'abilità è sbloccata E se è di tipo "Active"
            if (ProgressionManager.Instance.IsSpecialUpgradeUnlocked(ability.abilityID) && ability.behaviorType == AbilityBehaviorType.Active)
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

    private void HighlightSelectedWeapon(string weaponName)
    {
        Color selectedColor = Color.green;
        Color normalColor = Color.white;
        if (laserButton != null) laserButton.image.color = (weaponName == "Laser") ? selectedColor : normalColor;
        if (standardButton != null) standardButton.image.color = (weaponName == "Standard") ? selectedColor : normalColor;
        if (missileButton != null) missileButton.image.color = (weaponName == "Missile") ? selectedColor : normalColor;
    }
    
    public static string GetSelectedWeapon() { return PlayerPrefs.GetString("SelectedWeapon", selectedWeapon); }
    
    private void SetupFPSDropdown()
    {
        if (fpsDropdown == null) return;
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new List<string> { "Unlimited", "60 FPS", "90 FPS", "120 FPS" });
        int savedFPS = PlayerPrefs.GetInt("TargetFPS", -1);
        int index = FPSValueToIndex(savedFPS);
        fpsDropdown.value = index;
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
        ApplyFPS(savedFPS);
    }

    private void OnFPSChanged(int index)
    {
        int fps = IndexToFPSValue(index);
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
        ApplyFPS(fps);
    }

    private void ApplyFPS(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
    }

    private int FPSValueToIndex(int fps)
    {
        switch (fps)
        {
            case 60: return 1;
            case 90: return 2;
            case 120: return 3;
            default: return 0;
        }
    }

    private int IndexToFPSValue(int index)
    {
        switch (index)
        {
            case 1: return 60;
            case 2: return 90;
            case 3: return 120;
            default: return -1;
        }
    }
}