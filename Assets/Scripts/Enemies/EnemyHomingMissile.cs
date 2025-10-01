using UnityEngine;
using System.Collections;

public class EnemyHomingMissile : MonoBehaviour
{
    [Header("Statistiche Missile")]
    [Tooltip("La velocità di crociera del missile.")]
    public float speed = 4f;
    [Tooltip("La velocità con cui il missile può curvare.")]
    public float rotateSpeed = 150f;
    [Tooltip("Il danno inflitto all'impatto.")]
    public int damage = 25;
    [Tooltip("Il tag del VFX da usare per l'esplosione.")]
    public string impactVFXTag = "MissileExplosion";
    
    // --- VARIABILE RINOMINATA E NUOVA VARIABILE ---
    [Tooltip("Dopo quanti secondi il missile smette di inseguire il giocatore e prosegue dritto.")]
    public float homingDuration = 5f;
    [Tooltip("Durata di vita massima totale, per sicurezza, per distruggere il missile se non incontra mai la DeathZone.")]
    public float maxLifeTime = 12f;
    // --- FINE MODIFICHE ---

    private Transform target;
    private Rigidbody2D rb;

    // --- NUOVO STATO ---
    private bool isHoming = true;
    // --- FINE NUOVO STATO ---

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // A differenza del missile del giocatore, questo cerca il player una sola volta all'inizio.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }

        // --- LOGICA MODIFICATA ---
        // Avvia il timer per smettere di inseguire
        Invoke(nameof(StopHoming), homingDuration);
        // Imposta un timer di sicurezza per la distruzione totale
        Destroy(gameObject, maxLifeTime);
        // --- FINE MODIFICA ---
    }

    void FixedUpdate()
    {
        // --- LOGICA DI INSEGUIMENTO CONDIZIONALE ---
        // Esegui la logica di rotazione e inseguimento SOLO se isHoming è true
        if (isHoming && target != null)
        {
        // Calcola la direzione verso il giocatore
        Vector2 direction = (Vector2)target.position - rb.position;
        direction.Normalize();

        // Calcola la rotazione necessaria per "curvare" verso il giocatore
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        
        // Applica la rotazione e la velocità
        rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        // --- FINE MODIFICA ---
        
        // La velocità in avanti viene applicata sempre, sia in fase di inseguimento che dopo
        rb.linearVelocity = transform.up * speed;
    }
    
    // --- NUOVO METODO ---
    private void StopHoming()
    {
        isHoming = false;
        // Interrompiamo ogni rotazione per assicurarci che vada dritto
        rb.angularVelocity = 0;
    }
    // --- FINE NUOVO METODO ---

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se tocca la DeathZone, si distrugge senza fare un'esplosione
        if (other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
            return;
        }
        
        // Ignora le collisioni con altri nemici
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        // Se colpisce il giocatore...
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }

        // In ogni caso (tranne che con altri nemici), il missile esplode all'impatto.
        Explode();
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Gestisce anche le collisioni non-trigger (es. con i muri)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }
        Explode();
    }


    void Explode()
    {
        // Ferma il movimento per evitare che l'esplosione si sposti
        if(rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        
        // Crea l'effetto visivo dell'esplosione
        if (VFXPool.Instance != null && !string.IsNullOrEmpty(impactVFXTag))
        {
            GameObject vfx = VFXPool.Instance.GetVFX(impactVFXTag);
            if (vfx != null)
            {
                vfx.transform.position = transform.position;
            }
        }
        
        // Distrugge il missile
        Destroy(gameObject);
    }
}