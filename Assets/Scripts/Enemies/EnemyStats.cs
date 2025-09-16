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

    [Header("Animazione")]
    [Tooltip("Spunta questa casella se questo nemico ha un'animazione di morte configurata nell'Animator")]
    public bool hasDeathAnimation = false;

    public event Action<int, int> OnHealthChanged;
    
    private Animator animator;
    private bool isDying = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDying) return;

        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyHitSound);

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
        if (isDying) return;
        isDying = true;

        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyDeathSound);

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        int finalCoinReward = coinReward;
        if (player != null)
        {
            finalCoinReward = Mathf.RoundToInt(coinReward * player.coinDropMultiplier);
            
            AbilityController abilityController = player.GetComponent<AbilityController>();
            if (abilityController != null)
            {
                abilityController.AddChargeFromKill();
            }
        }
        
        GameManager.Instance?.EnemyDefeated(finalCoinReward, xpReward, specialCurrencyReward);
        
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        if (GetComponent<EnemyAI>() != null) GetComponent<EnemyAI>().enabled = false;
        if (GetComponent<EnemyShooter>() != null) GetComponent<EnemyShooter>().enabled = false;
        if (GetComponent<BossZigZagAI>() != null) GetComponent<BossZigZagAI>().enabled = false;
        if (GetComponent<BossRandomPatrolAI>() != null) GetComponent<BossRandomPatrolAI>().enabled = false;
        if (GetComponent<BossShooterAI>() != null) GetComponent<BossShooterAI>().enabled = false;

        if (animator != null && hasDeathAnimation)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}