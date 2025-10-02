using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class StageManager : MonoBehaviour
{
    [Header("Configurazione Modalità Endless")]
    [Tooltip("Liste di prefab usate solo se si avvia la modalità Endless.")]
    public List<GameObject> endlessEnemyPrefabs;
    // La lista degli elite prefab ora non è più necessaria, la lasciamo per retrocompatibilità ma non la useremo più
    // public List<GameObject> endlessElitePrefabs; 
    public List<GameObject> endlessBossPrefabs;
    public GameObject endlessSuperBossPrefab;
    public int endlessSuperBossStage = 20;

    // --- NUOVA SEZIONE PER GLI ELITE DINAMICI ---
    [Header("Configurazione Elite Dinamici")]
    [Tooltip("Trascina qui i 'Prefab dei Modificatori' che hai creato. Uno di questi verrà scelto casualmente per ogni Elite.")]
    public List<GameObject> eliteModifierPrefabs;

    // --- NUOVA SEZIONE DI CONFIGURAZIONE ---
    [Header("Configurazione Statistiche Elite")]
    public float eliteHealthMultiplier = 4f;
    public float eliteDamageMultiplier = 2f;
    public float eliteSpeedMultiplier = 1.2f;
    public float eliteAttackSpeedMultiplier = 1.5f;
    public float eliteCoinMultiplier = 5f;
    public float eliteXpMultiplier = 4f;
    public Color eliteColorTint = new Color(0.88f, 0.52f, 1f); // Viola
    // --- FINE NUOVA SEZIONE ---

    [Header("Gestione Stage Globale")]
    // --- MODIFICA: Variabili di crescita separate ---
    [Tooltip("Il tasso di crescita per la modalità Storia. Deve essere basso per una progressione graduale.")]
    public float storyGrowthRate = 0.05f;
    [Tooltip("Il tasso di crescita per la modalità Endless. Può essere più alto per una sfida crescente.")]
    public float endlessGrowthRate = 0.15f;
    public int enemiesPerStage = 8;
    public int stageToStartElites = 3;
    public float spawnY = 6f;
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    [Header("Probabilità di Spawn")]
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.1f;

    
    private SectorData currentSector;
    private GameMode currentMode;
    private bool isSpawningWave;
    private bool gameHasStarted; // = false
    private float survivalTimer; // = 0f;
    private int globalDifficultyLevel; //  = 0;
    public int stageNumber = 1;
    private bool isBossWave;

    
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
                    Debug.Log($"Inizio Settore. Livello Difficoltà Globale Base: {globalDifficultyLevel}");
                }
            }
        }

        if (currentMode == GameMode.Story && currentSector == null)
        {
            Debug.LogError("Modalità Storia selezionata ma nessun SectorData fornito! Avvio in Endless.");
            currentMode = GameMode.Endless;
        }

        Debug.Log("Modalità di Gioco Avviata: " + currentMode);
    }

    // --- NUOVA FUNZIONE PUBBLICA ---
    // Qualsiasi script ora può chiedere allo StageManager il moltiplicatore di difficoltà corretto.
    public float GetCurrentStatMultiplier()
    {
        float totalDifficultySteps = 0;
        if (currentMode == GameMode.Story)
        {
            const int averageWavesPerSector = 5;
            totalDifficultySteps = (globalDifficultyLevel * averageWavesPerSector) + (stageNumber - 1);
        }
        else
        {
            totalDifficultySteps = stageNumber - 1;
        }

        float currentGrowthRate = (currentMode == GameMode.Story) ? storyGrowthRate : endlessGrowthRate;
        float multiplier = 1f + totalDifficultySteps * currentGrowthRate;
        return multiplier;
    }
    // --- FINE NUOVA FUNZIONE ---

    public void BeginSpawning()
    {
        gameHasStarted = true;
        isSpawningWave = true;
        StartCoroutine(SpawnStageCoroutine());
    }

    void Update()
    {
        if (gameHasStarted)
        {
            survivalTimer += Time.deltaTime;
        }

        if (gameHasStarted && !isSpawningWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && FindFirstObjectByType<BossTurret>() == null)
        {
            if (isBossWave)
            {
                isBossWave = false;
                if (currentMode == GameMode.Story)
                {
                    // --- NUOVA LOGICA DI COMPLETAMENTO SETTORE ---
                    
                    // 1. Raccogli i dati della performance
                    PlayerStats player = FindFirstObjectByType<PlayerStats>();
                    if (player != null)
                    {
                        // 1. Calcola gli obiettivi raggiunti
                        SectorObjective objectivesAchieved = SectorObjective.NONE;

                        // Obiettivo 1: Hai completato il settore (sempre vero se arrivi qui)
                        objectivesAchieved |= SectorObjective.SECTOR_COMPLETED;

                        // Obiettivo 2: Hai finito con più del 70% di vita?
                        float healthPercentage = (float)player.currentHealth / player.maxHealth;
                        if (healthPercentage > 0.7f)
                        {
                            objectivesAchieved |= SectorObjective.HEALTH_OVER_70_PERCENT;
                        }

                        // Obiettivo 3: Non hai subito danni?
                        if (!player.tookDamageThisRun)
                        {
                            objectivesAchieved |= SectorObjective.NO_DAMAGE_TAKEN;
                        }

                        // 2. Passa tutti i dati al VictoryManager
                        VictoryManager.SetVictoryStats(currentSector, player.sessionCoins, player.sessionSpecialCurrency, objectivesAchieved);
                    }
                    
                    // 3. Carica la scena di vittoria
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("VictoryScene");
                    return;
                    // --- FINE MODIFICA ---
                }

                // Se era un boss della modalità Endless, ripristina la musica
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
            }
            
            // Passa all'ondata successiva
            NextStage();
        }
    }
    public float GetSurvivalTime()
    {
        return survivalTimer;
    }

    public void NextStage()
    {
        isSpawningWave = true;
        stageNumber++;

        if (currentMode == GameMode.Story)
        {
            if (stageNumber > currentSector.numberOfWaves)
            {
                StartCoroutine(SpawnGuardianBossCoroutine());
            }
            else
            {
                StartCoroutine(SpawnStageCoroutine());
            }
        }
        else // Modalità Endless
        {
            if (stageNumber == endlessSuperBossStage) StartCoroutine(SpawnGuardianBossCoroutine());
            else if (stageNumber % 10 == 0) StartCoroutine(SpawnBossCoroutine());
            else StartCoroutine(SpawnStageCoroutine());
        }
    }

    public void SpawnEnemy(Vector3 position, GameObject enemyToSpawn)
    {
        if (enemyToSpawn == null) return;
        GameObject e = Instantiate(enemyToSpawn, position, enemyToSpawn.transform.rotation);
        EnemyStats es = e.GetComponent<EnemyStats>();

        if (es != null && es.allowStatScaling)
        {
            // Ora usiamo la nuova funzione centralizzata
            float multiplier = GetCurrentStatMultiplier();

                es.maxHealth = Mathf.RoundToInt(es.maxHealth * multiplier);
                es.currentHealth = es.maxHealth;

            SuperBossAI superBoss = e.GetComponent<SuperBossAI>();
            if (superBoss != null)
            {
                superBoss.InitializeBoss(multiplier);
            }
        }
        e.tag = "Enemy";
    }

    private IEnumerator SpawnStageCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        List<GameObject> enemiesToUse = (currentMode == GameMode.Story && currentSector != null) ? currentSector.availableEnemies : endlessEnemyPrefabs;

        for (int i = 0; i < enemiesPerStage; i++)
        {
            // Scegli un prefab di nemico base
            GameObject prefabToSpawn = enemiesToUse[UnityEngine.Random.Range(0, enemiesToUse.Count)];
            
            float xPos = UnityEngine.Random.Range(spawnXMin, spawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);

            GameObject enemyInstance = Instantiate(prefabToSpawn, pos, prefabToSpawn.transform.rotation);

            // Decidi se promuoverlo a Elite
            float currentEliteChance = eliteSpawnChance + (globalDifficultyLevel * 0.01f);
            if (stageNumber >= stageToStartElites && UnityEngine.Random.value < currentEliteChance)
            {
                // 1. Aggiungi il componente EliteStats
                EliteStats eliteComponent = enemyInstance.AddComponent<EliteStats>();

                // 2. Inizializzalo con i valori presi dallo StageManager
                eliteComponent.Initialize(
                    eliteHealthMultiplier, 
                    eliteDamageMultiplier, 
                    eliteSpeedMultiplier, 
                    eliteAttackSpeedMultiplier, 
                    eliteCoinMultiplier, 
                    eliteXpMultiplier, 
                    eliteColorTint
                );
                // --- FINE MODIFICA ---

                // 3. Scegli e aggiungi un modificatore casuale
                if (eliteModifierPrefabs != null && eliteModifierPrefabs.Count > 0)
                {
                    // Scegli un prefab di modificatore a caso dalla lista
                    GameObject modifierPrefab = eliteModifierPrefabs[UnityEngine.Random.Range(0, eliteModifierPrefabs.Count)];
                    EliteModifier sourceModifier = modifierPrefab.GetComponent<EliteModifier>();

                    if (sourceModifier != null)
                    {
                        // Aggiungi un componente dello stesso TIPO del modificatore sorgente
                        EliteModifier newModifier = enemyInstance.AddComponent(sourceModifier.GetType()) as EliteModifier;
                        // Copia le proprietà configurate nel prefab sul nuovo componente
                        newModifier.CopyProperties(sourceModifier);
                        // Attiva il modificatore
                        newModifier.Activate(enemyInstance.GetComponent<EnemyStats>());
                    }
                }
            }

            float delay = UnityEngine.Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
        isSpawningWave = false;
    }

    private IEnumerator SpawnBossCoroutine() // Solo per Endless
    {
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        yield return new WaitForSeconds(2.5f);
        GameObject randomBossPrefab = endlessBossPrefabs[UnityEngine.Random.Range(0, endlessBossPrefabs.Count)];
        SpawnEnemy(new Vector3(0, spawnY, 0), randomBossPrefab);
        isSpawningWave = false;
    }

    private IEnumerator SpawnGuardianBossCoroutine() // Per Story e Endless
    {
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        yield return new WaitForSeconds(2.5f);

        GameObject bossToSpawn = (currentMode == GameMode.Story) ? currentSector.guardianBossPrefab : endlessSuperBossPrefab;
        Vector3 bossSpawnPosition = new Vector3(0, spawnY + 2f, 0); 
        SpawnEnemy(bossSpawnPosition, bossToSpawn);
        isSpawningWave = false;
    }
}