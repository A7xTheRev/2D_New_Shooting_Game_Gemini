using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Impostazioni di Sparo")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    
    [Tooltip("Quanti proiettili sparare in una singola raffica.")]
    public int projectileCount = 1;
    [Tooltip("L'angolo di dispersione se si spara più di un proiettile (es. 15 gradi).")]
    public float spreadAngle = 0f;

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

        // --- MODIFICA APPLICATA QUI ---
        // Invece di aspettare l'intero cooldown, impostiamo il timer a un valore
        // casuale tra 0.5 secondi e il cooldown massimo.
        // In questo modo il primo colpo sarà molto più veloce e meno prevedibile.
        fireTimer = Random.Range(0.5f, stats.fireRate);
        // --- FINE MODIFICA ---
    }

    void Update()
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

        if (projectileCount <= 1)
        {
            SpawnSingleProjectile(baseAngle);
        }
        else
        {
            float startAngle = baseAngle - (spreadAngle * (projectileCount - 1) / 2f);

            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + i * spreadAngle;
                SpawnSingleProjectile(currentAngle);
            }
        }
    }

    void SpawnSingleProjectile(float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, rotation);
        
        EnemyProjectile projectileScript = projectileInstance.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDamage(stats.projectileDamage);
        }
    }
}