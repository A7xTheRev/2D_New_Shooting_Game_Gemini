using UnityEngine;

public class DroneProjectile : MonoBehaviour
{
    public float speed = 15f;
    private int damage;
    private Transform target;
    private PlayerStats owner;
    private int pierceCount = 1;

    // Metodo di inizializzazione migliorato
    public void Initialize(int amount, Transform newTarget, PlayerStats ownerStats)
    {
        damage = amount;
        target = newTarget;
        owner = ownerStats;

        // Se il giocatore ha il potenziamento, aumenta il numero di perforazioni
        if (owner != null && owner.dronesHavePiercingShots)
        {
            pierceCount = 3; // Può colpire 3 nemici prima di sparire
        }
    }

    void Start()
    {
        if (target != null)
        {
            // --- NUOVA LOGICA DI MIRA ---
            // Per prima cosa, cerchiamo il collider del nostro bersaglio.
            Collider2D targetCollider = target.GetComponent<Collider2D>();
            
            // Di default, miriamo alla posizione del pivot del bersaglio.
            Vector2 aimPoint = target.position;

            // Ma se il bersaglio ha un collider, miriamo al centro di quel collider!
            if (targetCollider != null)
            {
                aimPoint = targetCollider.bounds.center;
            }
            // --- FINE NUOVA LOGICA ---

            // Ora calcoliamo la direzione verso il punto di mira corretto.
            Vector2 direction = (aimPoint - (Vector2)transform.position).normalized;
            GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        }
        
        // Aggiungiamo una leggera rotazione per far puntare lo sprite nella direzione di volo
        if (GetComponent<Rigidbody2D>().linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(GetComponent<Rigidbody2D>().linearVelocity.y, GetComponent<Rigidbody2D>().linearVelocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
            return;
        }

        // --- CORREZIONE APPLICATA QUI ---
        // Ora il proiettile reagisce sia ai nemici normali che ai boss.
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, false);
                
                // --- LOGICA DI PERFORAZIONE ---
                pierceCount--; // Riduci il conteggio ad ogni colpo
                if (pierceCount <= 0)
        {
                    Destroy(gameObject); // Distruggi solo quando ha finito i colpi
                }
            }
        }
    }
}