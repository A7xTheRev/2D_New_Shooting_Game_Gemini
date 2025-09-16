using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    private EnemyStats stats;
    private Transform player;
    private float cleanupYThreshold; // Soglia di pulizia sotto lo schermo

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Calcola la posizione Y sotto lo schermo più un margine di sicurezza
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
    }

    void Update()
    {
        // Controllo di pulizia
        if (transform.position.y < cleanupYThreshold)
        {
            Destroy(gameObject);
            return; // Ferma l'esecuzione per questo frame
        }

        if (player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)dir * stats.moveSpeed * Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats ps = collision.gameObject.GetComponent<PlayerStats>();
            if (ps != null) ps.TakeDamage(stats.contactDamage);
            Destroy(gameObject);
        }
    }
}