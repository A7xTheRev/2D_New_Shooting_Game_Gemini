using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

public class DebugController : MonoBehaviour
{
    public static DebugController Instance { get; private set; }

    [Header("Riferimenti Pannello")]
    public string debugPanelName = "DebugPanel"; // Cercheremo il pannello con questo nome
    public KeyCode toggleKey = KeyCode.F1;

    // Riferimenti interni, non più pubblici
    private GameObject debugPanel;
    private TMP_InputField healthInput;
    private TMP_InputField damageInput;
    private TMP_InputField attackSpeedInput;
    private TMP_Dropdown powerUpDropdown;
    private TMP_InputField stageInput;
    private TMP_InputField timerInput;

    // --- NUOVI RIFERIMENTI PER I MODULI ---
    private TMP_InputField moduleIDInput;
    private TMP_InputField moduleQuantityInput;
    private Button addModuleButton;
    // --- FINE NUOVI RIFERIMENTI ---

    private PlayerStats playerStats;
    private StageManager stageManager;
    private PowerUpManager powerUpManager;

    private bool isPanelActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Non facciamo nulla in Start, aspetteremo che la scena di gioco sia caricata
    }

    // Aggiungiamo un listener per ricaricare i riferimenti quando si carica la GameScene
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Ogni volta che carichiamo una scena, resettiamo i nostri riferimenti "fantasma"
        playerStats = null;
        stageManager = null;
        powerUpManager = null;
        debugPanel = null;
        isPanelActive = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isPanelActive = !isPanelActive;

            // Se stiamo attivando il pannello, assicuriamoci di avere tutti i riferimenti
            if (isPanelActive)
            {
                // Cerchiamo i riferimenti solo la prima volta che apriamo il pannello in una scena
                if (debugPanel == null)
                {
                    if (!FindAllReferencesInScene())
                    {
                        isPanelActive = false; // Se non troviamo il pannello, non lo attiviamo
                        return;
                    }
                }
                
                debugPanel.SetActive(true);
                Time.timeScale = 0f;
                UpdateUIFields();
            }
            else
            {
                // Se il pannello esiste, lo disattiviamo
                if(debugPanel != null)
                {
                    debugPanel.SetActive(false);
                }
                Time.timeScale = 1f;
            }
        }
    }

    // Trova tutti i riferimenti necessari nella scena corrente
    private bool FindAllReferencesInScene()
    {
        // --- LOGICA DI RICERCA CORRETTA ---
        // Troviamo il Canvas principale della scena
        Canvas mainCanvas = FindFirstObjectByType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("DebugController: Nessun Canvas trovato nella scena!");
            return false;
        }

        // Cerchiamo il pannello per nome tra i figli del Canvas (anche quelli inattivi)
        Transform panelTransform = mainCanvas.transform.Find(debugPanelName);
        if (panelTransform == null)
        {
            Debug.LogError($"DebugController: Pannello con nome '{debugPanelName}' non trovato come figlio del Canvas principale!");
            return false;
        }
        debugPanel = panelTransform.gameObject;
        // --- FINE LOGICA CORRETTA ---

        // Trova tutti i componenti UI come figli del pannello
        healthInput = debugPanel.transform.Find("HealthInput")?.GetComponent<TMP_InputField>();
        damageInput = debugPanel.transform.Find("DamageInput")?.GetComponent<TMP_InputField>();
        attackSpeedInput = debugPanel.transform.Find("AttackSpeedInput")?.GetComponent<TMP_InputField>();
        powerUpDropdown = debugPanel.transform.Find("PowerUpDropdown")?.GetComponent<TMP_Dropdown>();
        stageInput = debugPanel.transform.Find("StageInput")?.GetComponent<TMP_InputField>();
        timerInput = debugPanel.transform.Find("TimerInput")?.GetComponent<TMP_InputField>();
        
        // --- NUOVA LOGICA PER TROVARE I CAMPI DEI MODULI ---
        moduleIDInput = debugPanel.transform.Find("ModuleIDInput")?.GetComponent<TMP_InputField>();
        moduleQuantityInput = debugPanel.transform.Find("ModuleQuantityInput")?.GetComponent<TMP_InputField>();
        addModuleButton = debugPanel.transform.Find("AddModuleButton")?.GetComponent<Button>();
        // --- FINE NUOVA LOGICA ---
        
        // Collega gli eventi ai pulsanti
        debugPanel.transform.Find("ApplyPlayerStatsButton")?.GetComponent<Button>().onClick.AddListener(ApplyPlayerStats);
        debugPanel.transform.Find("HealButton")?.GetComponent<Button>().onClick.AddListener(HealPlayer);
        debugPanel.transform.Find("AddPowerUpButton")?.GetComponent<Button>().onClick.AddListener(AddPowerUp);
        debugPanel.transform.Find("ApplyStageSettingsButton")?.GetComponent<Button>().onClick.AddListener(ApplyStageSettings);

        // --- NUOVO LISTENER PER IL PULSANTE DEI MODULI ---
        if (addModuleButton != null)
        {
            addModuleButton.onClick.AddListener(AddModuleDebug);
        }
        // --- FINE NUOVO LISTENER ---

        // Trova i manager di scena
        playerStats = FindFirstObjectByType<PlayerStats>();
        stageManager = FindFirstObjectByType<StageManager>();
        powerUpManager = FindFirstObjectByType<PowerUpManager>();

        PopulatePowerUpDropdown();
        return true;
    }

    // Carica i valori attuali nella UI del pannello
    private void UpdateUIFields()
    {
        if (playerStats != null)
        {
            healthInput.text = playerStats.currentHealth.ToString();
            damageInput.text = playerStats.damage.ToString();
            attackSpeedInput.text = playerStats.attackSpeed.ToString("F2");
        }
        if (stageManager != null)
        {
            stageInput.text = stageManager.stageNumber.ToString();
            timerInput.text = (stageManager.GetSurvivalTime() / 60f).ToString("F2"); // In minuti
        }
    }

    private void PopulatePowerUpDropdown()
    {
        if (powerUpManager != null && powerUpDropdown != null && powerUpManager.allPowerUps != null)
        {
            powerUpDropdown.ClearOptions();
            List<string> powerUpNames = powerUpManager.allPowerUps.Select(p => p.displayName).ToList();
            powerUpDropdown.AddOptions(powerUpNames);
        }
    }

    // --- METODI CHIAMATI DAI PULSANTI DELLA UI ---

    public void ApplyPlayerStats()
    {
        if (playerStats == null) return;
        
        // Usiamo TryParse per evitare errori se l'input non è un numero valido
        if(int.TryParse(healthInput.text, out int health)) 
        {
            playerStats.currentHealth = health;
            playerStats.maxHealth = health; // Aggiorniamo anche la max per la UI
        }
        if(int.TryParse(damageInput.text, out int damage)) 
        {
            playerStats.damage = damage;
        }
        if(float.TryParse(attackSpeedInput.text, out float speed)) 
        {
            playerStats.attackSpeed = speed;
        }

        Debug.Log("Statistiche Giocatore Aggiornate!");
    }

    public void HealPlayer()
    {
        if (playerStats == null) return;
        playerStats.Heal(9999);
    }
    
    public void AddPowerUp()
    {
        if (playerStats == null || powerUpManager == null || powerUpManager.allPowerUps.Count == 0) return;
        
        int selectedIndex = powerUpDropdown.value;
        if(selectedIndex < 0 || selectedIndex >= powerUpManager.allPowerUps.Count) return;

        PowerUpEffect selectedPowerUp = powerUpManager.allPowerUps[selectedIndex];
        
        playerStats.AcquirePowerUp(selectedPowerUp);
        Debug.Log($"Aggiunto Power-Up: {selectedPowerUp.displayName}");
    }

    public void ApplyStageSettings()
    {
        if (stageManager == null) return;
        
        if(int.TryParse(stageInput.text, out int stage))
        {   
            stageManager.stageNumber = stage;
        }

        if(float.TryParse(timerInput.text, out float timeInMinutes))
        {
            // --- LOGICA DI SINCRONIZZAZIONE CORRETTA ---
            float newSurvivalTime = timeInMinutes * 60f;

            // 1. Aggiorna il timer di sopravvivenza
            var survivalTimerField = typeof(StageManager).GetField("survivalTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (survivalTimerField != null)
            {
                survivalTimerField.SetValue(stageManager, newSurvivalTime);
            }

            // 2. Ricalcola e aggiorna il timer del boss
            var bossTimerField = typeof(StageManager).GetField("bossTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (bossTimerField != null)
            {
                float bossIntervalSeconds = stageManager.bossIntervalInMinutes * 60f;
                if (bossIntervalSeconds > 0)
                {
                float timeSinceLastBoss = newSurvivalTime % bossIntervalSeconds;
                // Il tempo rimanente è l'intervallo totale meno il tempo già passato
                float newBossTimerValue = bossIntervalSeconds - timeSinceLastBoss;
                bossTimerField.SetValue(stageManager, newBossTimerValue);
                }
            }
        }
        Debug.Log("Impostazioni Stage Aggiornate!");
    }

    // --- NUOVO METODO PER AGGIUNGERE MODULI ---
    public void AddModuleDebug()
    {
        if (ProgressionManager.Instance == null)
        {
            Debug.LogError("ProgressionManager not found!");
            return;
        }

        string moduleID = moduleIDInput.text;
        if (string.IsNullOrEmpty(moduleID))
        {
            Debug.LogWarning("Module ID cannot be empty.");
            return;
        }

        if (!int.TryParse(moduleQuantityInput.text, out int quantity) || quantity <= 0)
        {
            quantity = 1; // Default to 1 if input is invalid or empty
        }

        // Check if the module ID is valid
        if (ProgressionManager.Instance.GetModuleDataByID(moduleID) == null)
        {
            Debug.LogError($"Module with ID '{moduleID}' not found in ProgressionManager's list of all modules.");
            return;
        }

        ProgressionManager.Instance.AddModule(moduleID, quantity);
        Debug.Log($"Successfully added {quantity}x module(s) with ID: '{moduleID}'");
    }
    // --- FINE NUOVO METODO ---
}