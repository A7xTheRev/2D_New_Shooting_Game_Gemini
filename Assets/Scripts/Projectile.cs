using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("Statistiche Proiettile")]
    public float speed = 10f;
    public int baseDamage = 10;
    public float damageMultiplier = 1f;
    public float areaDamageRadius = 0f;
    public string weaponType;

    [Header("Effetto Impatto")]
    public GameObject impactVFXPrefab;

    [Header("Gestione Vita")]
    public float lifeTime = 5f;

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

        // Calcola le dimensioni dello sprite per un rimbalzo più preciso
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
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        speed = baseSpeed;
        
        UpdateCameraBounds();

        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifeTime);
    }

    void FixedUpdate()
    {
        // La fisica va gestita in FixedUpdate
        HandleWallBounce();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Deactivate();
            return;
        }

        EnemyStats enemy = other.GetComponentInParent<EnemyStats>();
        if (enemy != null)
        {
            int finalDamage = GetFinalDamage();
            enemy.TakeDamage(finalDamage);
            SpawnImpactVFX(transform.position);

            if (areaDamageRadius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);
                foreach (var hit in hits)
                {
                    EnemyStats e = hit.GetComponentInParent<EnemyStats>();
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
                    EnemyStats e = hit.GetComponentInParent<EnemyStats>();
                    if (e != null && e != enemy)
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
                if (!bounced) Deactivate();
            }
            else Deactivate();
        }
    }

    private void HandleWallBounce()
    {
        // Funziona solo se il potenziamento è attivo (owner.bounceCountWall > 0)
        if (owner == null || owner.bounceCountWall <= 0 || bouncesDoneWall >= owner.bounceCountWall) return;

        Vector2 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        bool bounced = false;

        // Controlla solo i bordi laterali
        if ((pos.x - spriteWidth < camLeftEdge && vel.x < 0) || (pos.x + spriteWidth > camRightEdge && vel.x > 0))
        {
            vel.x *= -1; // Inverte solo la velocità orizzontale
            bounced = true;
        }
        
        // Le righe per il controllo di top e bottom sono state rimosse

        if (bounced)
        {
            bouncesDoneWall++;
            rb.linearVelocity = vel;
            if (spriteRenderer != null) StartCoroutine(BounceFlash());
            // Non usiamo più Deactivate qui perché il timer di vita gestirà la distruzione
        }
    }
    
    public void SetOwner(PlayerStats player) { owner = player; }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }
    // (Tutti gli altri metodi come Deactivate, GetFinalDamage, etc. rimangono qui)
    
    private int GetFinalDamage()
    {
        if (owner == null) return baseDamage;
        int finalDamage = Mathf.RoundToInt(owner.damage * damageMultiplier);
        if (Random.value < owner.critChance)
        {
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
            if(gameObject.activeInHierarchy)
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

    void SpawnImpactVFX(Vector3 position)
    {
        if (impactVFXPrefab != null)
        {
            GameObject vfx = Instantiate(impactVFXPrefab, position, Quaternion.identity);
            vfx.SetActive(true);
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
}