using UnityEngine;
using System;

public class EnemyStats : MonoBehaviour
{
    [Header("Statistiche base")]
    public int maxHealth = 50;
    public int currentHealth;
    public float moveSpeed = 2f;
    public int contactDamage = 10;

    [Header("Ricompense")]
    public int coinReward = 5;
    public int xpReward = 20;

    // Evento per aggiornare la UI
    public event Action<int, int> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth); // notifica valori iniziali
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance?.EnemyDefeated(coinReward, xpReward);
        Destroy(gameObject);
    }

    // Collisione con il player
    void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
        }
    }

    // Se preferisci usare collider trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
        }
    }
}
