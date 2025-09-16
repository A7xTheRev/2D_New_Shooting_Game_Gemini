using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossRandomPatrolAI : MonoBehaviour
{
    [Header("Impostazioni di Movimento")]
    [Tooltip("Quanto vicino deve arrivare al punto target prima di sceglierne un altro.")]
    public float waypointReachedThreshold = 0.1f;
    [Tooltip("Un piccolo margine dai bordi dello schermo per evitare che il boss si 'incolli' ai lati.")]
    public float patrolAreaPadding = 1f;

    [Header("Impostazioni di Spawn Minion")]
    public List<GameObject> minionPrefabs;
    public float spawnInterval = 5f;
    public Transform spawnPoint;
    
    private Vector2 targetPosition;
    private float spawnTimer;
    private StageManager stageManager;
    private EnemyStats stats;
    private float cleanupYThreshold;

    // Variabili per i confini dell'area di pattugliamento
    private float minX, maxX, minY, maxY;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        stageManager = FindFirstObjectByType<StageManager>();
        spawnTimer = spawnInterval;

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        // --- NUOVA LOGICA DI CALCOLO AUTOMATICO ---
        // Calcola l'area di pattugliamento (metà superiore dello schermo)
        Camera cam = Camera.main;
        // Punto in basso a sinistra della metà superiore (0% larghezza, 50% altezza)
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)); 
        // Punto in alto a destra (100% larghezza, 100% altezza)
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Imposta i confini con un piccolo margine (padding)
        minX = bottomLeft.x + patrolAreaPadding;
        maxX = topRight.x - patrolAreaPadding;
        minY = bottomLeft.y + patrolAreaPadding;
        maxY = topRight.y - patrolAreaPadding;
        // --- FINE NUOVA LOGICA ---

        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
        SetNewRandomTarget();
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
        if (Vector2.Distance(transform.position, targetPosition) < waypointReachedThreshold)
        {
            SetNewRandomTarget();
        }
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, stats.moveSpeed * Time.deltaTime);
    }

    void SetNewRandomTarget()
    {
        // Calcola un punto x e y casuale all'interno dei nuovi confini calcolati
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        
        targetPosition = new Vector2(randomX, randomY);
    }

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