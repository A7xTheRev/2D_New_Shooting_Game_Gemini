using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossShooterAI : MonoBehaviour
{
    [Header("Impostazioni di Movimento")]
    public float waypointReachedThreshold = 0.1f;
    [Tooltip("Un piccolo margine dai bordi dello schermo per evitare che il boss si 'incolli' ai lati.")]
    public float patrolAreaPadding = 1f;

    [Header("Impostazioni di Sparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int projectileCount = 3;
    public float projectileSpreadAngle = 15f;

    private Vector2 targetPosition;
    private float fireTimer;
    private Transform playerTransform;
    private EnemyStats stats;
    private float cleanupYThreshold;

    // --- NUOVE VARIABILI PER L'AREA DINAMICA ---
    private float minX, maxX, minY, maxY;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        fireTimer = stats.fireRate;
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
        
        // --- NUOVA LOGICA PER CALCOLARE L'AREA ---
        Camera cam = Camera.main;
        // Calcoliamo l'area di pattugliamento (metà superiore dello schermo)
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)); 
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        minX = bottomLeft.x + patrolAreaPadding;
        maxX = topRight.x - patrolAreaPadding;
        minY = bottomLeft.y + patrolAreaPadding;
        maxY = topRight.y - patrolAreaPadding;
        // --- FINE NUOVA LOGICA ---

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
        HandleShooting();
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
        // Ora usa i nuovi limiti calcolati dinamicamente
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        targetPosition = new Vector2(randomX, randomY);
    }

    void HandleShooting()
    {
        if (playerTransform == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = stats.fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 directionToPlayer = (playerTransform.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
        float startAngle = baseAngle - (projectileSpreadAngle * (projectileCount - 1) / 2f);

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + i * projectileSpreadAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, rotation);
            
            EnemyProjectile projectileScript = projectileInstance.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.SetDamage(stats.projectileDamage);
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