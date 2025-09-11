using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossRandomPatrolAI : MonoBehaviour
{
    [Header("Area di Pattugliamento")]
    [Tooltip("Un rettangolo invisibile in cui il boss si muoverà. X e Y sono l'angolo in basso a sinistra.")]
    public Rect patrolArea = new Rect(-7f, 1f, 14f, 3f);

    [Header("Impostazioni di Movimento")]
    [Tooltip("Quanto vicino deve arrivare al punto target prima di sceglierne un altro.")]
    public float waypointReachedThreshold = 0.1f;

    [Header("Impostazioni di Spawn Minion")]
    public List<GameObject> minionPrefabs;
    public float spawnInterval = 5f;
    public Transform spawnPoint;
    
    private Vector2 targetPosition;
    private float spawnTimer;
    private StageManager stageManager;
    private EnemyStats stats; // Riferimento al componente con le statistiche

    void Awake()
    {
        // Ottiene il riferimento al componente EnemyStats sullo stesso oggetto
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        spawnTimer = spawnInterval;

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        SetNewRandomTarget();
    }

    void Update()
    {
        HandleMovement();
        HandleMinionSpawning();
    }

    void HandleMovement()
    {
        if (Vector2.Distance(transform.position, targetPosition) < waypointReachedThreshold)
        {
            SetNewRandomTarget();
        }

        // Usa la velocità presa da EnemyStats
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, stats.moveSpeed * Time.deltaTime);
    }

    void SetNewRandomTarget()
    {
        float randomX = Random.Range(patrolArea.x, patrolArea.x + patrolArea.width);
        float randomY = Random.Range(patrolArea.y, patrolArea.y + patrolArea.height);
        
        targetPosition = new Vector2(randomX, randomY);
    }

    // ... (HandleMinionSpawning e SpawnMinion rimangono invariati) ...
    void HandleMinionSpawning()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnMinion();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnMinion()
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return;

        GameObject minionToSpawn = minionPrefabs[Random.Range(0, minionPrefabs.Count)];
        GameObject minionInstance = Instantiate(minionToSpawn, spawnPoint.position, spawnPoint.rotation);
        
        EnemyStats minionStats = minionInstance.GetComponent<EnemyStats>();
        if (minionStats != null && stageManager != null)
        {
            if (stageManager.stageNumber > 1)
            {
                float multiplier = 1f + (stageManager.stageNumber - 1) * stageManager.growthRate;
                minionStats.maxHealth = Mathf.RoundToInt(minionStats.maxHealth * multiplier);
                minionStats.currentHealth = minionStats.maxHealth;
            }
        }
    }
}