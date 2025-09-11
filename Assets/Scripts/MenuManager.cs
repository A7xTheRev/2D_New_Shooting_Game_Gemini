using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("UI Generale")]
    public TextMeshProUGUI coinsText;

    [Header("Pulsanti Arma")]
    public Button laserButton;
    public Button standardButton;
    public Button missileButton;

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown;

    [Header("Pannelli Potenziamenti Permanenti")]
    public UpgradeUIPanel healthUpgradeUI;
    public UpgradeUIPanel damageUpgradeUI;
    public UpgradeUIPanel attackSpeedUpgradeUI;
    public UpgradeUIPanel moveSpeedUpgradeUI;

    // Classe di supporto per organizzare gli elementi UI di ogni potenziamento
    [System.Serializable]
    public class UpgradeUIPanel
    {
        public PermanentUpgradeType upgradeType;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Button buyButton;
    }
    
    public static event Action<string> OnWeaponChanged;
    private static string selectedWeapon = "Standard";

    void Start()
    {
        // Applica le impostazioni salvate all'avvio
        string savedWeapon = PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
        HighlightSelectedWeapon(savedWeapon);
        SetupFPSDropdown();
        
        // Aggiorna tutta la UI all'avvio
        UpdateAllUI();
    }

    void OnEnable()
    {
        // Si iscrive all'evento del ProgressionManager.
        // Ogni volta che le monete o i livelli cambiano, UpdateAllUI() verrà chiamata automaticamente.
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateAllUI;
        }
        UpdateAllUI();
    }

    void OnDisable()
    {
        // Rimuove l'iscrizione per evitare errori quando si cambia scena
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateAllUI;
        }
    }
    
    // Metodo unico per aggiornare tutta la UI del menu
    void UpdateAllUI()
    {
        if (ProgressionManager.Instance == null) return;
        
        // Aggiorna il testo delle monete
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + ProgressionManager.Instance.GetCoins();
        }
        
        // Aggiorna tutti i pannelli dei potenziamenti
        UpdateSingleUpgradeUI(healthUpgradeUI);
        UpdateSingleUpgradeUI(damageUpgradeUI);
        UpdateSingleUpgradeUI(attackSpeedUpgradeUI);
        UpdateSingleUpgradeUI(moveSpeedUpgradeUI);
    }

    // Aggiorna la UI per un singolo potenziamento
    void UpdateSingleUpgradeUI(UpgradeUIPanel panel)
    {
        if (panel == null || panel.levelText == null || panel.costText == null || panel.buyButton == null) return;

        PermanentUpgrade upgrade = ProgressionManager.Instance.GetUpgrade(panel.upgradeType);
        if (upgrade == null) return;

        // Aggiorna il testo del livello (es. "Liv. 2/10")
        panel.levelText.text = $"Liv. {upgrade.currentLevel}/{upgrade.maxLevel}";

        // Aggiorna il costo e lo stato del pulsante
        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            panel.costText.text = "MAX";
            panel.buyButton.interactable = false; // Disabilita il pulsante se il livello è massimo
        }
        else
        {
            int cost = upgrade.GetNextLevelCost();
            panel.costText.text = cost.ToString();
            // Il pulsante è utilizzabile solo se il giocatore può permettersi il potenziamento
            panel.buyButton.interactable = ProgressionManager.Instance.CanAfford(upgrade);
        }
    }

    // Metodo chiamato dai pulsanti di acquisto
    public void OnBuyUpgradeButtonPressed(int typeAsInt)
    {
        // Converte l'intero ricevuto dal pulsante nell'enum corretto
        PermanentUpgradeType type = (PermanentUpgradeType)typeAsInt;
        ProgressionManager.Instance.BuyUpgrade(type);
    }
    
    // Metodo da collegare a un pulsante di "Reset" nella UI
    public void OnResetButtonPressed()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.ResetProgress();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void SelectWeapon(string weaponName)
    {
        selectedWeapon = weaponName;
        PlayerPrefs.SetString("SelectedWeapon", weaponName);
        PlayerPrefs.Save();
        HighlightSelectedWeapon(weaponName);
        OnWeaponChanged?.Invoke(weaponName);
    }

    private void HighlightSelectedWeapon(string weaponName)
    {
        Color selectedColor = Color.green;
        Color normalColor = Color.white;

        if (laserButton != null)
            laserButton.image.color = (weaponName == "Laser") ? selectedColor : normalColor;
        
        if (standardButton != null)
            standardButton.image.color = (weaponName == "Standard") ? selectedColor : normalColor;
        
        if (missileButton != null)
            missileButton.image.color = (weaponName == "Missile") ? selectedColor : normalColor;
    }

    public static string GetSelectedWeapon()
    {
        return PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
    }
    
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