using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    // --- MODIFICA: Le statistiche ora sono nascoste dall'inspector ---
    // Vengono impostate al momento dello sparo dal PlayerController, leggendole dal WeaponData.
    [HideInInspector] public float speed = 10f;
    [HideInInspector] public float damageMultiplier = 1f;
    [HideInInspector] public float areaDamageRadius = 0f;
    [HideInInspector] public int pierceCount = 0;
    // La variabile "baseDamage" è stata rimossa perché non era utilizzata.
    // --- FINE MODIFICA ---

    [Header("Gestione Vita")]
    public float lifeTime = 5f;

    [Header("Rimbalzo")]
    public Color bounceFlashColor = Color.yellow;
    public float flashDuration = 0.1f;

    // Riferimenti pubblici ancora necessari
    public string weaponType;
    public string impactVFXTag;

    private int currentPierceLeft;
    private float baseSpeed;
    private Rigidbody2D rb;
    private int bouncesDoneEnemy = 0;
    private int bouncesDoneWall = 0;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private PlayerStats owner;
    private Camera cam;
    private float camLeftEdge, camRightEdge, camTopEdge, camBottomEdge;
    private float spriteWidth, spriteHeight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        cam = Camera.main;
        baseSpeed = speed;
        if (GetComponent<SpriteRenderer>() != null)
        {
            spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2;
            spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y / 2;
        }
    }

    void OnEnable()
    {
        bouncesDoneEnemy = 0;
        bouncesDoneWall = 0;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        speed = baseSpeed;
        
        // Resetta il contatore di perforazione all'attivazione
        currentPierceLeft = pierceCount;
        UpdateCameraBounds();
        CancelInvoke(nameof(Deactivate));
    }
    
    // Questo metodo viene chiamato dal PlayerController DOPO aver impostato tutte le statistiche.
    public void Activate()
    {
        // --- MODIFICA CHIAVE QUI ---
        // Ora inizializziamo il contatore e il timer qui, DOPO che le statistiche sono state impostate.
        currentPierceLeft = pierceCount;
        Invoke(nameof(Deactivate), lifeTime);
    }

    void FixedUpdate()
    {
        HandleWallBounce();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Deactivate();
            return;
        }

    // --- LOGICA SEMPLIFICATA ---
    // Non ci serve più un controllo speciale per le torrette!
    // Cerchiamo semplicemente un componente EnemyStats.
        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
        // Se lo script è disabilitato (es. boss in fase 1), non fare nulla
            if (!enemy.enabled) return;
        
        bool isCrit;
            int finalDamage = GetFinalDamageWithCrit(out isCrit);

            enemy.TakeDamage(finalDamage, isCrit);
            SpawnImpactVFX(other.transform.position);

            if (owner != null)
            {
                if (owner.hasIncendiaryRounds)
                {
                    enemy.ApplyBurn(5f);
                }
                // In futuro qui aggiungeremo gli altri effetti
                // if (owner.hasCryoRounds) { enemy.ApplySlow(...); }
                // if (owner.hasChainLightning) { enemy.ApplyChainLightning(...); }
            }
            // --- FINE NUOVA LOGICA ---

            if (areaDamageRadius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);
                foreach (var hit in hits)
                {
                    EnemyStats e = hit.GetComponentInParent<EnemyStats>();
                    if (e != null && e != enemy && e.enabled)
                        e.TakeDamage(finalDamage, isCrit);
                }
            }

            // --- NUOVA LOGICA DI PRIORITÀ: PERFORAZIONE > RIMBALZO > DISTRUZIONE ---
            if (currentPierceLeft > 0)
            {
                // Se il proiettile può ancora perforare, riduce il contatore e continua la sua corsa.
                currentPierceLeft--;
            }
            else if (owner != null && bouncesDoneEnemy < owner.bounceCountEnemy)
            {
                // Se non può perforare, controlla se può rimbalzare.
                bouncesDoneEnemy++;
                BounceToNextEnemy(enemy.transform);
            }
            else
            {
                // Se non può né perforare né rimbalzare, si disattiva.
                Deactivate();
            }
            // --- FINE NUOVA LOGICA ---
        }
    }

    private void SpawnImpactVFX(Vector3 position)
    {
        if (!string.IsNullOrEmpty(impactVFXTag))
        {
            GameObject vfx = VFXPool.Instance.GetVFX(impactVFXTag);
            if (vfx != null)
            {
                vfx.transform.position = position;
            }
        }
    }

    private int GetFinalDamageWithCrit(out bool isCrit)
    {
        isCrit = false;
        if (owner == null) return 0; // Modificato per non usare più baseDamage
        
        int finalDamage = Mathf.RoundToInt(owner.damage * damageMultiplier);
        if (Random.value < owner.critChance)
        {
            isCrit = true;
            finalDamage = Mathf.RoundToInt(finalDamage * owner.critDamageMultiplier);
        }
        return finalDamage;
    }

    private void BounceToNextEnemy(Transform currentEnemy)
    {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 5f);
                bool bounced = false;
                foreach (var hit in enemies)
                {
            if (hit.transform != currentEnemy)
            {
                    EnemyStats e = hit.GetComponentInParent<EnemyStats>();
                if (e != null)
                    {
                        Vector2 dir = (e.transform.position - transform.position).normalized;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                        transform.rotation = Quaternion.Euler(0, 0, angle);
                        Launch(dir);
                        bounced = true;
                        if (spriteRenderer != null) StartCoroutine(BounceFlash());
                        break;
                    }
                }
        }
        if (!bounced) Deactivate();
    }

    private void HandleWallBounce()
    {
        if (owner == null || owner.bounceCountWall <= 0 || bouncesDoneWall >= owner.bounceCountWall) return;
        Vector2 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        bool bounced = false;
        if ((pos.x - spriteWidth < camLeftEdge && vel.x < 0) || (pos.x + spriteWidth > camRightEdge && vel.x > 0))
        {
            vel.x *= -1;
            bounced = true;
        }
        if (bounced)
        {
            bouncesDoneWall++;
            rb.linearVelocity = vel;
            if (spriteRenderer != null) StartCoroutine(BounceFlash());
        }
    }
    
    public void SetOwner(PlayerStats player) { owner = player; }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
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
        }
    }
    
    private IEnumerator BounceFlash()
    {
        spriteRenderer.color = bounceFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        if (areaDamageRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, areaDamageRadius);
        }
    }

    private void UpdateCameraBounds()
    {
        if (cam == null) return;
        camLeftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        camRightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        camBottomEdge = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        camTopEdge = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
    }
}