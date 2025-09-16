using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyStats))]
public class KamikazeAI : MonoBehaviour
{
    private enum State
    {
        Descending,
        LockingOn,
        Charging
    }

    [Header("Impostazioni Kamikaze")]
    public float descendYPosition = 4.5f;
    public float lockOnDelay = 0.5f;
    public Color chargeSignalColor = Color.red;

    private State currentState;
    private Vector2 chargeDirection; // MODIFICATO: Ora salviamo la direzione, non la posizione
    private EnemyStats stats;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float cleanupYThreshold;
    private Transform playerTransform;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Start()
    {
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        currentState = State.Descending;
    }

    void Update()
    {
        if (transform.position.y < cleanupYThreshold)
        {
            Destroy(gameObject);
            return;
        }

        switch (currentState)
        {
            case State.Descending:
                HandleDescending();
                break;
            case State.Charging:
                HandleCharging();
                break;
        }
    }

    private void HandleDescending()
    {
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, descendYPosition), stats.moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.y - descendYPosition) < 0.01f)
        {
            currentState = State.LockingOn;
            if (playerTransform != null)
            {
                StartCoroutine(ChargeSequence());
            }
        }
    }

    // --- LOGICA DI CARICA MODIFICATA ---
    private void HandleCharging()
    {
        // Muoviti costantemente nella direzione salvata, senza mai fermarti.
        transform.position += (Vector3)chargeDirection * stats.moveSpeed * Time.deltaTime;
    }
    // --- FINE MODIFICA ---

    private IEnumerator ChargeSequence()
    {
        yield return new WaitForSeconds(lockOnDelay);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = chargeSignalColor;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
        }

        // "Fotografa" la DIREZIONE del giocatore e inizia la carica
        if(playerTransform != null)
        {
            // Calcoliamo il vettore direzione e lo "normalizziamo" (lo rendiamo di lunghezza 1)
            chargeDirection = (playerTransform.position - transform.position).normalized;
        }
        else
        {
            // Se non trova il player, carica semplicemente verso il basso
            chargeDirection = Vector2.down;
        }
        currentState = State.Charging;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(stats.contactDamage);
            }
            stats.Die();
        }
    }
}