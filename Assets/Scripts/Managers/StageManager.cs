using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Liste Prefab")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> elitePrefabs;
    public List<GameObject> bossPrefabs;

    // --- NUOVA SEZIONE PER IL SUPER BOSS ---
    [Header("Super Boss")]
    [Tooltip("Trascina qui il prefab del Super Boss.")]
    public GameObject superBossPrefab;
    [Tooltip("L'ondata in cui apparirà il Super Boss.")]
    public int superBossStage = 20;
    // --- FINE NUOVA SEZIONE ---

    [Header("Probabilità di Spawn")]
    [Range(0f, 1f)]
    public float eliteSpawnChance = 0.1f;

    [Header("Gestione Stage")]
    public int stageNumber = 1;
    public float growthRate = 0.18f;
    public int enemiesPerStage = 8;
    public int stageToStartElites = 5;
    public float spawnY = 6f;

    [Header("Limiti orizzontali spawn")]
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;

    [Header("Limiti ritardo spawn")]
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    private bool isSpawningWave = false;
    private bool isBossWave = false;

    void Start()
    {
        isSpawningWave = true;
        StartCoroutine(SpawnStageCoroutine());
    }

    void Update()
    {
        if (!isSpawningWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && FindFirstObjectByType<BossTurret>() == null)
        {
            if (isBossWave)
            {
                isBossWave = false;
                AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic);
            }
            NextStage();
        }
    }

    public void NextStage()
    {
        isSpawningWave = true;
        stageNumber++;

        // --- LOGICA DI SPAWN AGGIORNATA ---
        // 1. Controlla se è lo stage del Super Boss
        if (stageNumber == superBossStage && superBossPrefab != null)
        {
            StartCoroutine(SpawnSuperBossCoroutine());
        }
        // 2. Altrimenti, controlla se è uno stage per un boss normale
        else if (stageNumber % 10 == 0)
        {
            StartCoroutine(SpawnBossCoroutine());
        }
        // 3. Altrimenti, è un'ondata normale
        else
        {
            StartCoroutine(SpawnStageCoroutine());
        }
        // --- FINE LOGICA AGGIORNATA ---
    }

    public void SpawnEnemy(Vector3 position, GameObject enemyToSpawn)
    {
        if (enemyToSpawn == null) return;
        GameObject e = Instantiate(enemyToSpawn, position, enemyToSpawn.transform.rotation);
        EnemyStats es = e.GetComponent<EnemyStats>();
        if (es != null)
        {
            float multiplier = 1f;
            if (stageNumber > 1) {
                multiplier = 1f + (stageNumber - 1) * growthRate;
            }

            // --- LOGICA MODIFICATA ---
            // 1. Applica lo scaling solo se il flag è attivo
            if (es.allowStatScaling)
            {
                es.maxHealth = Mathf.RoundToInt(es.maxHealth * multiplier);
                es.currentHealth = es.maxHealth;
            }
            
            // 2. Controlla se l'oggetto appena creato è un Super Boss
            SuperBossAI superBoss = e.GetComponent<SuperBossAI>();
            if (superBoss != null)
            {
                // Se sì, passagli il moltiplicatore di vita
                superBoss.InitializeBoss(multiplier);
            }
            // --- FINE MODIFICA ---
        }
        e.tag = "Enemy";
    }

    private IEnumerator SpawnStageCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < enemiesPerStage; i++)
        {
            GameObject prefabToSpawn;

            if (stageNumber >= stageToStartElites && elitePrefabs.Count > 0 && Random.value < eliteSpawnChance)
            {
                prefabToSpawn = elitePrefabs[Random.Range(0, elitePrefabs.Count)];
            }
            else
            {
                prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            }

            float xPos = Random.Range(spawnXMin, spawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);
            SpawnEnemy(pos, prefabToSpawn);

            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }

        isSpawningWave = false;
    }

    private IEnumerator SpawnBossCoroutine()
    {
        isBossWave = true; // Segna che è iniziata una boss wave
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic); // CAMBIA LA MUSICA!

        Debug.Log($"WAVE {stageNumber}: ARRIVA UN BOSS!");
        yield return new WaitForSeconds(2.5f);
        GameObject randomBossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
        Vector3 bossSpawnPosition = new Vector3(0, spawnY, 0);
        SpawnEnemy(bossSpawnPosition, randomBossPrefab);
        isSpawningWave = false;
    }
    private IEnumerator SpawnSuperBossCoroutine()
    {
        isBossWave = true;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bossMusic);

        Debug.Log($"WAVE {stageNumber}: ARRIVA IL SUPER BOSS!");
        yield return new WaitForSeconds(2.5f);
        
        // La posizione di spawn potrebbe essere più in alto per dargli spazio per l'entrata in scena
        Vector3 bossSpawnPosition = new Vector3(0, spawnY + 2f, 0); 
        
        // Spawna il prefab specifico del Super Boss
        SpawnEnemy(bossSpawnPosition, superBossPrefab);
        
        isSpawningWave = false;
    }
}