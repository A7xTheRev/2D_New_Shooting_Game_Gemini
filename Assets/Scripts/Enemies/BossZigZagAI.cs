using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossZigZagAI : MonoBehaviour
{
    [Header("Impostazioni di Movimento")]
    public float directionChangeInterval = 2f;
    public float patrolYPosition = 3.5f;

    [Header("Impostazioni di Spawn Minion")]
    public List<GameObject> minionPrefabs;
    public float spawnInterval = 5f;
    public Transform spawnPoint;

    private float directionTimer;
    private float spawnTimer;
    private int horizontalDirection = 1;
    private bool hasReachedPatrolZone = false;

    private StageManager stageManager;
    private EnemyStats stats;
    private float cleanupYThreshold;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        stageManager = FindFirstObjectByType<StageManager>();
        directionTimer = directionChangeInterval;
        spawnTimer = spawnInterval;
        horizontalDirection = Random.value < 0.5f ? 1 : -1;

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
    }

    void Update()
    {
        if (transform.position.y < cleanupYThreshold)
        {
            Debug.LogWarning("Un Boss è uscito dallo schermo ed è stato distrutto!");
            Destroy(gameObject);
            return;
        }

        HandleMovement();
        HandleMinionSpawning();
    }

    void HandleMovement()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            horizontalDirection *= -1;
            directionTimer = directionChangeInterval;
        }
        
        float currentVerticalSpeed = 0;
        if (!hasReachedPatrolZone)
        {
            currentVerticalSpeed = -stats.moveSpeed;
            if (transform.position.y <= patrolYPosition)
            {
                hasReachedPatrolZone = true;
                currentVerticalSpeed = 0;
            }
        }
        
        float currentHorizontalSpeed = stats.moveSpeed;
        Vector2 moveDirection = new Vector2(currentHorizontalSpeed * horizontalDirection, currentVerticalSpeed);
        
        transform.position += (Vector3)moveDirection * Time.deltaTime;
    }

    void HandleMinionSpawning()
    {
        if (!hasReachedPatrolZone) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnMinion();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnMinion()
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0)
        {
            Debug.LogWarning("La lista dei minion da spawnare per il boss è vuota.");
            return;
        }

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null && stats != null)
            {
                playerStats.TakeDamage(stats.contactDamage);
            }
        }
    }
}