using UnityEngine;

public class CombatDrone : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    private PlayerStats ownerStats;
    private Transform target;

    [Header("Statistiche di Fuoco")]
    public float fireRate = 2f;
    public float baseDamage = 5;
    public float abilityPowerScaling = 0.5f;
    private float fireTimer;

    [Header("Sistema di Ricerca")]
    public float detectionRadius = 15f;
    public float retargetFrequency = 0.5f;
    public LayerMask enemyLayer;

    // La posizione che il drone cercherà di mantenere rispetto al giocatore
    private Vector2 targetPosition;

    public void Initialize(PlayerStats owner, Vector2 offset)
    {
        ownerStats = owner;
        targetPosition = (Vector2)owner.transform.position + offset;
        InvokeRepeating(nameof(FindClosestTarget), 0, retargetFrequency);
    }

    void Update()
    {
        if (ownerStats == null)
        {
            Destroy(gameObject); // Se il giocatore muore, il drone si distrugge
            return;
        }

        // Segui fluidamente la posizione target
        transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        
        // Gestione del fuoco
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0 && target != null)
        {
            Shoot();
            // --- MODIFICA APPLICATA QUI ---
            // Ora il cooldown è influenzato dal moltiplicatore del giocatore
            fireTimer = (1f / fireRate) * ownerStats.combatDroneFireRateMultiplier;
        }
    }

    void FindClosestTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = enemy.transform;
                }
            }
        }
        target = closestTarget;
    }

    void Shoot()
    {
        if (projectilePrefab == null || target == null) return;

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        DroneProjectile projectile = projObj.GetComponent<DroneProjectile>();

        if (projectile != null)
        {
            // Calcola il danno finale
            int finalDamage = Mathf.RoundToInt(baseDamage + (ownerStats.abilityPower * abilityPowerScaling));
            
            // Passa anche il riferimento a PlayerStats al proiettile
            projectile.Initialize(finalDamage, target, ownerStats);
        }
    }

    // Metodo pubblico per aggiornare la posizione desiderata (per quando avremo più droni)
    public void SetTargetPosition(Vector2 newPosition)
    {
        targetPosition = newPosition;
    }
}