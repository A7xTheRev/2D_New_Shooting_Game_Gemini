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
            if (isQuitting) return null;
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ProgressionManager>();
            }
            return _instance;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    [Header("Potenziamenti Normali")] 
    public List<PermanentUpgrade> availableUpgrades = new List<PermanentUpgrade>();
    [Header("Potenziamenti Speciali")] 
    public List<SpecialAbility> allSpecialAbilities = new List<SpecialAbility>();
    [Tooltip("Trascina qui tutti gli asset ShipData di tutte le navicelle del gioco.")]
    public List<ShipData> allShips = new List<ShipData>();

    [Header("Configurazione Missioni")]
    [Tooltip("Trascina qui tutti gli asset MissionData del gioco.")]
    public List<MissionData> allMissions = new List<MissionData>();

    // Valute e stato
    private int coins; 
    private int specialCurrency; 
    private AbilityID equippedAbilityID;
    private int maxWaveReached;
    private int maxCoinsInSession;
    private string equippedShipName;
    
    // Dati di progressione
    private Dictionary<PermanentUpgradeType, int> upgradeLevels = new Dictionary<PermanentUpgradeType, int>();
    private HashSet<AbilityID> unlockedAbilitiesSet = new HashSet<AbilityID>();
    private HashSet<string> unlockedShipNamesSet = new HashSet<string>();

    // Dati progressione missioni
    private Dictionary<string, int> missionProgress = new Dictionary<string, int>();
    private HashSet<string> claimedMissions = new HashSet<string>();

    // Dati progressione settori
    private Dictionary<string, int> sectorProgress = new Dictionary<string, int>();
    private HashSet<string> claimedCodexRewards = new HashSet<string>();

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (_instance == null) 
        { 
            _instance = this; 
            DontDestroyOnLoad(gameObject);
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
        for (int i = 0; i < data.savedUpgradeTypes.Count; i++)
        {
            upgradeLevels[data.savedUpgradeTypes[i]] = data.savedUpgradeLevels[i];
        }
        foreach (var upgrade in availableUpgrades)
        {
            if (!upgradeLevels.ContainsKey(upgrade.upgradeType))
            {
                upgradeLevels[upgrade.upgradeType] = 0;
            }
            upgrade.currentLevel = upgradeLevels[upgrade.upgradeType];
        }
        
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
        
        // Caricamento dati missioni
        missionProgress.Clear();
        for (int i = 0; i < data.missionProgressID.Count; i++)
        {
            missionProgress[data.missionProgressID[i]] = data.missionProgressValue[i];
        }
        claimedMissions = new HashSet<string>(data.claimedMissionsID);

        sectorProgress.Clear();
        for (int i = 0; i < data.sectorProgressID.Count; i++)
        {
            sectorProgress[data.sectorProgressID[i]] = data.sectorProgressValue[i];
        }

        claimedCodexRewards = new HashSet<string>(data.claimedCodexRewardsID);
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
        data.savedUpgradeTypes = upgradeLevels.Keys.ToList();
        data.savedUpgradeLevels = upgradeLevels.Values.ToList();
        data.unlockedSpecialAbilities = unlockedAbilitiesSet.ToList();
        
        // Salvataggio dati missioni
        data.missionProgressID = missionProgress.Keys.ToList();
        data.missionProgressValue = missionProgress.Values.ToList();
        data.claimedMissionsID = claimedMissions.ToList();
        data.sectorProgressID = sectorProgress.Keys.ToList();
        data.sectorProgressValue = sectorProgress.Values.ToList();
        data.claimedCodexRewardsID = claimedCodexRewards.ToList();

        SaveSystem.SaveGame(data);
    }

    public int GetMaxWave() => maxWaveReached;
    public int GetMaxCoins() => maxCoinsInSession;

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
            SaveData();
        }
        return newRecord;
    }
    // --- FINE NUOVI METODI ---

    public int GetCoins() => coins;
    public int GetSpecialCurrency() => specialCurrency;

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

    public bool CanAfford(PermanentUpgrade upgrade) => coins >= upgrade.GetNextLevelCost();

    public void BuyUpgrade(PermanentUpgradeType type)
    {
        PermanentUpgrade u = GetUpgrade(type);
        if (u == null || u.currentLevel >= u.maxLevel) return;
        int c = u.GetNextLevelCost();
        if (coins >= c)
        {
            coins -= c;
            u.currentLevel++;
            upgradeLevels[type] = u.currentLevel;
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }

    public PermanentUpgrade GetUpgrade(PermanentUpgradeType type) => availableUpgrades.Find(u => u.upgradeType == type);
    public float GetTotalBonus(PermanentUpgradeType type)
    {
        PermanentUpgrade u = GetUpgrade(type);
        return u != null ? u.currentLevel * u.bonusPerLevel : 0f;
    }

    public bool CanAfford(SpecialAbility ability) => specialCurrency >= ability.cost;

    public void BuySpecialUpgrade(AbilityID id)
    {
        SpecialAbility a = allSpecialAbilities.Find(ab => ab.abilityID == id);
        if (a == null || IsSpecialUpgradeUnlocked(id)) return;
        if (specialCurrency >= a.cost)
        {
            specialCurrency -= a.cost;
            unlockedAbilitiesSet.Add(id);
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }

    public bool IsSpecialUpgradeUnlocked(AbilityID id)
    {
        SpecialAbility a = allSpecialAbilities.Find(ab => ab.abilityID == id);
        if (a != null && a.isDefaultAbility) return true;
        return unlockedAbilitiesSet.Contains(id);
    }

    public SpecialAbility GetEquippedAbility()
    {
        if (equippedAbilityID == AbilityID.None)
        {
            return allSpecialAbilities.Find(a => a.isDefaultAbility);
        }
        return allSpecialAbilities.Find(a => a.abilityID == equippedAbilityID);
    }

    public void SetEquippedAbility(SpecialAbility ability)
    {
        if (ability != null)
        {
            equippedAbilityID = ability.abilityID;
            SaveData();
        }
    }

    public ShipData GetEquippedShip() => allShips.Find(s => s.shipName == equippedShipName);

    public void SetEquippedShip(ShipData ship)
    {
        if (ship != null && unlockedShipNamesSet.Contains(ship.shipName))
        {
            equippedShipName = ship.shipName;
            SaveData();
        }
    }

    public bool IsShipUnlocked(string shipName) => unlockedShipNamesSet.Contains(shipName);

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

    // --- METODO RESETPROGRESS CORRETTO E RESO PIÙ ROBUSTO ---
    public void ResetProgress() 
    { 
        // 1. Cancella il vecchio file di salvataggio fisico
        SaveSystem.ResetSave(); 

        // 2. Resetta tutte le variabili in memoria allo stato iniziale
        coins = 0; 
        specialCurrency = 0; 
        maxWaveReached = 0;
        maxCoinsInSession = 0;
        equippedAbilityID = AbilityID.None; 
        upgradeLevels.Clear(); 
        unlockedAbilitiesSet.Clear(); 
        unlockedShipNamesSet.Clear(); 
        missionProgress.Clear(); 
        claimedMissions.Clear(); 
        sectorProgress.Clear();
        claimedCodexRewards.Clear();
        
        // 3. Imposta esplicitamente lo stato della navicella di default
        ShipData defaultShip = allShips.Find(s => s.isDefaultShip);
        if (defaultShip != null)
        {
            unlockedShipNamesSet.Add(defaultShip.shipName);
            equippedShipName = defaultShip.shipName;
        }
        else
        {
            equippedShipName = null;
        }

        foreach (var upgrade in availableUpgrades)
        {
            upgrade.currentLevel = 0;
        }
        SaveData(); 

        // 6. Notifica la UI per aggiornare la visualizzazione
        OnValuesChanged?.Invoke(); 
        Debug.Log("Progresso resettato e nuovo stato iniziale salvato correttamente.");
    }

    public void AddEnemyKill(string enemyDataID) // Il parametro ora è l'ID dell'EnemyData
    {
        // 1. Aggiorna il conteggio generico per il Codex e le missioni
        if (missionProgress.ContainsKey(enemyDataID))
        {
            missionProgress[enemyDataID]++;
        }
        else
        {
            missionProgress[enemyDataID] = 1;
        }
        
        // 2. Aggiorna la missione di uccisioni totali
        UpdateMissionProgress(MissionType.KILL_ENEMIES_TOTAL, 1);
        
        // 3. Controlla se questa uccisione è rilevante per una missione specifica "Uccidi nemico di tipo X"
        // NOTA: Questo richiede che il campo 'targetEnemyID' nelle MissionData corrisponda al nome dell'EnemyData (es. "ED_Kamikaze")
        UpdateMissionProgress(MissionType.KILL_ENEMIES_OF_TYPE, 1, enemyDataID);
    }

    public void AddCoinsCollected(int amount)
    {
        UpdateMissionProgress(MissionType.COLLECT_COINS_TOTAL, amount);
    }

    public void NotifySectorCompleted(string completedSectorName)
    {
        UpdateMissionProgress(MissionType.COMPLETE_SECTOR, 1, completedSectorName);
    }

    public void ReportEndlessSurvivalTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        if (minutes > 0)
        {
            UpdateMissionProgress(MissionType.SURVIVE_MINUTES_ENDLESS, minutes);
        }
    }

    private void UpdateMissionProgress(MissionType type, int amount, string targetID = "")
    {
        foreach (MissionData mission in allMissions)
        {
            if (mission.missionType == type && !claimedMissions.Contains(mission.missionID) && GetMissionProgress(mission.missionID) < mission.targetValue)
            {
                if ((type == MissionType.KILL_ENEMIES_OF_TYPE || type == MissionType.COMPLETE_SECTOR) && mission.targetEnemyID != targetID)
                {
                    continue;
                }

                int currentProgress = GetMissionProgress(mission.missionID);
                int newProgress = (type == MissionType.SURVIVE_MINUTES_ENDLESS) ? Mathf.Max(currentProgress, amount) : currentProgress + amount;
                
                missionProgress[mission.missionID] = Mathf.Min(newProgress, mission.targetValue);

                // --- MODIFICA APPLICATA QUI ---
                // Prima di stampare, controlla l'interruttore sul DebugManager.
                if (DebugManager.Instance != null && DebugManager.Instance.showMissionLogs)
                {
                Debug.Log($"Progresso missione '{mission.title}': {missionProgress[mission.missionID]}/{mission.targetValue}");
                }
                
                if(missionProgress[mission.missionID] >= mission.targetValue)
                {
                    if (DebugManager.Instance != null && DebugManager.Instance.showMissionLogs)
                {
                    Debug.Log($"MISSIONE COMPLETATA: '{mission.title}'!");
                    }
                    OnValuesChanged?.Invoke();
                }
            }
        }
    }
    
    public int GetMissionProgress(string missionID) => missionProgress.ContainsKey(missionID) ? missionProgress[missionID] : 0;
    public bool IsMissionComplete(string missionID)
    {
        MissionData mission = allMissions.Find(m => m.missionID == missionID);
        if (mission == null) return false;
        return GetMissionProgress(missionID) >= mission.targetValue;
    }
    public bool IsMissionClaimed(string missionID) => claimedMissions.Contains(missionID);

    public void ClaimMissionReward(string missionID)
    {
        if (IsMissionComplete(missionID) && !IsMissionClaimed(missionID))
        {
            MissionData mission = allMissions.Find(m => m.missionID == missionID);
            if (mission != null)
            {
                AddSpecialCurrency(mission.gemReward);
                claimedMissions.Add(missionID);
                SaveData();
            OnValuesChanged?.Invoke();
                Debug.Log($"Ricompensa per la missione '{mission.title}' riscattata!");
            }
        }
    }

    // --- METODI PROGRESSIONE SETTORI ---
    public void SetSectorProgress(string sectorID, SectorObjective objectivesAchieved)
    {
        int oldProgress = GetSectorProgressValue(sectorID);
        int newProgress = oldProgress | (int)objectivesAchieved;

        if (newProgress != oldProgress)
        {
            sectorProgress[sectorID] = newProgress;
            SaveData();

            // --- MODIFICA APPLICATA QUI ---
            if (DebugManager.Instance != null && DebugManager.Instance.showSectorProgressLogs)
            {
                Debug.Log($"Progresso per il settore '{sectorID}' aggiornato a: {newProgress}");
            }
        }
    }
    private int GetSectorProgressValue(string sectorID) => sectorProgress.ContainsKey(sectorID) ? sectorProgress[sectorID] : 0;
    public bool IsObjectiveComplete(string sectorID, SectorObjective objective)
    {
        int progress = GetSectorProgressValue(sectorID);
        return (progress & (int)objective) == (int)objective;
    }
    public int GetStarCount(string sectorID)
    {
        if (!sectorProgress.ContainsKey(sectorID)) return 0;
        
        int progress = GetSectorProgressValue(sectorID);
        int starCount = 0;
        if ((progress & (int)SectorObjective.SECTOR_COMPLETED) != 0) starCount++;
        if ((progress & (int)SectorObjective.HEALTH_OVER_70_PERCENT) != 0) starCount++;
        if ((progress & (int)SectorObjective.NO_DAMAGE_TAKEN) != 0) starCount++;
        return starCount;
    }

    // --- METODI DEL CODEX AGGIORNATI ---
    public void ClaimCodexReward(EnemyData enemyData)
    {
        if (enemyData == null || IsCodexRewardClaimed(enemyData.name)) return;
        
        int killCount = GetMissionProgress(enemyData.name);
        if (killCount >= enemyData.codexKillRequirement)
        {
            AddCoins(enemyData.codexCoinReward);
            AddSpecialCurrency(enemyData.codexGemReward);
            claimedCodexRewards.Add(enemyData.name);
            SaveData();
            OnValuesChanged?.Invoke();
            Debug.Log($"Ricompensa del Codex per '{enemyData.name}' riscattata!");
        }
    }
    public bool IsCodexRewardClaimed(string enemyID)
    {
        return claimedCodexRewards.Contains(enemyID);
    }
}