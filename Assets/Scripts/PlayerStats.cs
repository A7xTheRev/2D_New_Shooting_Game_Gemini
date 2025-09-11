using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistiche base")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 10;
    public float attackSpeed = 1f;
    public float moveSpeed = 5f;

    [Header("Progressione in partita")]
    public int currentXP = 0;
    public int xpToLevelUp = 100;
    public int level = 1;

    [Header("PowerUp")]
    public float xpMultiplier = 1f;
    public int projectileCount = 1;
    public int bounceCountEnemy = 0;
    public int bounceCountWall = 0;
    public float healthRegenPerSecond = 0f;
    public float critChance = 0f;
    public float critDamageMultiplier = 2f;
    public float projectileSizeMultiplier = 1f;
    public float coinDropMultiplier = 1f;

    [Header("Coins")]
    public int sessionCoins = 0;
    public static int lastSessionCoins = 0;

    [Header("Danno e invulnerabilità")]
    public float invulnerabilityTime = 1f;
    private bool isInvulnerable = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float regenTimer;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnSessionCoinsChanged;

    void Start()
    {
        ApplyPermanentUpgrades(); // Applica i bonus permanenti

        currentHealth = maxHealth; // Imposta la vita al massimo DOPO averla aumentata
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    
        UpdateAllUI();
    }

    void ApplyPermanentUpgrades()
    {
        if (ProgressionManager.Instance == null) return;

        // Applica i bonus alle statistiche base
        maxHealth += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Health);
        damage += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Damage);
        attackSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.AttackSpeed);
        moveSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.MoveSpeed);
    
        Debug.Log("Potenziamenti permanenti applicati!");
    }
    
    void Update()
    {
        if (healthRegenPerSecond > 0 && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f)
            {
                int healthToRegen = Mathf.FloorToInt(healthRegenPerSecond);
                Heal(healthToRegen);
                regenTimer -= 1f;
            }
        }
    }

    private void UpdateAllUI()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnXPChanged?.Invoke(currentXP, xpToLevelUp);
        OnLevelUp?.Invoke(level);
        OnSessionCoinsChanged?.Invoke(sessionCoins);
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        while (elapsed < invulnerabilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            elapsed += 0.2f;
        }
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddXP(int amount)
    {
        int finalXP = Mathf.RoundToInt(amount * xpMultiplier);
        currentXP += finalXP;
        while (currentXP >= xpToLevelUp)
        {
            currentXP -= xpToLevelUp;
            LevelUp();
        }
        OnXPChanged?.Invoke(currentXP, xpToLevelUp);
        OnLevelUp?.Invoke(level);
    }

    private void LevelUp()
    {
        level++;
        xpToLevelUp = Mathf.RoundToInt(xpToLevelUp * 1.2f);
        Debug.Log("Player salito a livello: " + level);

        // MODIFICATO QUI
        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
        {
            List<PowerUp> options = manager.GetRandomPowerUps(3);
            PowerUpUI.Instance.ShowPowerUpChoices(options, this);
        }
    }

    public void CollectCoin(int amount)
    {
        sessionCoins += amount;
        OnSessionCoinsChanged?.Invoke(sessionCoins);
    }

    private void Die()
    {
        Debug.Log("Player morto");
        lastSessionCoins = sessionCoins;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }
}