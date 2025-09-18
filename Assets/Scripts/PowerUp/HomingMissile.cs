using UnityEngine;
using System.Collections;

public class HomingMissile : MonoBehaviour
{
    [Header("Statistiche Missile")]
    public float speed = 10f;
    public float rotateSpeed = 300f;
    public int baseDamage = 20;
    public float abilityPowerScaling = 1.5f;
    public string impactVFXTag = "MissileExplosion";

    [Header("Sistema di Ricerca")]
    public float detectionRadius = 20f;
    public LayerMask enemyLayer;
    public float retargetFrequency = 0.25f;

    private Transform target;
    private Rigidbody2D rb;
    private PlayerStats owner;

    public void SetOwner(PlayerStats player)
    {
        owner = player;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        StartCoroutine(UpdateTargetCoroutine());
        Destroy(gameObject, 5f);
    }

    private IEnumerator UpdateTargetCoroutine()
    {
        while (true)
        {
            FindClosestTarget();
            yield return new WaitForSeconds(retargetFrequency);
        }
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = transform.up * speed;
            return;
        }

        // --- LOGICA DI MIRA MIGLIORATA ---
        // Cerca il collider del bersaglio.
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        
        // Di default, mira al pivot.
        Vector2 aimPoint = target.position;

        // Se troviamo un collider, miriamo al suo centro.
        if (targetCollider != null)
        {
            aimPoint = targetCollider.bounds.center;
        }
        
        // Calcola la direzione verso il punto di mira corretto.
        Vector2 direction = aimPoint - rb.position;
        // --- FINE LOGICA DI MIRA ---

        direction.Normalize();

        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rb.angularVelocity = -rotateAmount * rotateSpeed;
        rb.linearVelocity = transform.up * speed;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
            return;
        }

        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null && owner != null)
            {
                int finalDamage = baseDamage + Mathf.RoundToInt(owner.abilityPower * abilityPowerScaling);
                enemy.TakeDamage(finalDamage, true);
            }

            if (!string.IsNullOrEmpty(impactVFXTag))
            {
                GameObject vfx = VFXPool.Instance.GetVFX(impactVFXTag);
                if (vfx != null)
                {
                    vfx.transform.position = transform.position;
                }
            }
            
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}