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

    // --- NUOVE VARIABILI STATICHE PER LE RICOMPENSE IN SOSPESO ---
    public static int PendingCoinsGained { get; set; } = 0;
    public static int PendingGemsGained { get; set; } = 0;
    public static int lastRunXPGained = 0;
    // --- FINE NUOVE VARIABILI ---

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    [Header("Potenziamenti Normali")]
    public List<PermanentUpgradeData> availableUpgrades = new List<PermanentUpgradeData>();

    [Header("Potenziamenti Speciali")]
    public List<SpecialAbility> allSpecialAbilities = new List<SpecialAbility>();
    [Tooltip("Trascina qui tutti gli asset ShipData di tutte le navicelle del gioco.")]
    public List<ShipData> allShips = new List<ShipData>();

    [Header("Configurazione Armi")]
    public List<WeaponData> allWeapons = new List<WeaponData>();

    [Header("Configurazione Missioni")]
    [Tooltip("Trascina qui tutti gli asset MissionData del gioco.")]
    public List<MissionData> allMissions = new List<MissionData>();

    // --- NUOVA SEZIONE PER IL SISTEMA DI MODULI ---
    [Header("Configurazione Moduli")]
    [Tooltip("Trascina qui TUTTI gli asset ModuleData del gioco. Serve per trovarli tramite ID.")]
    public List<ModuleData> allModuleDataAssets = new List<ModuleData>();

    [Header("Configurazione Livello Pilota")]
    [Tooltip("XP base usato nella formula. Controlla la velocità di progressione generale.")]
    public int baseXpForLevel = 500;
    [Tooltip("Esponente della curva di progressione. Valori consigliati: 1.5 - 2.5")]
    public float xpCurveExponent = 2.2f;
    [Tooltip("Lista degli slot per moduli che si sbloccano a determinati livelli pilota.")]
    public List<PilotLevelReward> pilotLevelRewards;

    // Valute e stato
    private int coins;
    private int specialCurrency;
    private AbilityID equippedAbilityID;
    private int maxWaveReached;
    private int maxCoinsInSession;
    private string equippedShipName;
    private string equippedWeaponName;

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

    // Dati Sistema Moduli
    private long totalExperience;
    private int pilotLevel;
    private Dictionary<string, int> moduleInventory = new Dictionary<string, int>();
    private Dictionary<ModuleSlotType, List<string>> equippedModules = new Dictionary<ModuleSlotType, List<string>>();
    private Dictionary<ModuleRarity, List<ModuleData>> modulesByRarity = new Dictionary<ModuleRarity, List<ModuleData>>();

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeModuleDatabase();
            LoadData();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ApplyPendingRewards()
    {
        bool hasChanged = false;
        if (PendingCoinsGained > 0)
        {
            coins += PendingCoinsGained;
            hasChanged = true;
        }
        if (PendingGemsGained > 0)
        {
            specialCurrency += PendingGemsGained;
            hasChanged = true;
        }
        PendingCoinsGained = 0;
        PendingGemsGained = 0;
        if (hasChanged)
        {
            SaveData();
            OnValuesChanged?.Invoke();
            Debug.Log("Applicate ricompense in valuta in sospeso.");
        }
    }

    void LoadData()
    {
        SaveData data = SaveSystem.LoadGame();
        coins = data.coins;
        specialCurrency = data.specialCurrency;
        equippedAbilityID = data.equippedAbilityID;
        maxWaveReached = data.maxWaveReached;
        maxCoinsInSession = data.maxCoinsInSession;
        equippedShipName = data.equippedShipName;
        equippedWeaponName = data.equippedWeaponName;
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
        }

        unlockedAbilitiesSet = new HashSet<AbilityID>(data.unlockedSpecialAbilities);

        ShipData defaultShip = allShips.Find(s => s.isDefaultShip);
        if (defaultShip != null)
        {
            unlockedShipNamesSet.Add(defaultShip.shipName);
            if (string.IsNullOrEmpty(equippedShipName))
            {
                equippedShipName = defaultShip.shipName;
            }
        }

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
        totalExperience = data.totalExperience;
        pilotLevel = data.pilotLevel;

        moduleInventory.Clear();
        for (int i = 0; i < data.moduleInventory_keys.Count; i++)
        {
            moduleInventory[data.moduleInventory_keys[i]] = data.moduleInventory_values[i];
        }

        equippedModules.Clear();
        foreach (ModuleSlotType slotType in System.Enum.GetValues(typeof(ModuleSlotType)))
        {
            equippedModules[slotType] = new List<string>();
        }
        foreach (var savedModule in data.equippedModules)
        {
            while (equippedModules[savedModule.slotType].Count <= savedModule.slotIndex)
            {
                equippedModules[savedModule.slotType].Add(null);
            }
            equippedModules[savedModule.slotType][savedModule.slotIndex] = savedModule.moduleID;
        }
    }

    void SaveData()
    {
        SaveData data = new SaveData();
        data.coins = coins;
        data.specialCurrency = specialCurrency;
        data.equippedAbilityID = equippedAbilityID;
        data.maxWaveReached = maxWaveReached;
        data.maxCoinsInSession = maxCoinsInSession;
        data.equippedShipName = equippedShipName;
        data.equippedWeaponName = equippedWeaponName;
        data.unlockedShipNames = unlockedShipNamesSet.ToList();
        data.savedUpgradeTypes = upgradeLevels.Keys.ToList();
        data.savedUpgradeLevels = upgradeLevels.Values.ToList();
        data.unlockedSpecialAbilities = unlockedAbilitiesSet.ToList();
        data.missionProgressID = missionProgress.Keys.ToList();
        data.missionProgressValue = missionProgress.Values.ToList();
        data.claimedMissionsID = claimedMissions.ToList();
        data.sectorProgressID = sectorProgress.Keys.ToList();
        data.sectorProgressValue = sectorProgress.Values.ToList();
        data.claimedCodexRewardsID = claimedCodexRewards.ToList();
        data.totalExperience = totalExperience;
        data.pilotLevel = pilotLevel;
        data.moduleInventory_keys = moduleInventory.Keys.ToList();
        data.moduleInventory_values = moduleInventory.Values.ToList();
        data.equippedModules = new List<SerializableEquippedModule>();
        foreach (var entry in equippedModules)
        {
            for (int i = 0; i < entry.Value.Count; i++)
            {
                if (!string.IsNullOrEmpty(entry.Value[i]))
                {
                    data.equippedModules.Add(new SerializableEquippedModule
                    {
                        slotType = entry.Key,
                        moduleID = entry.Value[i],
                        slotIndex = i
                    });
                }
            }
        }
        SaveSystem.SaveGame(data);
    }

    #region Metodi di Gioco e Valute
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
    #endregion

    #region Potenziamenti Permanenti
    public int GetUpgradeLevel(PermanentUpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
    }

    public bool CanAfford(PermanentUpgradeType type)
    {
        PermanentUpgradeData u = GetUpgrade(type);
        if (u == null) return false;
        int currentLevel = GetUpgradeLevel(type);
        if (currentLevel >= u.maxLevel) return false;
        int cost = u.GetCostForLevel(currentLevel);
        return coins >= cost;
    }

    public void BuyUpgrade(PermanentUpgradeType type)
    {
        PermanentUpgradeData u = GetUpgrade(type);
        if (u == null) return;
        int currentLevel = GetUpgradeLevel(type);
        if (currentLevel >= u.maxLevel) return;
        int cost = u.GetCostForLevel(currentLevel);
        if (coins >= cost)
        {
            coins -= cost;
            upgradeLevels[type] = currentLevel + 1;
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }

    public PermanentUpgradeData GetUpgrade(PermanentUpgradeType type) => availableUpgrades.Find(u => u.upgradeType == type);

    public float GetTotalBonus(PermanentUpgradeType type)
    {
        PermanentUpgradeData u = GetUpgrade(type);
        int currentLevel = GetUpgradeLevel(type);
        return u != null ? currentLevel * u.bonusPerLevel : 0f;
    }
    #endregion

    #region Abilità Speciali
    public List<SpecialAbility> GetSpecialAbilities(AbilityBehaviorType behaviorType)
    {
        return allSpecialAbilities.Where(ability => ability.behaviorType == behaviorType).ToList();
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
            OnValuesChanged?.Invoke();
        }
    }
    #endregion

    #region Armi
    public WeaponData GetEquippedWeapon()
    {
        if (string.IsNullOrEmpty(equippedWeaponName))
        {
            return allWeapons.FirstOrDefault();
        }
        return allWeapons.Find(w => w.weaponName == equippedWeaponName);
    }

    public void SetEquippedWeapon(WeaponData weapon)
    {
        if (weapon != null)
        {
            equippedWeaponName = weapon.weaponName;
            SaveData();
            OnValuesChanged?.Invoke();
        }
    }
    #endregion

    #region Navicelle
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
    #endregion

    #region Sistema di Reset
    public void ResetProgress()
    {
        SaveSystem.ResetSave();
        coins = 0;
        specialCurrency = 0;
        maxWaveReached = 0;
        maxCoinsInSession = 0;
        equippedAbilityID = AbilityID.None;
        equippedWeaponName = null;
        upgradeLevels.Clear();
        unlockedAbilitiesSet.Clear();
        unlockedShipNamesSet.Clear();
        missionProgress.Clear();
        claimedMissions.Clear();
        sectorProgress.Clear();
        claimedCodexRewards.Clear();
        totalExperience = 0;
        pilotLevel = 1;
        moduleInventory.Clear();
        equippedModules.Clear();
        foreach (ModuleSlotType slotType in System.Enum.GetValues(typeof(ModuleSlotType)))
        {
            equippedModules[slotType] = new List<string>();
        }

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
        }
        SaveData();
        OnValuesChanged?.Invoke();
        Debug.Log("Progresso resettato e nuovo stato iniziale salvato correttamente.");
    }
    #endregion

    #region Metodi Missioni, Settori, Codex
    public void AddEnemyKill(string enemyDataID)
    {
        if (missionProgress.ContainsKey(enemyDataID))
        {
            missionProgress[enemyDataID]++;
        }
        else
        {
            missionProgress[enemyDataID] = 1;
        }
        UpdateMissionProgress(MissionType.KILL_ENEMIES_TOTAL, 1);
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

                if (DebugManager.Instance != null && DebugManager.Instance.showMissionLogs)
                {
                    Debug.Log($"Progresso missione '{mission.title}': {missionProgress[mission.missionID]}/{mission.targetValue}");
                }

                if (missionProgress[mission.missionID] >= mission.targetValue)
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

    public void SetSectorProgress(string sectorID, SectorObjective objectivesAchieved)
    {
        int oldProgress = GetSectorProgressValue(sectorID);
        int newProgress = oldProgress | (int)objectivesAchieved;
        if (newProgress != oldProgress)
        {
            sectorProgress[sectorID] = newProgress;
            SaveData();

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
    #endregion

    #region Module System
    private void InitializeModuleDatabase()
    {
        modulesByRarity.Clear();
        foreach (ModuleRarity rarity in System.Enum.GetValues(typeof(ModuleRarity)))
        {
            modulesByRarity[rarity] = new List<ModuleData>();
        }

        foreach (ModuleData module in allModuleDataAssets)
        {
            if (module != null)
            {
                modulesByRarity[module.rarity].Add(module);
            }
        }
        Debug.Log("Banca dati dei moduli per rarità inizializzata.");
    }

    public List<ModuleData> GetModulesByRarity(ModuleRarity rarity)
    {
        return modulesByRarity.ContainsKey(rarity) ? modulesByRarity[rarity] : new List<ModuleData>();
    }

    public ModuleData GetRandomModuleDrop(List<RarityDropChance> chances)
    {
        if (chances == null || chances.Count == 0) return null;

        float totalWeight = chances.Sum(c => c.weight);
        if (totalWeight <= 0) return null;

        float randomValue = Random.Range(0f, totalWeight);
        ModuleRarity chosenRarity = chances.Last().rarity;

        foreach (var chance in chances)
        {
            if (randomValue <= chance.weight)
            {
                chosenRarity = chance.rarity;
                break;
            }
            randomValue -= chance.weight;
        }

        List<ModuleData> availableModules = GetModulesByRarity(chosenRarity);
        if (availableModules.Count == 0)
        {
            Debug.LogWarning($"Nessun modulo trovato per la rarità estratta: {chosenRarity}. Controlla che 'allModuleDataAssets' sia popolato.");
            return null;
        }
        int randomIndex = Random.Range(0, availableModules.Count);
        return availableModules[randomIndex];
    }

    public int GetPilotLevel() => pilotLevel;
    public long GetTotalExperience() => totalExperience;

    public long GetTotalXPRequiredForLevel(int level)
    {
        if (level <= 1) return 0;

        long totalXp = 0;
        for (int i = 1; i < level; i++)
        {
            totalXp += (long)(baseXpForLevel * Mathf.Pow(i, xpCurveExponent));
        }
        return totalXp;
    }

    public long GetCurrentLevelXP()
    {
        return totalExperience - GetTotalXPRequiredForLevel(pilotLevel);
    }

    public long GetXPForNextLevel(int level)
    {
        return (long)(baseXpForLevel * Mathf.Pow(level, xpCurveExponent));
    }

    public long GetXPForNextLevel() => GetXPForNextLevel(pilotLevel);

    public int GetPilotLevelFromTotalXP(long totalXp)
    {
        int level = 1;
        while (totalXp >= GetTotalXPRequiredForLevel(level + 1))
        {
            level++;
        }
        return level;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        lastRunXPGained = amount;
        Debug.Log($"[ProgressionManager] Memorizzati {amount} XP da applicare.");
    }

    public void ApplyPendingExperience(int xpGained)
    {
        if (xpGained <= 0) return;
        int amount = xpGained;
        totalExperience += amount;

        while (totalExperience >= GetTotalXPRequiredForLevel(pilotLevel + 1))
        {
            pilotLevel++;
            Debug.Log($"Level Up Pilota! Nuovo livello: {pilotLevel}");
        }

        SaveData();
        Debug.Log($"[ProgressionManager] Applicati e salvati {amount} XP. Nuovo totale: {totalExperience}");
    }

    public PilotLevelReward GetRewardForLevel(int level) => pilotLevelRewards.Find(r => r.level == level);

    public void ApplyPilotLevelReward(PilotLevelReward reward)
    {
        Debug.Log($"Applicando ricompensa per il livello {reward.level}: {reward.rewardType}");
        switch (reward.rewardType)
        {
            case PilotRewardType.Coins:
                AddCoins(reward.amount);
                OnValuesChanged?.Invoke();
                break;
            case PilotRewardType.Gems:
                AddSpecialCurrency(reward.amount);
                OnValuesChanged?.Invoke();
                break;
            case PilotRewardType.ModuleSlot:
                break;
        }
    }

    public Dictionary<ModuleSlotType, int> GetUnlockedSlotsCount()
    {
        var unlockedCount = new Dictionary<ModuleSlotType, int>
        {
            { ModuleSlotType.Offensive, 1 },
            { ModuleSlotType.Defensive, 1 },
            { ModuleSlotType.Utility, 1 }
        };

        foreach (var reward in pilotLevelRewards)
        {
            if (reward.rewardType == PilotRewardType.ModuleSlot && pilotLevel >= reward.level)
            {
                unlockedCount[reward.moduleSlotType]++;
            }
        }
        return unlockedCount;
    }

    public ModuleData GetModuleDataByID(string id)
    {
        return allModuleDataAssets.Find(m => m.moduleID == id);
    }

    public int GetModuleCount(string moduleID)
    {
        return moduleInventory.ContainsKey(moduleID) ? moduleInventory[moduleID] : 0;
    }
    public Dictionary<string, int> GetModuleInventory()
    {
        return moduleInventory;
    }
    public void AddModule(string moduleID, int quantity = 1)
    {
        if (moduleInventory.ContainsKey(moduleID))
        {
            moduleInventory[moduleID] += quantity;
        }
        else
        {
            moduleInventory[moduleID] = quantity;
        }
        Debug.Log($"Aggiunto modulo {moduleID} x{quantity}. Totale: {moduleInventory[moduleID]}");
        SaveData();
        OnValuesChanged?.Invoke();
    }

    public bool FuseModules(string moduleID)
    {
        if (GetModuleCount(moduleID) < 3) return false;

        ModuleData sourceModule = GetModuleDataByID(moduleID);
        if (sourceModule == null || sourceModule.fusionResult == null)
        {
            Debug.LogWarning($"Impossibile fondere {moduleID}: dati del modulo o del risultato della fusione non trovati.");
            return false;
        }

        moduleInventory[moduleID] -= 3;
        if (moduleInventory[moduleID] <= 0)
        {
            moduleInventory.Remove(moduleID);
        }

        AddModule(sourceModule.fusionResult.moduleID, 1);
        Debug.Log($"Fusione riuscita! 3x {sourceModule.moduleName} -> 1x {sourceModule.fusionResult.moduleName}");
        SaveData();
        OnValuesChanged?.Invoke();
        return true;
    }

    public List<string> GetEquippedModules(ModuleSlotType slotType)
    {
        return equippedModules.ContainsKey(slotType) ? equippedModules[slotType] : new List<string>();
    }

    public void EquipModule(string moduleID, int slotIndex)
    {
        ModuleData moduleToEquip = GetModuleDataByID(moduleID);
        if (moduleToEquip == null) return;
        if (GetModuleCount(moduleID) <= 0) return;

        var unlockedSlots = GetUnlockedSlotsCount();
        if (slotIndex >= unlockedSlots[moduleToEquip.slotType])
        {
            Debug.LogError($"Tentativo di equipaggiare in uno slot non sbloccato! Tipo: {moduleToEquip.slotType}, Indice: {slotIndex}");
            return;
        }

        while (equippedModules[moduleToEquip.slotType].Count <= slotIndex)
        {
            equippedModules[moduleToEquip.slotType].Add(null);
        }

        string currentlyEquippedID = equippedModules[moduleToEquip.slotType][slotIndex];
        if (!string.IsNullOrEmpty(currentlyEquippedID))
        {
            AddModule(currentlyEquippedID, 1);
        }

        equippedModules[moduleToEquip.slotType][slotIndex] = moduleID;
        moduleInventory[moduleID]--;
        if (moduleInventory[moduleID] <= 0)
        {
            moduleInventory.Remove(moduleID);
        }

        SaveData();
        OnValuesChanged?.Invoke();
    }

    public void UnequipModule(ModuleSlotType slotType, int slotIndex)
    {
        if (!equippedModules.ContainsKey(slotType) || slotIndex >= equippedModules[slotType].Count) return;

        string moduleID = equippedModules[slotType][slotIndex];
        if (string.IsNullOrEmpty(moduleID)) return;

        equippedModules[slotType][slotIndex] = null;
        AddModule(moduleID, 1);
    }
    #endregion
}
