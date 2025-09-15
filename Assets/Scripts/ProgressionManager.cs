using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProgressionManager : MonoBehaviour
{
    private static ProgressionManager _instance;
    public static ProgressionManager Instance { get { if (_instance == null) { _instance = FindFirstObjectByType<ProgressionManager>(); if (_instance == null) { GameObject go = new GameObject("ProgressionManager (Auto-Generated)"); _instance = go.AddComponent<ProgressionManager>(); } } return _instance; } }

    [Header("Potenziamenti Normali")]
    public List<PermanentUpgrade> availableUpgrades = new List<PermanentUpgrade>();

    [Header("Potenziamenti Speciali")]
    public List<SpecialAbility> allSpecialAbilities = new List<SpecialAbility>();

    private int coins;
    private int specialCurrency;
    private AbilityID equippedAbilityID;
    private Dictionary<PermanentUpgradeType, int> upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
    private HashSet<AbilityID> unlockedAbilitiesSet = new HashSet<AbilityID>();

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); LoadData(); }
        else if (_instance != this) { Destroy(gameObject); }
    }

    void LoadData()
    {
        SaveData data = SaveSystem.LoadGame();
        coins = data.coins;
        specialCurrency = data.specialCurrency;
        equippedAbilityID = data.equippedAbilityID;
        upgradeLevels.Clear();
        for (int i = 0; i < data.savedUpgradeTypes.Count; i++) { upgradeLevels[data.savedUpgradeTypes[i]] = data.savedUpgradeLevels[i]; }
        foreach (var upgrade in availableUpgrades) { if (!upgradeLevels.ContainsKey(upgrade.upgradeType)) upgradeLevels[upgrade.upgradeType] = 0; upgrade.currentLevel = upgradeLevels[upgrade.upgradeType]; }
        unlockedAbilitiesSet = new HashSet<AbilityID>(data.unlockedSpecialAbilities);
    }

    void SaveData()
    {
        SaveData data = new SaveData();
        data.coins = coins;
        data.specialCurrency = specialCurrency;
        data.equippedAbilityID = equippedAbilityID;
        data.savedUpgradeTypes.Clear();
        data.savedUpgradeLevels.Clear();
        foreach (var pair in upgradeLevels) { data.savedUpgradeTypes.Add(pair.Key); data.savedUpgradeLevels.Add(pair.Value); }
        data.unlockedSpecialAbilities = unlockedAbilitiesSet.ToList();
        SaveSystem.SaveGame(data);
    }
    
    public int GetCoins() { return coins; }
    public int GetSpecialCurrency() { return specialCurrency; }
    public void AddCoins(int value) { coins += value; SaveData(); OnValuesChanged?.Invoke(); }
    public void AddSpecialCurrency(int value) { if (value <= 0) return; specialCurrency += value; SaveData(); OnValuesChanged?.Invoke(); }
    public bool CanAfford(PermanentUpgrade upgrade) { return coins >= upgrade.GetNextLevelCost(); }
    public void BuyUpgrade(PermanentUpgradeType type) { PermanentUpgrade u = GetUpgrade(type); if (u == null || u.currentLevel >= u.maxLevel) return; int c = u.GetNextLevelCost(); if (coins >= c) { coins -= c; u.currentLevel++; upgradeLevels[type] = u.currentLevel; SaveData(); OnValuesChanged?.Invoke(); } }
    public PermanentUpgrade GetUpgrade(PermanentUpgradeType type) { return availableUpgrades.Find(u => u.upgradeType == type); }
    public float GetTotalBonus(PermanentUpgradeType type) { PermanentUpgrade u = GetUpgrade(type); return u != null ? u.currentLevel * u.bonusPerLevel : 0f; }
    public bool CanAfford(SpecialAbility ability) { return specialCurrency >= ability.cost; }
    public void BuySpecialUpgrade(AbilityID id) { SpecialAbility a = allSpecialAbilities.Find(ab => ab.abilityID == id); if (a == null || IsSpecialUpgradeUnlocked(id)) return; if (specialCurrency >= a.cost) { specialCurrency -= a.cost; unlockedAbilitiesSet.Add(id); SaveData(); OnValuesChanged?.Invoke(); } }
    public bool IsSpecialUpgradeUnlocked(AbilityID id) { SpecialAbility a = allSpecialAbilities.Find(ab => ab.abilityID == id); if (a != null && a.isDefaultAbility) return true; return unlockedAbilitiesSet.Contains(id); }
    public SpecialAbility GetEquippedAbility() { if (equippedAbilityID == AbilityID.None) { return allSpecialAbilities.Find(a => a.isDefaultAbility); } return allSpecialAbilities.Find(a => a.abilityID == equippedAbilityID); }
    public void SetEquippedAbility(SpecialAbility ability) { if (ability != null) { equippedAbilityID = ability.abilityID; SaveData(); } }
    public void ResetProgress() { SaveSystem.ResetSave(); coins = 0; specialCurrency = 0; equippedAbilityID = AbilityID.None; upgradeLevels = new Dictionary<PermanentUpgradeType, int>(); unlockedAbilitiesSet = new HashSet<AbilityID>(); LoadData(); OnValuesChanged?.Invoke(); }
}