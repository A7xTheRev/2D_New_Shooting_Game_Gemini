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
    public int stageNumber = 1; // In Story mode, è il livello. In Endless, è l'ondata.
    public float growthRate = 0.18f;
    public int enemiesPerStage = 8;
    public int stageToStartElites = 5;
    public float spawnY = 6f;
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    [Header("Probabilità di Spawn")]
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.1f;

    // Variabili interne per gestire la modalità corrente
    private bool isSpawningWave = false;
    private bool isBossWave = false;
    private bool gameHasStarted = false;
    private GameMode currentMode;
    private SectorData currentSector;

    void Start()
    {
        // All'avvio, decide quale modalità di gioco eseguire
        if (GameDataManager.Instance != null)
        {
            currentMode = GameDataManager.Instance.selectedGameMode;
            currentSector = GameDataManager.Instance.selectedSector;
        }

        // Fallback di sicurezza se la modalità storia è selezionata ma non ci sono dati
        if (currentMode == GameMode.Story && currentSector == null)
        {
            Debug.LogError("Modalità Storia selezionata ma nessun SectorData fornito! Avvio in modalità Endless come fallback.");
            currentMode = GameMode.Endless;
        }

        Debug.Log("Modalità di Gioco Avviata: " + currentMode);
    }

    public void BeginSpawning()
    {
        gameHasStarted = true;
        isSpawningWave = true;
        StartCoroutine(SpawnStageCoroutine());
    }

    void Update()
    {
        // Controlla se l'ondata è finita per passare alla successiva
        if (gameHasStarted && !isSpawningWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && FindFirstObjectByType<BossTurret>() == null)
        {
            // Se era una boss wave, gestisci la fine della battaglia
            if (isBossWave)
            {
                isBossWave = false;
                
                // Se siamo in modalità Storia e il boss è stato sconfitto, il settore è completato
                if (currentMode == GameMode.Story)
                {
                    Debug.Log("SETTORE " + currentSector.sectorName + " COMPLETATO!");
                    // Qui puoi aggiungere una schermata di vittoria, per ora torniamo al menu
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("MainMenu");
                    return; // Ferma l'esecuzione per evitare di chiamare NextStage()
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

        // Logica a bivi basata sulla modalità di gioco
        if (currentMode == GameMode.Story)
        {
            // Se siamo nell'ultimo livello del settore, spawna il boss guardiano
            if (stageNumber >= currentSector.numberOfLevels) // Usiamo >= per sicurezza
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
        if (es != null)
        {
            float multiplier = 1f + (stageNumber - 1) * growthRate;
            if (es.allowStatScaling)
            {
                es.maxHealth = Mathf.RoundToInt(es.maxHealth * multiplier);
                es.currentHealth = es.maxHealth;
            }
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

        // Sceglie da quale lista di nemici pescare in base alla modalità
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

        for (int i = 0; i < enemiesPerStage; i++)
        {
            GameObject prefabToSpawn;
            if (stageNumber >= stageToStartElites && elitesToUse != null && elitesToUse.Count > 0 && Random.value < eliteSpawnChance)
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