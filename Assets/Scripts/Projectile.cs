using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private bool hasChained = false; // NUOVA VARIABILE: tiene traccia se questo proiettile ha già scatenato un fulmine

    // --- NUOVO: Variabile per memorizzare il danno del proiettile ---
    private int currentDamage;
    // --- FINE NUOVO ---

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

        currentPierceLeft = pierceCount;
        UpdateCameraBounds();
        CancelInvoke(nameof(Deactivate));
        hasChained = false;
    }

    // Questo metodo viene chiamato dal PlayerController DOPO aver impostato tutte le statistiche.
    public void Activate()
    {
        // --- MODIFICA CHIAVE QUI ---
        // Ora inizializziamo il contatore e il timer qui, DOPO che le statistiche sono state impostate.
        currentPierceLeft = pierceCount;
        Invoke(nameof(Deactivate), lifeTime);
        
        // --- NUOVO: Imposta il danno iniziale del proiettile al momento dell'attivazione ---
        if (owner != null)
        {
            this.currentDamage = Mathf.RoundToInt(owner.GetCurrentDamage() * this.damageMultiplier);
        }
        else
        {
            this.currentDamage = 0;
        }
        // --- FINE NUOVO ---
    }

    void FixedUpdate()
    {
        HandleWallBounce();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("DeathZone"))
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
                // Effetto Incendiario
                if (owner.hasIncendiaryRounds)
                {
                    int burnDamage = Mathf.RoundToInt(owner.abilityPower * owner.burnDamageMultiplier);
                    if (burnDamage > 0) enemy.ApplyBurn(owner.burnDuration, burnDamage);
                }

                // Effetto Congelante
                if (owner.hasCryoRounds)
                {
                    // Ora legge i valori personalizzati dal PlayerStats
                    enemy.ApplySlow(owner.cryoSlowMultiplier, owner.cryoSlowDuration); 
                }
            }
            // --- LOGICA CHAIN LIGHTNING CORRETTA ---
            // Si attiva solo se ha il potenziamento E se non lo ha già fatto in questa vita
            if (owner != null && owner.hasChainLightning && !hasChained)
            {
                int lightningBaseDamage = Mathf.RoundToInt(owner.abilityPower * owner.initialChainDamageMultiplier);
                if (lightningBaseDamage > 0)
                {
                    HandleChainLightning(enemy.transform, lightningBaseDamage);
                    hasChained = true; // Marchia questo proiettile, non scatenerà più fulmini
                }
            }

            if (areaDamageRadius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);
                foreach (var hit in hits)
                {
                    EnemyStats e = hit.GetComponentInParent<EnemyStats>();
                    if (e != null && e != enemy && e.enabled) e.TakeDamage(finalDamage, isCrit);
                }
            }

            if (currentPierceLeft > 0)
            {
                currentPierceLeft--;
            }
            else if (owner != null && bouncesDoneEnemy < owner.bounceCountEnemy)
            {
                bouncesDoneEnemy++;
                // --- NUOVO: Riduci il danno prima del prossimo rimbalzo ---
                this.currentDamage = Mathf.RoundToInt(this.currentDamage * owner.bounceDamageMultiplier);
                // --- FINE NUOVO ---
                BounceToNextEnemy(enemy.transform);
            }
            else
            {
                Deactivate();
            }
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

        // --- MODIFICA: Usa 'currentDamage' come base invece di ricalcolarlo da owner.damage ---
        int finalDamage = this.currentDamage;
        // --- FINE MODIFICA ---

        if (UnityEngine.Random.value < owner.critChance)
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

    public void Deactivate()
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
        if(spriteRenderer != null)
        {
        spriteRenderer.color = originalColor;
        }
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
    
    // --- METODO CHAIN LIGHTNING COMPLETAMENTE RISCRITTO ---
    private void HandleChainLightning(Transform initialTarget, int initialDamage)
    {
        // --- CORREZIONE ERRORE: ORA USA IL PREFAB PASSATO DAL GIOCATORE ---
        if (owner == null || owner.chainLightningVFXPrefab == null)
        {
            Debug.LogError("Prefab del Chain Lightning non assegnato al PlayerStats!");
            return;
        }
        // --- FINE CORREZIONE ---

        List<Transform> hitTargets = new List<Transform> { initialTarget };
        Transform currentTarget = initialTarget;
        int currentDamage = initialDamage;

        // Esegui il ciclo per ogni "rimbalzo" del fulmine
        for (int i = 0; i < owner.chainCount; i++)
        {
            // ... (la logica interna per trovare il prossimo bersaglio rimane invariata) ...
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(currentTarget.position, 7f);
            Transform nextTarget = null;
            float minDistance = Mathf.Infinity;

            // Trova il prossimo bersaglio più vicino che non sia già stato colpito
            foreach (Collider2D col in nearbyEnemies)
            {
                if (!hitTargets.Contains(col.transform) && col.CompareTag("Enemy"))
                {
                    float distance = Vector2.Distance(currentTarget.position, col.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextTarget = col.transform;
                    }
                }
            }

            // Se abbiamo trovato un nuovo bersaglio...
            if (nextTarget != null)
            {
                // --- CORREZIONE ERRORE: Usa il prefab corretto ---
                GameObject lightningObj = Instantiate(owner.chainLightningVFXPrefab);
                
                ChainLightningVFX vfxScript = lightningObj.GetComponent<ChainLightningVFX>();
                if (vfxScript != null)
                {
                vfxScript.Setup(currentTarget.position, nextTarget.position);
                }

                // Applica il danno ridotto
                currentDamage = Mathf.RoundToInt(currentDamage * owner.chainDamageMultiplier);
                if (currentDamage <= 0) break;

                EnemyStats targetStats = nextTarget.GetComponent<EnemyStats>();
                if(targetStats != null) targetStats.TakeDamage(currentDamage, false);

                // Aggiorna la lista e preparati al prossimo "salto"
                hitTargets.Add(nextTarget);
                currentTarget = nextTarget;
            }
            else
            {
                // Se non ci sono più bersagli, interrompi il ciclo
                break; 
            }
        }
    }
}