using UnityEngine;

public class DroneProjectile : MonoBehaviour
{
    public float speed = 15f;
    private int damage;
    private Transform target;
    private PlayerStats owner; // Riferimento al giocatore
    private int pierceCount = 1; // Quanti nemici può colpire

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
            Vector2 direction = (target.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
        }
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy"))
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