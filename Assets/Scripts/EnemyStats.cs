using UnityEngine;
using System;

public class EnemyStats : MonoBehaviour
{
    [Header("Statistiche base")]
    public int maxHealth = 50;
    public int currentHealth;
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public int projectileDamage = 10;
    public float fireRate = 3f;

    [Header("Ricompense")]
    public int coinReward = 5;
    public int xpReward = 20;
    public int specialCurrencyReward = 0;

    public event Action<int, int> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        int finalCoinReward = coinReward;

        if (player != null)
        {
            finalCoinReward = Mathf.RoundToInt(coinReward * player.coinDropMultiplier);
        }
        
        GameManager.Instance?.EnemyDefeated(finalCoinReward, xpReward, specialCurrencyReward);
        
        Destroy(gameObject);
    }
}