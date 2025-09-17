using UnityEngine;
using System;

public class BossTurret : MonoBehaviour
{
    [Header("Statistiche Torretta")]
    public int maxHealth = 200;
    private int currentHealth;
    public bool canBeDamaged = false;

    [Header("Sistema di Fuoco")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float fireTimer;

    [Header("Riferimenti")]
    private Transform playerTransform;
    private SuperBossAI mainBossAI;

    public event Action<int, int> OnHealthChanged;

    void Awake()
    {
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainBossAI = GetComponentInParent<SuperBossAI>();
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (!canBeDamaged) return;
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null) return;
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Instantiate(projectilePrefab, firePoint.position, rotation);
    }

    // --- METODO TAKEDAMAGE MODIFICATO ---
    public void TakeDamage(int amount, bool isCrit)
    {
        if (!canBeDamaged || currentHealth <= 0) return;

        // Mostra il numero del danno, proprio come fa EnemyStats
        ShowDamageNumber(amount, isCrit);

        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- NUOVO METODO PRESO DA ENEMYSTATS ---
    private void ShowDamageNumber(int damageAmount, bool isCrit)
    {
        GameObject numberObject = VFXPool.Instance.GetVFX("DamageNumber");
        if (numberObject != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), 0.5f, 0);
            spawnPosition.z = -1f; 
            numberObject.transform.position = spawnPosition;
            
            DamageNumber dn = numberObject.GetComponent<DamageNumber>();
            if (dn != null)
            {
                dn.Show(damageAmount, isCrit);
            }
        }
    }

    void Die()
    {
        if (mainBossAI != null)
        {
            mainBossAI.TurretDestroyed();
        }
        GameObject vfx = VFXPool.Instance.GetVFX("EnemyExplosion");
        if (vfx != null)
        {
            vfx.transform.position = transform.position;
            vfx.transform.localScale = Vector3.one * 1.5f;
        }
        CameraShake.Instance.StartShake(0.2f, 0.15f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Dobbiamo disattivare il proiettile qui perché la logica di danno è stata spostata
            // direttamente nel proiettile stesso. Questo metodo ora è vuoto per evitare doppi danni.
        }
    }
}