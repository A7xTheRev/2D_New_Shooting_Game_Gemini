using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Impostazioni di Sparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    // public float fireRate = 3f; // RIMOSSA DA QUI

    private float fireTimer;
    private Transform playerTransform;
    private EnemyStats stats;

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

        // Imposta il timer iniziale con il valore preso da EnemyStats
        fireTimer = stats.fireRate;
    }

    void Update()
    {
        if (playerTransform == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            // Resetta il timer con il valore (potenzialmente modificato) da EnemyStats
            fireTimer = stats.fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, rotation);
        
        EnemyProjectile projectileScript = projectileInstance.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDamage(stats.projectileDamage);
        }
    }
}