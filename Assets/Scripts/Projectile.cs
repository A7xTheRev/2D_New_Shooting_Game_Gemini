using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Statistiche Proiettile")]
    public float speed = 10f;
    public int baseDamage = 10;         // DANNO BASE DEL PLAYER (modifica)
    public float damageMultiplier = 1f; // MOLTIPLICATORE ARMA
    public float areaDamageRadius = 0f;

    [Header("Rimbalzo")]
    public Color bounceFlashColor = Color.yellow;
    public float flashDuration = 0.1f;

    private Rigidbody2D rb;
    private int bouncesDoneEnemy = 0;
    private int bouncesDoneWall = 0;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private PlayerStats owner;

    // Confini camera
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
    }

    void OnEnable()
    {
        bouncesDoneEnemy = 0;
        bouncesDoneWall = 0;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
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
        // Usa il damage del player * moltiplicatore arma
        if (owner == null) return Mathf.RoundToInt(baseDamage * damageMultiplier);
        return Mathf.RoundToInt(owner.damage * damageMultiplier);
    }

    void UpdateCameraBounds()
    {
        if (cam == null) return;
        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight   = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        camLeft = bottomLeft.x;
        camBottom = bottomLeft.y;
        camRight = topRight.x;
        camTop = topRight.y;
    }

    void LateUpdate()
    {
        if (owner == null || owner.bounceCountWall <= 0) return;

        Vector2 pos = transform.position;
        Vector2 vel = rb.linearVelocity;
        bool bounced = false;

        // Controllo confini camera
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

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            int finalDamage = GetFinalDamage();
            enemy.TakeDamage(finalDamage);

            // Area damage
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

            // Rimbalzo verso nemici
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

    private System.Collections.IEnumerator BounceFlash()
    {
        spriteRenderer.color = bounceFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void Deactivate() => gameObject.SetActive(false);

    void OnDrawGizmosSelected()
    {
        if (areaDamageRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, areaDamageRadius);
        }
    }
}
