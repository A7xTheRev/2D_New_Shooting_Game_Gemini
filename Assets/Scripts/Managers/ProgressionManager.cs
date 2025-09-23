using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProgressionManager : MonoBehaviour
{
    private static ProgressionManager _instance;
    private static bool isQuitting = false;

    public static ProgressionManager Instance
    {
        get
        {
            if (isQuitting)
            {
                Debug.LogWarning("ProgressionManager Instance richiesto durante la chiusura, restituisco null.");
                return null;
            }
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ProgressionManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ProgressionManager (Auto-Generated)");
                    _instance = singletonObject.AddComponent<ProgressionManager>();
                }
            }
            return _instance;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    [Header("Potenziamenti Normali")] public List<PermanentUpgrade> availableUpgrades = new List<PermanentUpgrade>();
    [Header("Potenziamenti Speciali")] public List<SpecialAbility> allSpecialAbilities = new List<SpecialAbility>();
    [Tooltip("Trascina qui tutti gli asset ShipData di tutte le navicelle del gioco.")]
    public List<ShipData> allShips = new List<ShipData>();
    private int coins; private int specialCurrency; private AbilityID equippedAbilityID;
    // --- NUOVE VARIABILI PER I RECORD ---
    private int maxWaveReached;
    private int maxCoinsInSession;
    // --- FINE NUOVE VARIABILI ---
    private Dictionary<PermanentUpgradeType, int> upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
    private HashSet<AbilityID> unlockedAbilitiesSet = new HashSet<AbilityID>();

    // --- NUOVI CAMPI PER GESTIRE LE NAVICELLE ---
    private string equippedShipName;
    private HashSet<string> unlockedShipNamesSet = new HashSet<string>();
    // --- FINE NUOVI CAMPI ---

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (_instance == null) 
        { 
            _instance = this; 
            DontDestroyOnLoad(gameObject); // RIMOSSA DA QUI
            LoadData(); 
        }
        else if (_instance != this) 
        { 
            Destroy(gameObject); 
        }
    }

    void LoadData()
    {
        SaveData data = SaveSystem.LoadGame();
        coins = data.coins;
        specialCurrency = data.specialCurrency;
        equippedAbilityID = data.equippedAbilityID;
        // Carica i record
        maxWaveReached = data.maxWaveReached;
        maxCoinsInSession = data.maxCoinsInSession;

        // Carica navicelle
        equippedShipName = data.equippedShipName;
        unlockedShipNamesSet = new HashSet<string>(data.unlockedShipNames);

        upgradeLevels.Clear();
        for (int i = 0; i < data.savedUpgradeTypes.Count; i++) { upgradeLevels[data.savedUpgradeTypes[i]] = data.savedUpgradeLevels[i]; }
        foreach (var upgrade in availableUpgrades) { if (!upgradeLevels.ContainsKey(upgrade.upgradeType)) upgradeLevels[upgrade.upgradeType] = 0; upgrade.currentLevel = upgradeLevels[upgrade.upgradeType]; }
        unlockedAbilitiesSet = new HashSet<AbilityID>(data.unlockedSpecialAbilities);

        // Assicurati che la navicella di default sia sempre sbloccata
        ShipData defaultShip = allShips.Find(s => s.isDefaultShip);
        if (defaultShip != null)
        {
            unlockedShipNamesSet.Add(defaultShip.shipName);
            if (string.IsNullOrEmpty(equippedShipName))
            {
                equippedShipName = defaultShip.shipName;
            }
        }
    }

    void SaveData()
    {
        SaveData data = new SaveData();
        data.coins = coins;
        data.specialCurrency = specialCurrency;
        data.equippedAbilityID = equippedAbilityID;
        // Salva i record
        data.maxWaveReached = maxWaveReached;
        data.maxCoinsInSession = maxCoinsInSession;

        // Salva navicelle
        data.equippedShipName = equippedShipName;
        data.unlockedShipNames = unlockedShipNamesSet.ToList();

        data.savedUpgradeTypes.Clear();
        data.savedUpgradeLevels.Clear();
        foreach (var pair in upgradeLevels) { data.savedUpgradeTypes.Add(pair.Key); data.savedUpgradeLevels.Add(pair.Value); }
        data.unlockedSpecialAbilities = unlockedAbilitiesSet.ToList();
        SaveSystem.SaveGame(data);
    }

    // --- NUOVI METODI PUBBLICI PER I RECORD ---
    public int GetMaxWave() { return maxWaveReached; }
    public int GetMaxCoins() { return maxCoinsInSession; }

    // Questo metodo controlla se abbiamo stabilito nuovi record e restituisce true se è così
    public bool CheckForNewHighScores(int currentWave, int currentCoins)
    {
        bool newRecord = false;

        if (currentWave > maxWaveReached)
        {
            maxWaveReached = currentWave;
            newRecord = true;
        }
        if (currentCoins > maxCoinsInSession)
        {
            maxCoinsInSession = currentCoins;
            newRecord = true;
        }

        if (newRecord)
        {
            SaveData(); // Salva i nuovi record
        }
        return newRecord;
    }
    // --- FINE NUOVI METODI ---

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

    public ShipData GetEquippedShip()
    {
        return allShips.Find(s => s.shipName == equippedShipName);
    }

    public void SetEquippedShip(ShipData ship)
    {
        if (ship != null && unlockedShipNamesSet.Contains(ship.shipName))
        {
            equippedShipName = ship.shipName;
            SaveData();
        }
    }

    public bool IsShipUnlocked(string shipName)
    {
        return unlockedShipNamesSet.Contains(shipName);
    }

    public void UnlockShip(string shipName)
    {
        ShipData shipToUnlock = allShips.Find(s => s.shipName == shipName);
        if (shipToUnlock != null && !IsShipUnlocked(shipName) && specialCurrency >= shipToUnlock.costInGems)
        {
            specialCurrency -= shipToUnlock.costInGems;
            unlockedShipNamesSet.Add(shipName);
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }
}