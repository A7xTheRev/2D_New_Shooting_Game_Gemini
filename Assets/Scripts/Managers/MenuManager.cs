using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    // Classe interna per collegare pulsanti e dati arma nell'Inspector
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
    public GameObject worldSelectionPanel;
    public GameObject sectorSelectionPanel;
    public GameObject missionsPanel;

    [Header("Selezione Modalità di Gioco")]
    public List<WorldData> allWorlds;
    public Transform worldButtonContainer;    // NUOVO: Container per i pulsanti dei mondi
    public GameObject worldButtonPrefab;      // NUOVO: Prefab per i pulsanti dei mondi
    public Transform sectorButtonContainer;
    public GameObject sectorButtonPrefab;

    [Header("UI Generale")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI specialCurrencyText;

    [Header("UI Record Personali")]
    public TextMeshProUGUI maxWaveText;
    public TextMeshProUGUI maxCoinsText;

    [Header("Pulsanti Arma")]
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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        int targetFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }

    void Start()
    {
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

    private void DeactivateAllPanels()
    {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        hangarPanel.SetActive(false);
        backgroundPanel.SetActive(false);
        shipPanel.SetActive(false);
        if (worldSelectionPanel != null) worldSelectionPanel.SetActive(false);
        if (sectorSelectionPanel != null) sectorSelectionPanel.SetActive(false);
        if (missionsPanel != null) missionsPanel.SetActive(false); // AGGIUNTO
    }

    public void ShowMainPanel()
    {
        DeactivateAllPanels();
        mainPanel.SetActive(true);
    }
    public void ShowStorePanel() 
    { 
        DeactivateAllPanels();
        storePanel.SetActive(true); 
    }
    public void ShowHangarPanel() 
    { 
        DeactivateAllPanels();
        hangarPanel.SetActive(true); 
    }
    public void ShowBackgroundPanel()
    {
        DeactivateAllPanels();
        backgroundPanel.SetActive(true);
    }
    public void ShowShipPanel()
    {
        DeactivateAllPanels();
        shipPanel.SetActive(true);
    }
    public void ShowWorldSelectionPanel()
    {
        if (allWorlds == null || allWorlds.Count == 0)
        {
            Debug.LogError("Nessun mondo (WorldData) è stato assegnato al MenuManager!");
            return;
        }
        DeactivateAllPanels();
        worldSelectionPanel.SetActive(true);
        PopulateWorldButtons();
    }
    public void ShowMissionsPanel()
    {
        DeactivateAllPanels();
        missionsPanel.SetActive(true);
    }
    
    // --- NUOVO METODO ---
    // Popola la UI con i pulsanti per ogni mondo
    private void PopulateWorldButtons()
    {
        foreach (Transform child in worldButtonContainer) { Destroy(child.gameObject); }

        foreach (WorldData world in allWorlds)
        {
            GameObject buttonObj = Instantiate(worldButtonPrefab, worldButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = world.worldName;
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnWorldSelected(world));
        }
    }

    // --- NUOVO METODO ---
    // Chiamato quando un giocatore clicca su un mondo
    private void OnWorldSelected(WorldData selectedWorld)
    {
        DeactivateAllPanels();
        sectorSelectionPanel.SetActive(true);
        PopulateSectorButtons(selectedWorld);
    }

    // Questo metodo ora viene chiamato da OnWorldSelected
    private void PopulateSectorButtons(WorldData world)
    {
        foreach (Transform child in sectorButtonContainer) { Destroy(child.gameObject); }
        foreach (SectorData sector in world.sectors)
        {
            GameObject buttonObj = Instantiate(sectorButtonPrefab, sectorButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = sector.sectorName;
            buttonObj.GetComponent<Button>().onClick.AddListener(() => StartStoryMode(world, sector));
        }
    }

    public void StartStoryMode(WorldData world, SectorData sector)
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedGameMode = GameMode.Story;
            GameDataManager.Instance.selectedWorld = world;
            GameDataManager.Instance.selectedSector = sector;
            LaunchGame();
        }
    }

    public void StartEndlessMode()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedGameMode = GameMode.Endless;
            GameDataManager.Instance.selectedWorld = null;
            GameDataManager.Instance.selectedSector = null;
            LaunchGame();
        }
    }

    private void LaunchGame()
    {
        string selectedWeaponName = PlayerPrefs.GetString("SelectedWeapon", "Standard");
        WeaponData dataToPass = weaponButtons.Find(wb => wb.weaponData.weaponName == selectedWeaponName)?.weaponData;
        if (dataToPass != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedWeapon = dataToPass;
        }
        else
        {
            Debug.LogError("Impossibile trovare i dati per l'arma selezionata (" + selectedWeaponName + ") o il GameDataManager!");
            if (weaponButtons.Count > 0 && GameDataManager.Instance != null) GameDataManager.Instance.selectedWeapon = weaponButtons[0].weaponData;
        }
        // --- RIGA MANCANTE AGGIUNTA QUI ---
        // Leggiamo la navicella equipaggiata dal ProgressionManager e la passiamo al GameDataManager
        if (ProgressionManager.Instance != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedShip = ProgressionManager.Instance.GetEquippedShip();
        }
        // --- FINE RIGA AGGIUNTA ---
        SceneManager.LoadScene("GameScene");
    }
    
    public void OnBuyUpgradeButtonPressed(PermanentUpgradeType type) { ProgressionManager.Instance.BuyUpgrade(type); }
    public void OnBuySpecialUpgradeButtonPressed(AbilityID id) { ProgressionManager.Instance.BuySpecialUpgrade(id); }
    public void OnResetButtonPressed() { if (ProgressionManager.Instance != null) { ProgressionManager.Instance.ResetProgress(); } }
    
    public void SelectWeapon(WeaponData weaponData)
    {
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
            if (wb.button != null)
            {
                wb.button.image.color = (wb.weaponData.weaponName == weaponName) ? selectedColor : normalColor;
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
        
        foreach (var panel in normalUpgradePanels) { UpdateSingleUpgradeUI(panel); }
        foreach (var panel in specialUpgradePanels) { UpdateSingleSpecialUpgradeUI(panel); }
        
        UpdateHangarAbilityUI();
    }
    
    private void UpdateSingleUpgradeUI(UpgradeUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.levelText == null || panel.costText == null) return;
        PermanentUpgrade upgrade = ProgressionManager.Instance.GetUpgrade(panel.upgradeType);
        if (upgrade == null) { if (panel.buyButton != null) panel.buyButton.gameObject.SetActive(false); return; }
        
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
        if (ability == null) { if (panel.buyButton != null) panel.buyButton.gameObject.SetActive(false); return; }

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