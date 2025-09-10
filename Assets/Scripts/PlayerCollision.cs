using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
        if (player == null)
        {
            Debug.LogError("PlayerCollision: Nessun PlayerStats trovato sul GameObject!");
        }
    }

    // Metodo chiamato quando un Collider2D entra in contatto
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Controlla se il nemico ha lo script EnemyStats
        EnemyStats enemy = collision.gameObject.GetComponent<EnemyStats>();
        if (enemy != null && player != null)
        {
            player.TakeDamage(enemy.contactDamage);
        }
    }

    // Se preferisci usare trigger (Collider2D isTrigger = true)
    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyStats enemy = other.GetComponent<EnemyStats>();
        if (enemy != null && player != null)
        {
            player.TakeDamage(enemy.contactDamage);
        }
    }
}
