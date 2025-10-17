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
    public GameObject gameModeSelectionPanel;
    public GameObject codexPanel;
    public GameObject workshopPanel;

    [Header("Selezione Modalità di Gioco")]
    public List<WorldData> allWorlds;
    public Transform worldButtonContainer;
    public GameObject worldButtonPrefab;
    public Transform sectorButtonContainer;
    public GameObject sectorButtonPrefab;

    [Header("UI Record Personali")]
    public TextMeshProUGUI maxWaveText;
    public TextMeshProUGUI maxCoinsText;

    [Header("Pulsanti Arma")]
    // Questa lista ora serve solo per conoscere le armi disponibili, la UI è gestita altrove
    public List<WeaponSelectionButton> weaponButtons;

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

// ---- ShowMainPanel();
        UpdateAllUI();
        UpdateRecordUI();
    }

    void OnEnable() 
    { 
        if (ProgressionManager.Instance != null) { ProgressionManager.OnValuesChanged += UpdateAllUI; } 
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
// ---- mainPanel.SetActive(false);
// ---- storePanel.SetActive(false);
// ---- hangarPanel.SetActive(false);
        backgroundPanel.SetActive(false);
// ---- shipPanel.SetActive(false);
        if (worldSelectionPanel != null) worldSelectionPanel.SetActive(false);
        if (sectorSelectionPanel != null) sectorSelectionPanel.SetActive(false);
// ---- if (missionsPanel != null) missionsPanel.SetActive(false);
        if (gameModeSelectionPanel != null) gameModeSelectionPanel.SetActive(false);
        if (codexPanel != null) codexPanel.SetActive(false);
        if (missionsPanel != null) missionsPanel.SetActive(false);
    }

    public void ShowMainPanel() { DeactivateAllPanels(); mainPanel.SetActive(true); }
    public void ShowStorePanel() { DeactivateAllPanels(); storePanel.SetActive(true); }
    public void ShowHangarPanel() { DeactivateAllPanels(); hangarPanel.SetActive(true); }
    public void ShowBackgroundPanel() { DeactivateAllPanels(); backgroundPanel.SetActive(true); }
    public void ShowShipPanel() { DeactivateAllPanels(); shipPanel.SetActive(true); }
    public void ShowMissionsPanel() { DeactivateAllPanels(); missionsPanel.SetActive(true); }
    public void ShowCodexPanel() { DeactivateAllPanels(); codexPanel.SetActive(true); }
    
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
    
    public void ShowGameModeSelectionPanel()
    {
        if (gameModeSelectionPanel != null) gameModeSelectionPanel.SetActive(true);
    }

    public void HideGameModeSelectionPanel()
    {
        if (gameModeSelectionPanel != null) gameModeSelectionPanel.SetActive(false);
    }

    // --- NUOVO METODO ---
    // Popola la UI con i pulsanti per ogni mondo
    private void PopulateWorldButtons()
    {
        foreach (Transform child in worldButtonContainer) { Destroy(child.gameObject); }

        for (int i = 0; i < allWorlds.Count; i++)
        {
            WorldData currentWorld = allWorlds[i];
            GameObject buttonObj = Instantiate(worldButtonPrefab, worldButtonContainer);

            // --- NUOVA LOGICA DI SETUP ---
            bool isUnlocked = IsWorldUnlocked(i);
            
            WorldButtonUI buttonUI = buttonObj.GetComponent<WorldButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(currentWorld.worldName, isUnlocked);
            }
            else // Fallback se lo script manca
            {
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = currentWorld.worldName;
                buttonObj.GetComponent<Button>().interactable = isUnlocked;
            }

            if (isUnlocked)
            {
                buttonObj.GetComponent<Button>().onClick.AddListener(() => OnWorldSelected(currentWorld));
            }
            // --- FINE NUOVA LOGICA ---
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

    // --- METODO AGGIORNATO CON LA NUOVA LOGICA DI PROGRESSIONE ---
    private void PopulateSectorButtons(WorldData world)
    {
        foreach (Transform child in sectorButtonContainer) { Destroy(child.gameObject); }

        for (int i = 0; i < world.sectors.Count; i++)
        {
            SectorData currentSector = world.sectors[i];
            GameObject buttonObj = Instantiate(sectorButtonPrefab, sectorButtonContainer);
            
            bool isUnlocked = IsSectorUnlocked(world, i);
            int starCount = ProgressionManager.Instance.GetStarCount(currentSector.name);

            SectorButtonUI buttonUI = buttonObj.GetComponent<SectorButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(currentSector.sectorName, isUnlocked, starCount);
            }
            else
            {
                // Fallback per il vecchio sistema
                buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = currentSector.sectorName;
                buttonObj.GetComponent<Button>().interactable = isUnlocked;
            }

            if (isUnlocked)
            {
                buttonObj.GetComponent<Button>().onClick.AddListener(() => StartStoryMode(world, currentSector));
            }
        }
    }

    private bool IsWorldUnlocked(int worldIndex)
    {
        if (ProgressionManager.Instance == null) return false;

        // Il primo mondo (indice 0) è sempre sbloccato
        if (worldIndex == 0)
        {
            return true;
        }

        // Per i mondi successivi, controlla il mondo precedente
        if (worldIndex > 0)
        {
            WorldData previousWorld = allWorlds[worldIndex - 1];
            // Prendi l'ultimo settore del mondo precedente
            SectorData lastSectorOfPreviousWorld = previousWorld.sectors[previousWorld.sectors.Count - 1];
            // Il mondo si sblocca se l'ultimo settore del mondo precedente è stato completato (ha > 0 stelle)
            return ProgressionManager.Instance.GetStarCount(lastSectorOfPreviousWorld.name) > 0;
        }

        return false;
    }
    // --- NUOVO METODO PER LA LOGICA DI SBLOCCO ---
    private bool IsSectorUnlocked(WorldData currentWorld, int sectorIndex)
    {
        if (ProgressionManager.Instance == null) return false;

        int worldIndex = allWorlds.IndexOf(currentWorld);

        // Il primo settore del primo mondo è sempre sbloccato
        if (worldIndex == 0 && sectorIndex == 0)
        {
            return true;
        }

        // Se è il primo settore di un mondo (ma non il primo in assoluto)
        if (sectorIndex == 0 && worldIndex > 0)
        {
            WorldData previousWorld = allWorlds[worldIndex - 1];
            // Deve aver completato l'ultimo settore del mondo precedente
            SectorData lastSectorOfPreviousWorld = previousWorld.sectors[previousWorld.sectors.Count - 1];
            return ProgressionManager.Instance.GetStarCount(lastSectorOfPreviousWorld.name) > 0;
        }

        // Se è un settore successivo nello stesso mondo
        if (sectorIndex > 0)
        {
            SectorData previousSector = currentWorld.sectors[sectorIndex - 1];
            return ProgressionManager.Instance.GetStarCount(previousSector.name) > 0;
        }

        return false;
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
        
        if (ProgressionManager.Instance != null && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.selectedShip = ProgressionManager.Instance.GetEquippedShip();
        }
        
        SceneManager.LoadScene("GameScene");
    }
    
    public void OnBuyUpgradeButtonPressed(PermanentUpgradeType type) { ProgressionManager.Instance.BuyUpgrade(type); }
    public void OnBuySpecialUpgradeButtonPressed(AbilityID id) { ProgressionManager.Instance.BuySpecialUpgrade(id); }
    public void OnResetButtonPressed() { if (ProgressionManager.Instance != null) { ProgressionManager.Instance.ResetProgress(); } }
    
    // Questo metodo è ora chiamato dal nuovo WeaponSelectorUI
    public void SelectWeapon(WeaponData weaponData)
    {
        PlayerPrefs.SetString("SelectedWeapon", weaponData.weaponName);
        PlayerPrefs.Save();
    }

    private void UpdateAllUI()
    {
        if (ProgressionManager.Instance == null) return;
        
        foreach (var panel in normalUpgradePanels) { UpdateSingleUpgradeUI(panel); }
        foreach (var panel in specialUpgradePanels) { UpdateSingleSpecialUpgradeUI(panel); }
        
        // La chiamata a UpdateHangarAbilityUI è stata rimossa
    }
    
    // --- METODO COMPLETAMENTE RISCRITTO PER RISOLVERE GLI ERRORI ---
    private void UpdateSingleUpgradeUI(UpgradeUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.levelText == null || panel.costText == null) return;

        // 1. Ottiene i dati (ScriptableObject) del potenziamento dal ProgressionManager
        PermanentUpgradeData upgradeData = ProgressionManager.Instance.GetUpgrade(panel.upgradeType);
        if (upgradeData == null) 
        { 
            // Se non trova i dati, nasconde il pannello per sicurezza
            if (panel.buyButton != null) panel.buyButton.gameObject.SetActive(false); 
            return; 
        }
        
        // 2. Ottiene il livello CORRENTE di questo potenziamento dal ProgressionManager
        int currentLevel = ProgressionManager.Instance.GetUpgradeLevel(panel.upgradeType);

        // 3. Aggiorna il testo del livello
        panel.levelText.text = $"Liv. {currentLevel}/{upgradeData.maxLevel}";

        // 4. Controlla se il potenziamento ha raggiunto il livello massimo
        if (currentLevel >= upgradeData.maxLevel) 
        { 
            panel.costText.text = "MAX"; 
            panel.buyButton.interactable = false; 
        }
        else 
        { 
            // 5. Se non è al massimo, calcola il costo per il prossimo livello
            int cost = upgradeData.GetCostForLevel(currentLevel); 
            panel.costText.text = cost.ToString(); 

            // 6. Controlla se il giocatore può permetterselo usando la NUOVA funzione
            //    che accetta il tipo di potenziamento (PermanentUpgradeType)
            panel.buyButton.interactable = ProgressionManager.Instance.CanAfford(panel.upgradeType); 
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
}