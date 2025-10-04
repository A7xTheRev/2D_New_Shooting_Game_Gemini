using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic; // <-- LIBRERIA MANCANTE AGGIUNTA QUI

public class DebugController : MonoBehaviour
{
    public static DebugController Instance { get; private set; }

    [Header("Riferimenti Pannello")]
    public GameObject debugPanel;
    public KeyCode toggleKey = KeyCode.F1;

    [Header("Riferimenti Giocatore")]
    public TMP_InputField healthInput;
    public TMP_InputField damageInput;
    public TMP_InputField attackSpeedInput;
    public TMP_Dropdown powerUpDropdown;

    [Header("Riferimenti Stage")]
    public TMP_InputField stageInput;
    public TMP_InputField timerInput;

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
        // Cerca i manager solo una volta all'inizio della scena di gioco
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene")
        {
            FindReferences();
        }
        if(debugPanel != null) debugPanel.SetActive(false);
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
        if (scene.name == "GameScene")
        {
            FindReferences();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isPanelActive = !isPanelActive;
            debugPanel.SetActive(isPanelActive);

            if (isPanelActive)
            {
                // Quando apriamo il pannello, aggiorniamo i valori e mettiamo in pausa
                Time.timeScale = 0f;
                UpdateUIFields();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
    
    // Trova i riferimenti ai componenti nella scena
    private void FindReferences()
    {
        // --- METODI OBSOLETI AGGIORNATI ---
        playerStats = FindFirstObjectByType<PlayerStats>();
        stageManager = FindFirstObjectByType<StageManager>();
        powerUpManager = FindFirstObjectByType<PowerUpManager>();
        // --- FINE AGGIORNAMENTO ---
        PopulatePowerUpDropdown();
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
        playerStats.currentHealth = int.Parse(healthInput.text);
        playerStats.maxHealth = playerStats.currentHealth; // Aggiorniamo anche la max per la UI
        playerStats.damage = int.Parse(damageInput.text);
        playerStats.attackSpeed = float.Parse(attackSpeedInput.text);
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
            var field = typeof(StageManager).GetField("survivalTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(stageManager, timeInMinutes * 60f);
            }
        }
        Debug.Log("Impostazioni Stage Aggiornate!");
    }
}