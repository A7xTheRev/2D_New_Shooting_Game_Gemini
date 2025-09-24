using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class HealerAI : MonoBehaviour
{
    [Header("Impostazioni Cura")]
    [Tooltip("La quantità di vita curata ad ogni impulso.")]
    public int healAmount = 15;
    [Tooltip("Il raggio d'azione dell'impulso di cura.")]
    public float healRadius = 4f;
    [Tooltip("L'intervallo in secondi tra un impulso e l'altro.")]
    public float healInterval = 5f;
    [Tooltip("Il tag del VFX da usare per l'impulso di cura (deve esistere nel VFXPool).")]
    public string healVFXTag = "HealPulse";

    [Header("Impostazioni Movimento")]
    [Tooltip("Velocità con cui il nemico si sposta da un punto all'altro.")]
    public float patrolSpeed = 1f;
    [Tooltip("Margine dai bordi dello schermo.")]
    public float patrolAreaPadding = 1f;

    private float healTimer;
    private Vector2 targetPosition;
    private float minX, maxX, minY, maxY;

    void Start()
    {
        healTimer = healInterval;

        // Calcola l'area di pattugliamento (terzo superiore dello schermo)
        Camera cam = Camera.main;
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0.66f, 0)); 
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        minX = bottomLeft.x + patrolAreaPadding;
        maxX = topRight.x - patrolAreaPadding;
        minY = bottomLeft.y + patrolAreaPadding;
        maxY = topRight.y - patrolAreaPadding;
        
        SetNewRandomTarget();
    }

    void Update()
    {
        HandleMovement();
        HandleHealing();
    }

    void HandleMovement()
    {
        // Si muove verso il punto target
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);
        // Se ha raggiunto il punto, ne sceglie un altro
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);
        targetPosition = new Vector2(randomX, randomY);
    }

    void HandleHealing()
    {
        healTimer -= Time.deltaTime;
        if (healTimer <= 0f)
        {
            HealNearbyEnemies();
            healTimer = healInterval;
        }
    }

    void HealNearbyEnemies()
    {
        // Crea l'effetto visivo della cura
        if (VFXPool.Instance != null && !string.IsNullOrEmpty(healVFXTag))
        {
            GameObject vfx = VFXPool.Instance.GetVFX(healVFXTag);
            if (vfx != null)
            {
                vfx.transform.position = transform.position;

                // --- LOGICA DI SCALING AUTOMATICO ---
                // Troviamo lo Sprite Renderer nel prefab del VFX (anche se è in un figlio).
                SpriteRenderer vfxRenderer = vfx.GetComponentInChildren<SpriteRenderer>();

                // Controlliamo di aver trovato tutto il necessario per evitare errori.
                if (vfxRenderer != null && vfxRenderer.sprite != null)
                {
                    // 1. Leggiamo la larghezza di base dello sprite in Unità.
                    float baseSpriteDiameter = vfxRenderer.sprite.bounds.size.x;

                    // 2. Calcoliamo il diametro desiderato basato sul raggio di cura.
                    float desiredDiameter = healRadius * 2;

                    // 3. Calcoliamo il fattore di scala necessario.
                    // Se la dimensione base è 0, evitiamo una divisione per zero.
                    if (baseSpriteDiameter > 0)
                    {
                        float finalScale = desiredDiameter / baseSpriteDiameter;
                        vfx.transform.localScale = new Vector3(finalScale, finalScale, 1f);
                    }
                }
                // --- FINE LOGICA DI SCALING ---
            }
        }
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, healRadius);
        foreach (Collider2D hit in colliders)
        {
            // Escludi te stesso dalla cura
            if (hit.gameObject == this.gameObject) continue;

            EnemyStats enemyToHeal = hit.GetComponent<EnemyStats>();
            if (enemyToHeal != null)
            {
                enemyToHeal.Heal(healAmount);
            }
        }
    }

    // Disegna un cerchio nell'editor per visualizzare il raggio di cura
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}