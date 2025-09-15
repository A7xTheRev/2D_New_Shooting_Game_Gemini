using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyAI : MonoBehaviour
{
    private EnemyStats stats;
    private Transform player;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)dir * stats.moveSpeed * Time.deltaTime;
        }
    }

    // Questa è la funzione che ora verrà eseguita correttamente
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats ps = collision.gameObject.GetComponent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(stats.contactDamage);
            }
            // Distrugge il nemico dopo l'impatto
            Destroy(gameObject);
        }
    }
}