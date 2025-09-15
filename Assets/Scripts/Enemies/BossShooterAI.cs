using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossShooterAI : MonoBehaviour
{
    [Header("Area di Pattugliamento")]
    public Rect patrolArea = new Rect(-7f, 1f, 14f, 3f);

    [Header("Impostazioni di Movimento")]
    public float waypointReachedThreshold = 0.1f;

    [Header("Impostazioni di Sparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int projectileCount = 3; // Quanti proiettili sparare in una singola raffica
    public float projectileSpreadAngle = 15f; // L'angolo tra un proiettile e l'altro

    private Vector2 targetPosition;
    private float fireTimer;
    private Transform playerTransform;
    private EnemyStats stats;
    private StageManager stageManager;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        stageManager = FindFirstObjectByType<StageManager>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }

        // Imposta i timer e la prima destinazione
        fireTimer = stats.fireRate;
        SetNewRandomTarget();
    }

    void Update()
    {
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
        float randomX = Random.Range(patrolArea.x, patrolArea.x + patrolArea.width);
        float randomY = Random.Range(patrolArea.y, patrolArea.y + patrolArea.height);
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
                // Imposta il danno leggendolo da EnemyStats (che può essere stato potenziato da EliteStats)
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