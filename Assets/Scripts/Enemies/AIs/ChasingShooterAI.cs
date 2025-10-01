using UnityEngine;

[RequireComponent(typeof(EnemyStats), typeof(EnemyShooter))]
public class ChasingShooterAI : MonoBehaviour
{
    [Header("Impostazioni di Ingaggio")]
    [Tooltip("La distanza massima a cui il nemico si fermerà per iniziare a sparare.")]
    public float shootingRange = 7f;
    [Tooltip("La distanza minima che cercherà di mantenere dal giocatore. Utile per evitare che si avvicini troppo.")]
    public float stoppingDistance = 5f;

    private EnemyStats stats;
    private Transform player;
    private EnemyShooter shooter;
    private float cleanupYThreshold;

    // --- NUOVE VARIABILI PER I CONFINI ---
    private Camera mainCamera;
    private float minX, maxX, minY, maxY;
    private Vector2 spriteExtents;
    // --- FINE NUOVE VARIABILI ---

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        shooter = GetComponent<EnemyShooter>();
        // Disabilitiamo lo sparo all'inizio. Verrà attivato solo quando il nemico è in posizione.
        shooter.enabled = false; 
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;

        // --- NUOVA LOGICA: CALCOLO DINAMICO DEI CONFINI ---
        mainCamera = Camera.main;
        
        // Calcoliamo la dimensione mezza larghezza e mezza altezza dello sprite
        // per assicurarci che l'intero sprite rimanga visibile.
        spriteExtents = GetComponent<SpriteRenderer>().bounds.extents;

        // Troviamo i punti del mondo corrispondenti agli angoli dello schermo
        Vector2 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Impostiamo i confini tenendo conto della dimensione dello sprite
        minX = bottomLeft.x + spriteExtents.x;
        maxX = topRight.x - spriteExtents.x;
        minY = bottomLeft.y + spriteExtents.y;
        maxY = topRight.y - spriteExtents.y;
        // --- FINE NUOVA LOGICA ---
    }

    void Update()
    {
        // Controllo di pulizia se esce dallo schermo
        if (transform.position.y < cleanupYThreshold)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null)
        {
            // Se non c'è un giocatore, si comporta come un nemico base che scende lentamente
            transform.position += Vector3.down * (stats.moveSpeed / 2) * Time.deltaTime;
            return;
        }

        HandleMovementAndShooting();
    }

    // --- NUOVO METODO: LATEUPDATE PER IL CLAMPING ---
    // LateUpdate viene eseguito dopo tutti gli Update, quindi è il posto perfetto
    // per applicare correzioni finali alla posizione.
    void LateUpdate()
    {
        // "Blocca" la posizione del nemico all'interno dei confini calcolati
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }
    // --- FINE NUOVO METODO ---

    private void HandleMovementAndShooting()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Se il giocatore è nel raggio di tiro...
        if (distanceToPlayer <= shootingRange)
        {
            // ...attiva lo sparo...
            shooter.enabled = true;

            // ...e se è troppo vicino, si allontana lentamente per mantenere la distanza.
            if (distanceToPlayer < stoppingDistance)
            {
                Vector2 dir = (transform.position - player.position).normalized;
                transform.position += (Vector3)dir * stats.moveSpeed * 0.5f * Time.deltaTime; // Si muove a metà velocità
            }
            // Altrimenti, se è nella "fascia" ottimale, si ferma.
            else
            {
                // Potremmo aggiungere una logica di "strafing" (movimento laterale) qui in futuro.
                // Per ora, si ferma per garantire la mira.
            }
        }
        // Se il giocatore è troppo lontano...
        else
        {
            // ...disattiva lo sparo e riprende a inseguire.
            shooter.enabled = false;
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)dir * stats.moveSpeed * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats ps = collision.gameObject.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(stats.contactDamage);
            }
            // A differenza del Chaser, non si autodistrugge al contatto,
            // ma indietreggia per riprendere a sparare.
        }
    }
}