using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Necessario se useremo un pool per gli effetti

public class Projectile : MonoBehaviour
{
    [Header("Statistiche Proiettile")]
    public float speed = 10f;
    public int baseDamage = 10;
    public float damageMultiplier = 1f;
    public float areaDamageRadius = 0f;
    public string weaponType;

    // --- NUOVA VARIABILE ---
    [Header("Effetto Impatto")]
    public GameObject impactVFXPrefab; // Prefab dell'effetto visivo da istanziare all'impatto

    [Header("Gestione Vita")]
    public float lifeTime = 3f;

    [Header("Rimbalzo")]
    public Color bounceFlashColor = Color.yellow;
    public float flashDuration = 0.1f;

    private float baseSpeed;
    private Rigidbody2D rb;
    private int bouncesDoneEnemy = 0;
    private int bouncesDoneWall = 0;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private PlayerStats owner;
    private Camera cam;
    private float camLeft, camRight, camTop, camBottom;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        cam = Camera.main;
        UpdateCameraBounds();
        baseSpeed = speed;
    }

    void OnEnable()
    {
        bouncesDoneEnemy = 0;
        bouncesDoneWall = 0;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        speed = baseSpeed;
        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            int finalDamage = GetFinalDamage();
            enemy.TakeDamage(finalDamage);

            // --- NUOVA LOGICA: Attiva l'effetto visivo qui ---
            SpawnImpactVFX(transform.position);
            // --- FINE NUOVA LOGICA ---

            if (areaDamageRadius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);
                foreach (var hit in hits)
                {
                    EnemyStats e = hit.GetComponent<EnemyStats>();
                    if (e != null && e != enemy)
                        e.TakeDamage(finalDamage);
                }
            }

            if (owner != null && bouncesDoneEnemy < owner.bounceCountEnemy)
            {
                bouncesDoneEnemy++;
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 5f);
                bool bounced = false;

                foreach (var hit in enemies)
                {
                    EnemyStats e = hit.GetComponent<EnemyStats>();
                    if (e != null && e != enemy)
                    {
                        Vector2 dir = (e.transform.position - transform.position).normalized;

                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                        transform.rotation = Quaternion.Euler(0, 0, angle);
                        
                        Launch(dir);
                        bounced = true;

                        if (spriteRenderer != null)
                            StartCoroutine(BounceFlash());

                        break;
                    }
                }

                if (!bounced) Deactivate();
            }
            else Deactivate();
        }
    }

    void LateUpdate()
    {
        if (owner == null || owner.bounceCountWall <= 0) return;

        Vector2 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        bool bounced = false;

        if (pos.x <= camLeft) { vel.x = Mathf.Abs(vel.x); bounced = true; }
        else if (pos.x >= camRight) { vel.x = -Mathf.Abs(vel.x); bounced = true; }

        if (pos.y <= camBottom) { vel.y = Mathf.Abs(vel.y); bounced = true; }
        else if (pos.y >= camTop) { vel.y = -Mathf.Abs(vel.y); bounced = true; }

        if (bounced)
        {
            bouncesDoneWall++;
            rb.linearVelocity = vel;
            if (spriteRenderer != null)
                StartCoroutine(BounceFlash());
            if (bouncesDoneWall > owner.bounceCountWall)
                Deactivate();
        }
    }
    
    public void SetOwner(PlayerStats player)
    {
        owner = player;
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private int GetFinalDamage()
    {
        if (owner == null) return Mathf.RoundToInt(baseDamage * damageMultiplier);

        int finalDamage = Mathf.RoundToInt(owner.damage * damageMultiplier);

        // --- NUOVA LOGICA PER IL CRITICO ---
        // Controlla se il colpo è critico
        if (Random.value < owner.critChance) // Random.value è un numero casuale tra 0.0 e 1.0
        {
            Debug.Log("COLPO CRITICO!");
            finalDamage = Mathf.RoundToInt(finalDamage * owner.critDamageMultiplier);
        }

        return finalDamage;
    }

    void Deactivate()
    {
        CancelInvoke(nameof(Deactivate));
        if (ProjectilePool.Instance != null && !string.IsNullOrEmpty(weaponType))
        {
            ProjectilePool.Instance.ReturnProjectile(weaponType, gameObject);
        }
        else
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator BounceFlash()
    {
        spriteRenderer.color = bounceFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // --- NUOVO METODO: Per attivare l'effetto visivo ---
    void SpawnImpactVFX(Vector3 position)
    {
        if (impactVFXPrefab != null)
        {
            // Per ora, lo istanziamo direttamente.
            // In futuro, per ottimizzare, potremmo usare un Object Pool per gli effetti VFX.
            GameObject vfx = Instantiate(impactVFXPrefab, position, Quaternion.identity);
            vfx.SetActive(true); // Assicurati che sia attivo
            // Lo script VFX_LifeCycle sul prefab si occuperà di disattivarlo.
        }
    }
    // --- FINE NUOVO METODO ---

    void OnDrawGizmosSelected()
    {
        if (areaDamageRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, areaDamageRadius);
        }
    }

    void UpdateCameraBounds()
    {
        if (cam == null) return;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        camLeft = bottomLeft.x;
        camBottom = bottomLeft.y;
        camRight = topRight.x;
        camTop = topRight.y;
    }
}