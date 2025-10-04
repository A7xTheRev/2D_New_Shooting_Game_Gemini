using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// Enum per scegliere la modalità Endless dall'Inspector
public enum EndlessModeType { Waves, Continuous }

// Aggiunto un nuovo enum per gestire gli stati della modalità continua
public enum EndlessContinuousState { Spawning, BossTransition, BossFight }

[System.Serializable]
public class TimedEnemyGroup
{
    [Tooltip("Solo per riferimento nell'editor.")]
    public string groupName;
    [Tooltip("A quale minuto di gioco verranno introdotti questi nemici.")]
    public float timeToIntroduceInMinutes;
    public List<GameObject> enemyPrefabs;
}

public class StageManager : MonoBehaviour
{
    [Header("Configurazione Modalità Storia")]
    public float storyGrowthRate = 0.05f;

    [Header("Configurazione Endless Generale")]
    [Tooltip("Scegli quale logica usare per la modalità Endless.")]
    public EndlessModeType endlessModeType = EndlessModeType.Continuous;
    [Tooltip("Il tasso di crescita per la modalità Endless (usato da entrambe le logiche).")]
    public float endlessGrowthRate = 0.15f;
    public List<GameObject> endlessBossPrefabs;
    [Tooltip("Usato da entrambe le modalità Endless per il super boss.")]
    public GameObject endlessSuperBossPrefab;

    // --- LISTA UNIFICATA PER TUTTI I NEMICI ENDLESS ---
    [Tooltip("La lista completa di tutti i nemici che possono apparire in QUALSIASI modalità Endless.")]
    public List<GameObject> endlessFullEnemyPool;

    [Header("Endless: Modalità a Ondate (Legacy)")]
    public int endlessSuperBossStage = 20;

    [Header("Endless: Modalità Continua")]
    [Tooltip("Intervallo di spawn all'inizio della partita (secondi).")]
    public float initialSpawnInterval = 1.5f;
    [Tooltip("Intervallo di spawn minimo che si raggiunge a fine partita (secondi).")]
    public float minSpawnInterval = 0.1f;
    [Tooltip("Dopo quanti minuti si raggiunge l'intervallo di spawn minimo.")]
    public float timeToReachMinIntervalInMinutes = 10f;
    [Tooltip("Limite di nemici su schermo all'inizio.")]
    public int initialEnemyCap = 20;
    [Tooltip("Limite massimo di nemici su schermo.")]
    public int maxEnemyCap = 150;
    [Tooltip("Dopo quanti minuti si raggiunge il limite massimo di nemici.")]
    public float timeToReachMaxCapInMinutes = 15f;
    [Tooltip("Quanti tipi di nemici casuali sono disponibili all'inizio.")]
    public int startingEnemyTypeCount = 3;
    [Tooltip("Ogni quanti secondi un nuovo tipo di nemico viene aggiunto al pool di spawn.")]
    public float newEnemyIntroductionInterval = 45f;
    [Tooltip("Ogni quanti minuti appare un boss nella modalità continua.")]
    public float bossIntervalInMinutes = 4f;

    [Header("Configurazione Elite Dinamici")]
    public List<GameObject> eliteModifierPrefabs;
    [Range(0f, 1f)] public float eliteSpawnChance = 0.1f;
    [Header("Configurazione Statistiche Elite")]
    public float eliteHealthMultiplier = 4f;
    public float eliteDamageMultiplier = 2f;
    public float eliteSpeedMultiplier = 1.2f;
    public float eliteAttackSpeedMultiplier = 1.5f;
    public float eliteCoinMultiplier = 5f;
    public float eliteXpMultiplier = 4f;
    public Color eliteColorTint = new Color(0.88f, 0.52f, 1f);

    [Header("Gestione Stage (Legacy per Modalità Waves)")]
    public int enemiesPerStage = 8;
    public int stageToStartElites = 3;
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    // Variabili private
    private GameMode currentMode;
    private SectorData currentSector;
    private bool gameHasStarted = false;
    private float survivalTimer = 0f;
    private int globalDifficultyLevel = 0;
    
    // Confini di spawn dinamici
    private float spawnXMin, spawnXMax, spawnY;

    // Modalità Storia & Endless a Ondate
    [HideInInspector] public int stageNumber = 1;
    private bool isSpawningWave;
    private bool isBossWave;
    
    // Modalità Endless Continua
    private EndlessContinuousState endlessState;
    private float bossTimer;
    private float currentSpawnInterval;
    private int currentEnemyCap;
    private float spawnTimer;
    private List<GameObject> availableEnemyPool = new List<GameObject>();
    private List<GameObject> activeSpawnPool = new List<GameObject>();
    private float newEnemyTimer;

    
    void Awake()
    {
        // --- CALCOLO PUNTI DI SPAWN DINAMICI ---
        Camera cam = Camera.main;
        // Calcola la posizione Y appena sopra il bordo superiore dello schermo
        spawnY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1, 0)).y + 1f;
        // Calcola le posizioni X ai bordi dello schermo
        spawnXMin = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x;
        spawnXMax = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;
    }

    void Start()
    {
        if (GameDataManager.Instance != null)
        {
            currentMode = GameDataManager.Instance.selectedGameMode;
            currentSector = GameDataManager.Instance.selectedSector;
            if (currentMode == GameMode.Story)
            {
                if (MenuManager.Instance != null && GameDataManager.Instance.selectedWorld != null)
                {
                    int worldIndex = MenuManager.Instance.allWorlds.IndexOf(GameDataManager.Instance.selectedWorld);
                    int sectorIndex = GameDataManager.Instance.selectedWorld.sectors.IndexOf(currentSector);
                    int completedSectorsInPreviousWorlds = 0;
                    for (int i = 0; i < worldIndex; i++)
                    {
                        completedSectorsInPreviousWorlds += MenuManager.Instance.allWorlds[i].sectors.Count;
                    }
                    globalDifficultyLevel = completedSectorsInPreviousWorlds + sectorIndex;
                }
            }
        }
        if (currentMode == GameMode.Story && currentSector == null)
        {
            Debug.LogError("Modalità Storia selezionata ma nessun SectorData fornito! Avvio in Endless.");
            currentMode = GameMode.Endless;
        }
        if (currentMode == GameMode.Endless)
        {
            InitializeEndlessMode();
        }
        Debug.Log("Modalità di Gioco Avviata: " + currentMode);
    }

    void InitializeEndlessMode()
    {
        if (endlessModeType == EndlessModeType.Continuous)
        {
            endlessState = EndlessContinuousState.Spawning;
            bossTimer = bossIntervalInMinutes * 60f;
            currentSpawnInterval = initialSpawnInterval;
            currentEnemyCap = initialEnemyCap;
            newEnemyTimer = newEnemyIntroductionInterval;
            availableEnemyPool = new List<GameObject>(endlessFullEnemyPool);
            activeSpawnPool.Clear();
            Debug.Log($"Inizio Endless Continuo con {startingEnemyTypeCount} tipi di nemici.");
            for (int i = 0; i < startingEnemyTypeCount; i++)
            {
                AddNewEnemyToPool();
            }
        }
    }

    public float GetCurrentStatMultiplier()
    {
        float timeBasedDifficulty = (currentMode == GameMode.Endless) ? (survivalTimer / 60f) * 2 : 0; // Aggiunge difficoltà basata sul tempo in endless
        float totalDifficultySteps;
        if (currentMode == GameMode.Story)
        {
            const int averageWavesPerSector = 5;
            totalDifficultySteps = (globalDifficultyLevel * averageWavesPerSector) + (stageNumber - 1);
        }
        else
        {
            totalDifficultySteps = stageNumber - 1 + timeBasedDifficulty;
        }
        float currentGrowthRate = (currentMode == GameMode.Story) ? storyGrowthRate : endlessGrowthRate;
        return 1f + totalDifficultySteps * currentGrowthRate;
    }

    public void BeginSpawning()
    {
        gameHasStarted = true;
        if (currentMode == GameMode.Story || (currentMode == GameMode.Endless && endlessModeType == EndlessModeType.Waves))
        {
            isSpawningWave = true;
            StartCoroutine(SpawnStageCoroutine());
        }
        // Per la modalità continua, l'Update si occuperà di tutto.
    }

    void Update()
    {
        if (!gameHasStarted) return;

        if (currentMode == GameMode.Story || (currentMode == GameMode.Endless && endlessModeType == EndlessModeType.Waves))
        {
            survivalTimer += Time.deltaTime;
            UpdateWaveBasedMode();
        }
        else if (currentMode == GameMode.Endless && endlessModeType == EndlessModeType.Continuous)
        {
            // In modalità continua, il timer di sopravvivenza avanza solo durante lo spawn normale
            if (endlessState == EndlessContinuousState.Spawning)
            {
                survivalTimer += Time.deltaTime;
                bossTimer -= Time.deltaTime;
                newEnemyTimer -= Time.deltaTime;
            }
            UpdateEndlessContinuousMode();
        }
    }
    
    void UpdateWaveBasedMode()
    {
        if (isSpawningWave) return; // Non controllare se stiamo ancora generando nemici

        // --- LOGICA DI CONTROLLO FINE ONDATA CORRETTA ---
        bool waveCleared = false;
        if (isBossWave)
        {
            // Se è un'ondata di boss, controlliamo che non ci siano più oggetti con il tag "Boss"
            if (GameObject.FindGameObjectWithTag("Boss") == null)
            {
                waveCleared = true;
            }
        }
        else
        {
            // Se è un'ondata normale, controlliamo che non ci siano più oggetti con il tag "Enemy"
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                waveCleared = true;
            }
        }

        // Se l'ondata è finita, procedi
        if (waveCleared)
        {
            if (isBossWave)
            {
                isBossWave = false; // Resetta il flag
                if (currentMode == GameMode.Story)
                {
                    EndStorySector();
                    return;
                }
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
            }
            NextStage();
        }
    }


    void UpdateEndlessContinuousMode()
    {
        switch (endlessState)
        {
            case EndlessContinuousState.Spawning:
                HandleContinuousSpawning();
                break;
            case EndlessContinuousState.BossTransition:
                break;
            case EndlessContinuousState.BossFight:
                if (GameObject.FindGameObjectWithTag("Boss") == null)
        {
                    EndBossFight();
                }
                break;
        }
    }

    void HandleContinuousSpawning()
    {
            float timeInSeconds = survivalTimer;
            float capLerpFactor = Mathf.Clamp01(timeInSeconds / (timeToReachMaxCapInMinutes * 60f));
            currentEnemyCap = Mathf.RoundToInt(Mathf.Lerp(initialEnemyCap, maxEnemyCap, capLerpFactor));
            float intervalLerpFactor = Mathf.Clamp01(timeInSeconds / (timeToReachMinIntervalInMinutes * 60f));
            currentSpawnInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, intervalLerpFactor);
        
            if (newEnemyTimer <= 0)
            {
                AddNewEnemyToPool();
                newEnemyTimer = newEnemyIntroductionInterval;
            }
        
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                if (GameObject.FindGameObjectsWithTag("Enemy").Length < currentEnemyCap)
                {
                    SpawnRandomEndlessEnemy();
                }
                spawnTimer = currentSpawnInterval;
            }
        
            if (bossTimer <= 0)
            {
                StartBossFight();
        }   
    }
    
    void StartBossFight()
    {
        Debug.Log("INIZIO COMBATTIMENTO BOSS!");
        endlessState = EndlessContinuousState.BossTransition;
        StartCoroutine(SpawnBossCoroutine());
    }

    void EndBossFight()
    {
        Debug.Log("FINE COMBATTIMENTO BOSS!");
        endlessState = EndlessContinuousState.Spawning;
        bossTimer = bossIntervalInMinutes * 60f;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
    }
    
    void EndStorySector()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            SectorObjective objectivesAchieved = SectorObjective.SECTOR_COMPLETED;
            if ((float)player.currentHealth / player.maxHealth > 0.7f) objectivesAchieved |= SectorObjective.HEALTH_OVER_70_PERCENT;
            if (!player.tookDamageThisRun) objectivesAchieved |= SectorObjective.NO_DAMAGE_TAKEN;
            VictoryManager.SetVictoryStats(currentSector, player.sessionCoins, player.sessionSpecialCurrency, objectivesAchieved);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene("VictoryScene");
    }

    void AddNewEnemyToPool()
    {
        if (availableEnemyPool.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableEnemyPool.Count);
            GameObject newEnemy = availableEnemyPool[randomIndex];
            activeSpawnPool.Add(newEnemy);
            availableEnemyPool.RemoveAt(randomIndex);
            Debug.Log($"NUOVO NEMICO INTRODOTTO AL POOL DI SPAWN: {newEnemy.name}");
        }
    }

    public void SpawnEnemy(Vector3 position, GameObject enemyToSpawn)
    {
        if (enemyToSpawn == null) return;
        GameObject e = Instantiate(enemyToSpawn, position, enemyToSpawn.transform.rotation);
        EnemyStats es = e.GetComponent<EnemyStats>();
        if (es != null) // Sempre vero se il prefab è corretto
        {
            if (es.allowStatScaling)
            {
                // Applica lo scaling chiamando il nuovo metodo centralizzato in EnemyStats
                es.ApplyStatScaling(GetCurrentStatMultiplier());
            }

            SuperBossAI superBoss = e.GetComponent<SuperBossAI>();
            if (superBoss != null) 
            {
                superBoss.InitializeBoss(GetCurrentStatMultiplier());
            }
        }

        if (e.CompareTag("Untagged"))
        {
            e.tag = "Enemy";
        }
    }

    private IEnumerator SpawnStageCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        List<GameObject> enemiesToUse = (currentMode == GameMode.Story && currentSector != null) ? currentSector.availableEnemies : endlessFullEnemyPool;

        if (enemiesToUse == null || enemiesToUse.Count == 0)
        {
            isSpawningWave = false;
            yield break;
        }

        for (int i = 0; i < enemiesPerStage; i++)
        {
            GameObject prefabToSpawn = enemiesToUse[UnityEngine.Random.Range(0, enemiesToUse.Count)];
            float enemyWidth = prefabToSpawn.GetComponent<SpriteRenderer>().bounds.extents.x;
            float safeSpawnXMin = spawnXMin + enemyWidth;
            float safeSpawnXMax = spawnXMax - enemyWidth;
            float xPos = UnityEngine.Random.Range(safeSpawnXMin, safeSpawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);
            
            // Creiamo l'istanza qui, ma applichiamo lo scaling DOPO
            GameObject enemyInstance = Instantiate(prefabToSpawn, pos, prefabToSpawn.transform.rotation);
            enemyInstance.GetComponent<EnemyStats>()?.ApplyStatScaling(GetCurrentStatMultiplier());
            
            float currentEliteChance = eliteSpawnChance + (globalDifficultyLevel * 0.01f);
            if (stageNumber >= stageToStartElites && UnityEngine.Random.value < currentEliteChance)
            {
                PromoteToElite(enemyInstance);
            }

            float delay = UnityEngine.Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
        isSpawningWave = false;
    }

    void SpawnRandomEndlessEnemy()
    {
        if (activeSpawnPool.Count == 0) return;
        GameObject prefabToSpawn = activeSpawnPool[UnityEngine.Random.Range(0, activeSpawnPool.Count)];
        
        float enemyWidth = prefabToSpawn.GetComponent<SpriteRenderer>().bounds.extents.x;
        float safeSpawnXMin = spawnXMin + enemyWidth;
        float safeSpawnXMax = spawnXMax - enemyWidth;
        float xPos = UnityEngine.Random.Range(safeSpawnXMin, safeSpawnXMax);
        Vector3 pos = new Vector3(xPos, spawnY, 0f);

        // Usiamo il metodo SpawnEnemy per creare e scalare il nemico
        GameObject enemyInstance = Instantiate(prefabToSpawn, pos, prefabToSpawn.transform.rotation);
        enemyInstance.GetComponent<EnemyStats>()?.ApplyStatScaling(GetCurrentStatMultiplier());

        float eliteIntroTimeInMinutes = 1f;
        if (survivalTimer / 60f > eliteIntroTimeInMinutes && UnityEngine.Random.value < eliteSpawnChance)
        {
            PromoteToElite(enemyInstance);
        }
    }

    private void PromoteToElite(GameObject enemyInstance)
    {
        if(enemyInstance == null) return;
        EliteStats eliteComponent = enemyInstance.AddComponent<EliteStats>();
        eliteComponent.Initialize(eliteHealthMultiplier, eliteDamageMultiplier, eliteSpeedMultiplier, eliteAttackSpeedMultiplier, eliteCoinMultiplier, eliteXpMultiplier, eliteColorTint);
        if (eliteModifierPrefabs != null && eliteModifierPrefabs.Count > 0)
        {
            GameObject modifierPrefab = eliteModifierPrefabs[UnityEngine.Random.Range(0, eliteModifierPrefabs.Count)];
            EliteModifier sourceModifier = modifierPrefab.GetComponent<EliteModifier>();
            if (sourceModifier != null)
            {
                EliteModifier newModifier = enemyInstance.AddComponent(sourceModifier.GetType()) as EliteModifier;
                newModifier.CopyProperties(sourceModifier);
                newModifier.Activate(enemyInstance.GetComponent<EnemyStats>());
            }
        }
    }

    public float GetSurvivalTime() => survivalTimer;

    public void NextStage()
    {
        isSpawningWave = true;
        stageNumber++;
        if (currentMode == GameMode.Story)
        {
            if (stageNumber > currentSector.numberOfWaves) StartCoroutine(SpawnGuardianBossCoroutine());
            else StartCoroutine(SpawnStageCoroutine());
        }
        else // Modalità Endless a Ondate
        {
            if (stageNumber == endlessSuperBossStage) StartCoroutine(SpawnGuardianBossCoroutine());
            else if (stageNumber % 10 == 0) StartCoroutine(SpawnBossCoroutine());
            else StartCoroutine(SpawnStageCoroutine());
        }
    }

    private IEnumerator SpawnBossCoroutine()
    {
        // --- CORREZIONE LOGICA BOSS ---
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        yield return new WaitForSeconds(2.5f);
        
        if (endlessBossPrefabs != null && endlessBossPrefabs.Count > 0)
        {
            GameObject randomBossPrefab = endlessBossPrefabs[UnityEngine.Random.Range(0, endlessBossPrefabs.Count)];
            SpawnEnemy(new Vector3(0, spawnY, 0), randomBossPrefab);
        }
        
        if (currentMode == GameMode.Endless && endlessModeType == EndlessModeType.Continuous)
        {
            endlessState = EndlessContinuousState.BossFight;
        }
        else
        {
            isSpawningWave = false;
        }
    }

    private IEnumerator SpawnGuardianBossCoroutine()
    {
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        yield return new WaitForSeconds(2.5f);
        GameObject bossToSpawn = (currentMode == GameMode.Story) ? currentSector.guardianBossPrefab : endlessSuperBossPrefab;
        SpawnEnemy(new Vector3(0, spawnY + 2f, 0), bossToSpawn);
        
        if(endlessModeType == EndlessModeType.Waves || currentMode == GameMode.Story)
        {
        isSpawningWave = false;
        }
    }
}