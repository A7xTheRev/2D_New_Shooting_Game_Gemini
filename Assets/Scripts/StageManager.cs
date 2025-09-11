using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Prefab Nemici")]
    public List<GameObject> enemyPrefabs;
    public List<GameObject> bossPrefabs; // Ora è una lista per contenere più boss

    [Header("Gestione Stage")]
    public int stageNumber = 1;
    public float growthRate = 0.15f;
    public int enemiesPerStage = 5;
    public float spawnY = 6f;

    [Header("Limiti orizzontali spawn")]
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;

    [Header("Limiti ritardo spawn")]
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;
    
    private bool isSpawningWave = false;

    void Start()
    {
        // Controllo di sicurezza aggiornato per la lista dei boss
        if ((enemyPrefabs == null || enemyPrefabs.Count == 0) || (bossPrefabs == null || bossPrefabs.Count == 0))
        {
            Debug.LogError("Le liste dei prefab nemici o dei boss non sono state assegnate nello StageManager!");
            this.enabled = false;
            return;
        }
        
        isSpawningWave = true;
        StartCoroutine(SpawnStageCoroutine());
    }

    void Update()
    {
        if (!isSpawningWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            NextStage();
        }
    }

    public void NextStage()
    {
        isSpawningWave = true; 
        stageNumber++;

        if (stageNumber % 10 == 0)
        {
            StartCoroutine(SpawnBossCoroutine());
        }
        else
        {
            StartCoroutine(SpawnStageCoroutine());
        }
    }

    public void SpawnEnemy(Vector3 position, GameObject enemyToSpawn)
    {
        if (enemyToSpawn == null) return;

        GameObject e = Instantiate(enemyToSpawn, position, enemyToSpawn.transform.rotation);

        EnemyStats es = e.GetComponent<EnemyStats>();
        if (es != null)
        {
            if (stageNumber > 1) {
                float multiplier = 1f + (stageNumber - 1) * growthRate;
                es.maxHealth = Mathf.RoundToInt(es.maxHealth * multiplier);
                es.currentHealth = es.maxHealth;
            }
        }

        e.tag = "Enemy";
    }
    
    private IEnumerator SpawnStageCoroutine()
    {
        yield return new WaitForSeconds(1.5f); 
        
        for (int i = 0; i < enemiesPerStage; i++)
        {
            GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            float xPos = Random.Range(spawnXMin, spawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);
            SpawnEnemy(pos, randomEnemyPrefab);
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
        
        isSpawningWave = false;
    }

    private IEnumerator SpawnBossCoroutine()
    {
        Debug.Log($"WAVE {stageNumber}: ARRIVA UN BOSS!");
        yield return new WaitForSeconds(2.5f);

        // Sceglie un boss a caso dalla nuova lista
        GameObject randomBossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Count)];
        
        Vector3 bossSpawnPosition = new Vector3(0, spawnY, 0);
        SpawnEnemy(bossSpawnPosition, randomBossPrefab);

        isSpawningWave = false;
    }
}