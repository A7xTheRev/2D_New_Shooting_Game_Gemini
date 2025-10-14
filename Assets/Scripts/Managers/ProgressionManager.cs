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
    // --- NUOVA VARIABILE STATICA PER L'ANIMAZIONE DELLA UI ---
    public static int lastRunXPGained = 0;
    // --- FINE RIPRISTINO ---

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
    
    // Dati Sistema Moduli
    private long totalExperience;
    private int pilotLevel;
    private Dictionary<string, int> moduleInventory = new Dictionary<string, int>();
    // La chiave è il tipo di slot, la lista contiene gli ID dei moduli equipaggiati in ordine
    private Dictionary<ModuleSlotType, List<string>> equippedModules = new Dictionary<ModuleSlotType, List<string>>();
    
    // --- NUOVO: Dizionario per accedere velocemente ai moduli per rarità ---
    private Dictionary<ModuleRarity, List<ModuleData>> modulesByRarity = new Dictionary<ModuleRarity, List<ModuleData>>();
    // --- FINE NUOVO ---

    public static event System.Action OnValuesChanged;

    void Awake()
    {
        if (_instance == null) 
        { 
            _instance = this; 
            DontDestroyOnLoad(gameObject);
            InitializeModuleDatabase(); // --- NUOVO: Pre-organizza i moduli all'avvio ---
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

        // --- NUOVA LOGICA DI CARICAMENTO PER I MODULI ---
        totalExperience = data.totalExperience;
        pilotLevel = data.pilotLevel;

        moduleInventory.Clear();
        for (int i = 0; i < data.moduleInventory_keys.Count; i++)
        {
            moduleInventory[data.moduleInventory_keys[i]] = data.moduleInventory_values[i];
        }

        equippedModules.Clear();
        // Inizializza la dictionary per tutti i tipi di slot
        foreach (ModuleSlotType slotType in System.Enum.GetValues(typeof(ModuleSlotType)))
        {
            equippedModules[slotType] = new List<string>();
        }
        // Popola la dictionary con i dati salvati
        foreach (var savedModule in data.equippedModules)
        {
            // Assicura che la lista sia abbastanza grande da contenere il modulo
            while (equippedModules[savedModule.slotType].Count <= savedModule.slotIndex)
            {
                equippedModules[savedModule.slotType].Add(null); // Aggiunge slot vuoti se necessario
            }
            equippedModules[savedModule.slotType][savedModule.slotIndex] = savedModule.moduleID;
        }
        // --- FINE NUOVA LOGICA ---
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

        // --- NUOVA LOGICA DI SALVATAGGIO PER I MODULI ---
        data.totalExperience = totalExperience;
        data.pilotLevel = pilotLevel;
        
        data.moduleInventory_keys = moduleInventory.Keys.ToList();
        data.moduleInventory_values = moduleInventory.Values.ToList();
        
        data.equippedModules = new List<SerializableEquippedModule>();
        foreach (var entry in equippedModules)
        {
            for (int i = 0; i < entry.Value.Count; i++)
            {
                if (!string.IsNullOrEmpty(entry.Value[i])) // Salva solo gli slot non vuoti
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
        // --- FINE NUOVA LOGICA ---

        SaveSystem.SaveGame(data);
    }
    
    #region Metodi Esistenti

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
        
        // --- NUOVA LOGICA DI RESET PER I MODULI ---
        totalExperience = 0;
        pilotLevel = 1;
        moduleInventory.Clear();
        equippedModules.Clear();
        foreach (ModuleSlotType slotType in System.Enum.GetValues(typeof(ModuleSlotType)))
        {
            equippedModules[slotType] = new List<string>();
        }
        // --- FINE NUOVA LOGICA ---
        
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

    // ... tutti gli altri metodi per missioni, settori e codex ...
    #region Metodi Missioni, Settori, Codex
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
    
    #endregion
    
    #endregion
    
    #region Module System

    // --- NUOVI METODI PER LA LOGICA DI LOOT BASATA SULLA RARITA' ---

    /// <summary>
    /// Pre-organizza tutti i moduli in un dizionario per efficienza, chiamato una sola volta all'avvio.
    /// </summary>
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

    /// <summary>
    /// Restituisce una lista di tutti i ModuleData di una specifica rarità.
    /// </summary>
    public List<ModuleData> GetModulesByRarity(ModuleRarity rarity)
    {
        return modulesByRarity.ContainsKey(rarity) ? modulesByRarity[rarity] : new List<ModuleData>();
    }

    /// <summary>
    /// Esegue la logica di drop a 2 fasi: prima sceglie una rarità, poi un modulo a caso di quella rarità.
    /// </summary>
    public ModuleData GetRandomModuleDrop(List<RarityDropChance> chances)
    {
        if (chances == null || chances.Count == 0) return null;

        // FASE 1: Scegli la Rarità in base ai pesi
        float totalWeight = chances.Sum(c => c.weight);
        if (totalWeight <= 0) return null;

        float randomValue = Random.Range(0f, totalWeight);
        ModuleRarity chosenRarity = chances.Last().rarity; // Fallback in caso di errori

        foreach (var chance in chances)
        {
            if (randomValue <= chance.weight)
            {
                chosenRarity = chance.rarity;
                break;
            }
            randomValue -= chance.weight;
        }

        // FASE 2: Scegli un modulo a caso di quella rarità
        List<ModuleData> availableModules = GetModulesByRarity(chosenRarity);
        if (availableModules.Count == 0)
        {
            Debug.LogWarning($"Nessun modulo trovato per la rarità estratta: {chosenRarity}. Controlla che 'allModuleDataAssets' sia popolato.");
            return null;
        }

        int randomIndex = Random.Range(0, availableModules.Count);
        return availableModules[randomIndex];
    }
    // --- FINE NUOVI METODI ---


    // --- METODI ESISTENTI DEL MODULE SYSTEM ---
    public int GetPilotLevel() => pilotLevel;
    public long GetTotalExperience() => totalExperience;

    // --- FORMULA XP AGGIORNATA ---
    // Calcola l'XP totale necessario per raggiungere un dato livello
    public long GetTotalXPRequiredForLevel(int level)
    {
        if (level <= 1) return 0;

        long totalXp = 0;
        for (int i = 1; i < level; i++)
        {
            // La nuova formula calcola il costo di ogni singolo livello e lo somma
            totalXp += (long)(baseXpForLevel * Mathf.Pow(i, xpCurveExponent));
        }
        return totalXp;
    }

    public long GetCurrentLevelXP()
    {
        return totalExperience - GetTotalXPRequiredForLevel(pilotLevel);
    }

    // Overload per ottenere l'XP per un livello specifico (usato dalla UI)
    public long GetXPForNextLevel(int level)
    {
        // Costo per il prossimo livello = base * (livello_attuale ^ esponente)
        return (long)(baseXpForLevel * Mathf.Pow(level, xpCurveExponent));
    }

    // Metodo originale senza argomenti
    public long GetXPForNextLevel() => GetXPForNextLevel(pilotLevel);

    /// <summary>
    /// Calcola a quale livello corrisponde un dato ammontare di esperienza totale.
    /// </summary>
    public int GetPilotLevelFromTotalXP(long totalXp)
    {
        int level = 1;
        while (totalXp >= GetTotalXPRequiredForLevel(level + 1))
        {
            level++;
        }
        return level;
    }
    
    /// <summary>
    /// Aggiunge esperienza al totale del giocatore e gestisce i level up.
    /// Da chiamare alla fine di una partita.
    /// MODIFICATA: Ora memorizza solo l'XP. L'applicazione vera e propria
    /// viene gestita da ApplyPendingExperience().
    /// </summary>
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        lastRunXPGained = amount; // Memorizza l'XP per l'animazione della UI
        Debug.Log($"[ProgressionManager] Memorizzati {amount} XP da applicare.");
    }

    public void ApplyPendingExperience(int xpGained)
    {
        if (xpGained <= 0) return;
        int amount = xpGained;
        totalExperience += amount;

        // Controlla e aggiorna il livello del pilota in base alla nuova esperienza totale.
        // Il ciclo while gestisce i level up multipli.
        while (totalExperience >= GetTotalXPRequiredForLevel(pilotLevel + 1))
        {
            pilotLevel++;
            Debug.Log($"Level Up Pilota! Nuovo livello: {pilotLevel}");
        }

        // Le ricompense vengono ora applicate istantaneamente dal popup PilotLevelRewardPopup.
        // Questa funzione ora si occupa solo di aggiornare l'esperienza e il livello del pilota.

        // Non è necessario chiamare OnValuesChanged qui, perché viene già chiamato
        // da ApplyPilotLevelReward al momento giusto.
        SaveData();
        Debug.Log($"[ProgressionManager] Applicati e salvati {amount} XP. Nuovo totale: {totalExperience}");
    }
    
    /// <summary>
    /// Trova la ricompensa associata a un livello specifico.
    /// </summary>
    public PilotLevelReward GetRewardForLevel(int level) => pilotLevelRewards.Find(r => r.level == level);
    // --- FINE NUOVI METODI ---
    public void ApplyPilotLevelReward(PilotLevelReward reward)
    {
        Debug.Log($"Applicando ricompensa per il livello {reward.level}: {reward.rewardType}");
        switch (reward.rewardType)
        {
            case PilotRewardType.Coins:
                AddCoins(reward.amount);
                OnValuesChanged?.Invoke(); // Notifica la UI dell'aggiunta di monete
                // TODO: Mostrare una notifica UI per la ricompensa
                break;
            case PilotRewardType.Gems:
                AddSpecialCurrency(reward.amount);
                OnValuesChanged?.Invoke(); // Notifica la UI dell'aggiunta di gemme
                // TODO: Mostrare una notifica UI per la ricompensa
                break;
            case PilotRewardType.ModuleSlot:
                // L'esistenza della ricompensa è sufficiente, GetUnlockedSlotsCount() farà il resto.
                // TODO: Mostrare una notifica UI per lo sblocco dello slot
                break;
        }
    }
    
    // --- METODI PER GLI SLOT ---
    
    /// <summary>
    /// Calcola il numero di slot sbloccati per ogni tipo in base al livello pilota.
    /// </summary>
    public Dictionary<ModuleSlotType, int> GetUnlockedSlotsCount()
    {
        var unlockedCount = new Dictionary<ModuleSlotType, int>
        {
            { ModuleSlotType.Offensive, 1 }, // Inizia con 1 slot di ogni tipo
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

    // --- METODI PER L'INVENTARIO DEI MODULI ---
    
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
    
    /// <summary>
    /// Tenta di fondere 3 moduli per ottenerne uno di rarità superiore.
    /// </summary>
    /// <returns>True se la fusione è riuscita, altrimenti False.</returns>
    public bool FuseModules(string moduleID)
    {
        if (GetModuleCount(moduleID) < 3) return false;

        ModuleData sourceModule = GetModuleDataByID(moduleID);
        if (sourceModule == null || sourceModule.fusionResult == null)
        {
            Debug.LogWarning($"Impossibile fondere {moduleID}: dati del modulo o del risultato della fusione non trovati.");
            return false;
        }

        // Rimuovi 3 moduli sorgente
        moduleInventory[moduleID] -= 3;
        if (moduleInventory[moduleID] <= 0)
        {
            moduleInventory.Remove(moduleID);
        }

        // Aggiungi 1 modulo risultato
        AddModule(sourceModule.fusionResult.moduleID, 1);
        
        Debug.Log($"Fusione riuscita! 3x {sourceModule.moduleName} -> 1x {sourceModule.fusionResult.moduleName}");
        SaveData();
        OnValuesChanged?.Invoke();
        return true;
    }
    
    // --- METODI PER EQUIPAGGIARE I MODULI ---
    
    public List<string> GetEquippedModules(ModuleSlotType slotType)
    {
        return equippedModules.ContainsKey(slotType) ? equippedModules[slotType] : new List<string>();
    }
    
    /// <summary>
    /// Equipaggia un modulo in uno slot specifico.
    /// </summary>
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

        // Assicura che la lista sia abbastanza grande
        while (equippedModules[moduleToEquip.slotType].Count <= slotIndex)
        {
            equippedModules[moduleToEquip.slotType].Add(null);
        }

        // Se c'è già un modulo in quello slot, lo restituisce all'inventario
        string currentlyEquippedID = equippedModules[moduleToEquip.slotType][slotIndex];
        if (!string.IsNullOrEmpty(currentlyEquippedID))
        {
            AddModule(currentlyEquippedID, 1);
        }
        
        // Equipaggia il nuovo modulo
        equippedModules[moduleToEquip.slotType][slotIndex] = moduleID;
        // Rimuove 1 istanza dall'inventario
        moduleInventory[moduleID]--;
        if (moduleInventory[moduleID] <= 0)
        {
            moduleInventory.Remove(moduleID);
        }
        
        SaveData();
        OnValuesChanged?.Invoke();
    }
    
    /// <summary>
    /// Rimuove un modulo da uno slot e lo restituisce all'inventario.
    /// </summary>
    public void UnequipModule(ModuleSlotType slotType, int slotIndex)
    {
        if (!equippedModules.ContainsKey(slotType) || slotIndex >= equippedModules[slotType].Count) return;
        
        string moduleID = equippedModules[slotType][slotIndex];
        if (string.IsNullOrEmpty(moduleID)) return;

        // Rimuovi dallo slot
        equippedModules[slotType][slotIndex] = null;
        // Aggiungi all'inventario
        AddModule(moduleID, 1);
    }
    #endregion
}