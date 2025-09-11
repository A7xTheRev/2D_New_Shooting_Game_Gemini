using UnityEngine;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [Header("Potenziamenti Permanenti")]
    public List<PermanentUpgrade> availableUpgrades;

    private int coins;
    // --- CORREZIONE QUI ---
    // Inizializziamo il dizionario per evitare l'errore NullReferenceException
    private Dictionary<PermanentUpgradeType, int> upgradeLevels = new Dictionary<PermanentUpgradeType, int>();

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
        
        upgradeLevels.Clear();
        for (int i = 0; i < data.savedUpgradeTypes.Count; i++)
        {
            PermanentUpgradeType type = data.savedUpgradeTypes[i];
            int level = data.savedUpgradeLevels[i];
            upgradeLevels[type] = level;
        }

        foreach (var upgrade in availableUpgrades)
        {
            if (!upgradeLevels.ContainsKey(upgrade.upgradeType))
            {
                upgradeLevels[upgrade.upgradeType] = 0;
            }
            upgrade.currentLevel = upgradeLevels[upgrade.upgradeType];
        }
    }

    void SaveData()
    {
        SaveData data = new SaveData();
        data.coins = coins;

        foreach (var pair in upgradeLevels)
        {
            data.savedUpgradeTypes.Add(pair.Key);
            data.savedUpgradeLevels.Add(pair.Value);
        }

        SaveSystem.SaveGame(data);
    }

    public int GetCoins() { return coins; }
    public void AddCoins(int value)
    {
        coins += value;
        SaveData();
        OnValuesChanged?.Invoke();
    }

    public bool CanAfford(PermanentUpgrade upgrade)
    {
        return coins >= upgrade.GetNextLevelCost();
    }

    public void BuyUpgrade(PermanentUpgradeType type)
    {
        PermanentUpgrade upgradeToBuy = GetUpgrade(type);
        if (upgradeToBuy == null || upgradeToBuy.currentLevel >= upgradeToBuy.maxLevel) return;

        int cost = upgradeToBuy.GetNextLevelCost();
        if (coins >= cost)
        {
            coins -= cost;
            upgradeToBuy.currentLevel++;
            upgradeLevels[type] = upgradeToBuy.currentLevel;
            
            SaveData();
            OnValuesChanged?.Invoke();
            Debug.Log($"Acquistato {upgradeToBuy.upgradeName} Livello {upgradeToBuy.currentLevel} per {cost} monete.");
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

    public void ResetProgress()
    {
        SaveSystem.ResetSave();
        coins = 0;
        upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
        LoadData();
        OnValuesChanged?.Invoke();
        Debug.Log("Progresso resettato!");
    }
}