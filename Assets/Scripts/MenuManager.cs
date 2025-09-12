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

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown;

    [Header("Pannelli Potenziamenti Normali")]
    public UpgradeUIPanel healthUpgradeUI;
    public UpgradeUIPanel damageUpgradeUI;
    public UpgradeUIPanel attackSpeedUpgradeUI;
    public UpgradeUIPanel moveSpeedUpgradeUI;

    [Header("Pannelli Potenziamenti Speciali")]
    public SpecialUpgradeUIPanel secondChanceUpgradeUI;
    public SpecialUpgradeUIPanel startingPowerUpUI;
    public SpecialUpgradeUIPanel rerollUpgradeUI;

    [System.Serializable]
    public class UpgradeUIPanel
    {
        public PermanentUpgradeType upgradeType;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public Button buyButton;
    }

    [System.Serializable]
    public class SpecialUpgradeUIPanel
    {
        public SpecialUpgradeType upgradeType;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI costText;
        public Button buyButton;
        public GameObject unlockedIndicator;
    }
    
    public static event Action<string> OnWeaponChanged;
    private static string selectedWeapon = "Standard";

    void Start()
    {
        ShowMainPanel();
        string savedWeapon = PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
        HighlightSelectedWeapon(savedWeapon);
        SetupFPSDropdown();
        UpdateAllUI();
    }

    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateAllUI;
        }
        UpdateAllUI();
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateAllUI;
        }
    }
    
    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        storePanel.SetActive(false);
        hangarPanel.SetActive(false);
    }

    public void ShowStorePanel()
    {
        mainPanel.SetActive(false);
        storePanel.SetActive(true);
        hangarPanel.SetActive(false);
    }

    public void ShowHangarPanel()
    {
        mainPanel.SetActive(false);
        storePanel.SetActive(false);
        hangarPanel.SetActive(true);
    }

    public void OnBuyUpgradeButtonPressed(int typeAsInt)
    {
        PermanentUpgradeType type = (PermanentUpgradeType)typeAsInt;
        ProgressionManager.Instance.BuyUpgrade(type);
    }
    
    public void OnBuySpecialUpgradeButtonPressed(int typeAsInt)
    {
        SpecialUpgradeType type = (SpecialUpgradeType)typeAsInt;
        ProgressionManager.Instance.BuySpecialUpgrade(type);
    }

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

    private void UpdateAllUI()
    {
        if (ProgressionManager.Instance == null) return;
        
        if (coinsText != null)
            coinsText.text = "Coins: " + ProgressionManager.Instance.GetCoins();
        if (specialCurrencyText != null)
            specialCurrencyText.text = "Gemme: " + ProgressionManager.Instance.GetSpecialCurrency();
        
        UpdateSingleUpgradeUI(healthUpgradeUI);
        UpdateSingleUpgradeUI(damageUpgradeUI);
        UpdateSingleUpgradeUI(attackSpeedUpgradeUI);
        UpdateSingleUpgradeUI(moveSpeedUpgradeUI);
        
        UpdateSingleSpecialUpgradeUI(secondChanceUpgradeUI);
        UpdateSingleSpecialUpgradeUI(startingPowerUpUI);
        UpdateSingleSpecialUpgradeUI(rerollUpgradeUI);
    }

    private void UpdateSingleUpgradeUI(UpgradeUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.levelText == null || panel.costText == null) return;
        PermanentUpgrade upgrade = ProgressionManager.Instance.GetUpgrade(panel.upgradeType);
        if (upgrade == null) return;

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
    
    private void UpdateSingleSpecialUpgradeUI(SpecialUpgradeUIPanel panel)
    {
        if (panel == null || panel.buyButton == null || panel.descriptionText == null || panel.costText == null) return;
        SpecialUpgrade upgrade = ProgressionManager.Instance.availableSpecialUpgrades.Find(u => u.upgradeType == panel.upgradeType);
        if (upgrade == null) return;

        panel.descriptionText.text = upgrade.description;

        if (upgrade.isUnlocked)
        {
            panel.buyButton.interactable = false;
            panel.costText.gameObject.SetActive(false);
            if (panel.unlockedIndicator != null) panel.unlockedIndicator.SetActive(true);
        }
        else
        {
            panel.buyButton.interactable = ProgressionManager.Instance.CanAfford(upgrade);
            panel.costText.gameObject.SetActive(true);
            panel.costText.text = upgrade.cost.ToString();
            if (panel.unlockedIndicator != null) panel.unlockedIndicator.SetActive(false);
        }
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