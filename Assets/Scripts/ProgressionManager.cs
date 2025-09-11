using UnityEngine;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [Header("Potenziamenti Normali")]
    public List<PermanentUpgrade> availableUpgrades;

    [Header("Potenziamenti Speciali")]
    public List<SpecialUpgrade> availableSpecialUpgrades;

    private int coins;
    private int specialCurrency;

    private Dictionary<PermanentUpgradeType, int> upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
    private Dictionary<SpecialUpgradeType, bool> specialUpgradesUnlocked = new Dictionary<SpecialUpgradeType, bool>();

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadData()
    {
        SaveData data = SaveSystem.LoadGame();
        coins = data.coins;
        specialCurrency = data.specialCurrency;

        upgradeLevels.Clear();
        for (int i = 0; i < data.savedUpgradeTypes.Count; i++)
        {
            upgradeLevels[data.savedUpgradeTypes[i]] = data.savedUpgradeLevels[i];
        }
        foreach (var upgrade in availableUpgrades)
        {
            if (!upgradeLevels.ContainsKey(upgrade.upgradeType)) upgradeLevels[upgrade.upgradeType] = 0;
            upgrade.currentLevel = upgradeLevels[upgrade.upgradeType];
        }

        specialUpgradesUnlocked.Clear();
        foreach (var unlocked in data.unlockedSpecialUpgrades)
        {
            specialUpgradesUnlocked[unlocked] = true;
        }
        foreach (var upgrade in availableSpecialUpgrades)
        {
            if (!specialUpgradesUnlocked.ContainsKey(upgrade.upgradeType)) specialUpgradesUnlocked[upgrade.upgradeType] = false;
            upgrade.isUnlocked = specialUpgradesUnlocked[upgrade.upgradeType];
        }
    }

    void SaveData()
    {
        SaveData data = new SaveData();
        data.coins = coins;
        data.specialCurrency = specialCurrency;

        data.savedUpgradeTypes.Clear();
        data.savedUpgradeLevels.Clear();
        foreach (var pair in upgradeLevels)
        {
            data.savedUpgradeTypes.Add(pair.Key);
            data.savedUpgradeLevels.Add(pair.Value);
        }

        data.unlockedSpecialUpgrades.Clear();
        foreach (var pair in specialUpgradesUnlocked)
        {
            if (pair.Value == true)
            {
                data.unlockedSpecialUpgrades.Add(pair.Key);
            }
        }

        SaveSystem.SaveGame(data);
    }
    
    public int GetCoins() { return coins; }
    public int GetSpecialCurrency() { return specialCurrency; }

    public void AddCoins(int value)
    {
        coins += value;
        SaveData();
        OnValuesChanged?.Invoke();
    }
    
    public void AddSpecialCurrency(int value)
    {
        if (value <= 0) return;
        specialCurrency += value;
        SaveData();
        OnValuesChanged?.Invoke();
    }

    public bool CanAfford(PermanentUpgrade upgrade)
    {
        return coins >= upgrade.GetNextLevelCost();
    }

    public void BuyUpgrade(PermanentUpgradeType type)
    {
        PermanentUpgrade upgradeToBuy = availableUpgrades.Find(u => u.upgradeType == type);
        if (upgradeToBuy == null || upgradeToBuy.currentLevel >= upgradeToBuy.maxLevel) return;

        int cost = upgradeToBuy.GetNextLevelCost();
        if (coins >= cost)
        {
            coins -= cost;
            upgradeToBuy.currentLevel++;
            upgradeLevels[type] = upgradeToBuy.currentLevel;
            
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }
    
    public PermanentUpgrade GetUpgrade(PermanentUpgradeType type)
    {
        return availableUpgrades.Find(u => u.upgradeType == type);
    }

    public float GetTotalBonus(PermanentUpgradeType type)
    {
        PermanentUpgrade upgrade = GetUpgrade(type);
        if (upgrade != null)
        {
            return upgrade.currentLevel * upgrade.bonusPerLevel;
        }
        return 0f;
    }
    
    public bool CanAfford(SpecialUpgrade upgrade)
    {
        return specialCurrency >= upgrade.cost;
    }

    public void BuySpecialUpgrade(SpecialUpgradeType type)
    {
        SpecialUpgrade upgradeToBuy = availableSpecialUpgrades.Find(u => u.upgradeType == type);
        if (upgradeToBuy == null || upgradeToBuy.isUnlocked) return;

        if (specialCurrency >= upgradeToBuy.cost)
        {
            specialCurrency -= upgradeToBuy.cost;
            upgradeToBuy.isUnlocked = true;
            specialUpgradesUnlocked[type] = true;
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }

    public bool IsSpecialUpgradeUnlocked(SpecialUpgradeType type)
    {
        if (specialUpgradesUnlocked.ContainsKey(type))
        {
            return specialUpgradesUnlocked[type];
        }
        return false;
    }

    public void ResetProgress()
    {
        SaveSystem.ResetSave();
        coins = 0;
        specialCurrency = 0;
        upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
        specialUpgradesUnlocked = new Dictionary<SpecialUpgradeType, bool>();
        LoadData();
        OnValuesChanged?.Invoke();
        Debug.Log("Progresso resettato!");
    }
}