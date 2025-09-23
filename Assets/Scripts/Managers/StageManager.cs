using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Configurazione Modalità Endless")]
    [Tooltip("Liste di prefab usate solo se si avvia la modalità Endless.")]
    public List<GameObject> endlessEnemyPrefabs;
    public List<GameObject> endlessElitePrefabs;
    public List<GameObject> endlessBossPrefabs;
    public GameObject endlessSuperBossPrefab;
    public int endlessSuperBossStage = 20;

    [Header("Gestione Stage Globale")]
    // --- MODIFICA: Variabili di crescita separate ---
    [Tooltip("Il tasso di crescita per la modalità Storia. Deve essere basso per una progressione graduale.")]
    public float storyGrowthRate = 0.05f;
    [Tooltip("Il tasso di crescita per la modalità Endless. Può essere più alto per una sfida crescente.")]
    public float endlessGrowthRate = 0.15f;
    // --- FINE MODIFICA ---
    public int enemiesPerStage = 8;
    public int stageToStartElites = 3; // Abbassato per vederli prima
    public float spawnY = 6f;
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    [Header("Probabilità di Spawn")]
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.1f;

    private int globalDifficultyLevel = 0;
    
    [HideInInspector]
    public int stageNumber = 1; // Rimosso dall'header, gestito internamente

    private bool isSpawningWave = false;
    private bool isBossWave = false;
    private bool gameHasStarted = false;
    private GameMode currentMode;
    private SectorData currentSector;

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
        if (gameHasStarted && !isSpawningWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && FindFirstObjectByType<BossTurret>() == null)
        {
            if (isBossWave)
            {
                isBossWave = false;
                if (currentMode == GameMode.Story)
                {
                    Debug.Log("SETTORE " + currentSector.sectorName + " COMPLETATO!");
                    // Qui puoi aggiungere una schermata di vittoria, per ora torniamo al menu
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("MainMenu");
                    return;
                }

                // In modalità endless, ripristina la musica normale
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
            }
            NextStage();
        }
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

        List<GameObject> enemiesToUse;
        List<GameObject> elitesToUse;

        if (currentMode == GameMode.Story)
        {
            // Ora usiamo direttamente le liste di prefab dal SectorData
            enemiesToUse = currentSector.availableEnemies;
            elitesToUse = currentSector.availableElites;
        }
        else
        {
            enemiesToUse = endlessEnemyPrefabs;
            elitesToUse = endlessElitePrefabs;
        }

        // Per ora, questa logica rimane semplice come richiesto (archetipo "Mixed")
        for (int i = 0; i < enemiesPerStage; i++)
        {
            GameObject prefabToSpawn;
            // La probabilità di elite può dipendere anche dalla difficoltà globale!
            float currentEliteChance = eliteSpawnChance + (globalDifficultyLevel * 0.01f);
            
            if (stageNumber >= stageToStartElites && elitesToUse != null && elitesToUse.Count > 0 && Random.value < currentEliteChance)
            {
                prefabToSpawn = elitesToUse[Random.Range(0, elitesToUse.Count)];
            }
            else
            {
                prefabToSpawn = enemiesToUse[Random.Range(0, enemiesToUse.Count)];
            }
            float xPos = Random.Range(spawnXMin, spawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);
            SpawnEnemy(pos, prefabToSpawn);
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
        isSpawningWave = false;
    }

    private IEnumerator SpawnBossCoroutine() // Solo per Endless
    {
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);
        yield return new WaitForSeconds(2.5f);
        GameObject randomBossPrefab = endlessBossPrefabs[Random.Range(0, endlessBossPrefabs.Count)];
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