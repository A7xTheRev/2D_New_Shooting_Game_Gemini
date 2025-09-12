using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Statistiche base")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 12;
    public float attackSpeed = 1.2f;
    public float moveSpeed = 5f;

    [Header("Progressione in partita")]
    public int currentXP = 0;
    public int xpToLevelUp = 50;
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

    [Header("Valuta Speciale")]
    public int sessionSpecialCurrency = 0;
    public static int lastSessionSpecialCurrency = 0;

    [Header("Danno e invulnerabilità")]
    public float invulnerabilityTime = 1.5f;

    [Header("Invulnerabilità Visiva")]
    public float startBlinkInterval = 0.2f;
    public float endBlinkInterval = 0.05f;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnSessionCoinsChanged;
    public event Action<int> OnSessionSpecialCurrencyChanged;

    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float regenTimer;
    private int playerLayer;
    private int enemyLayer;
    private bool secondChanceAvailable = true;

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer == -1) Debug.LogError("Layer 'Player' non trovato!");
        if (enemyLayer == -1) Debug.LogError("Layer 'Enemy' non trovato!");
    }

    void Start()
    {
        ApplyPermanentUpgrades();

        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(SpecialUpgradeType.StartingPowerUp))
        {
            Debug.Log("POTENZIAMENTO SPECIALE: Partenza Vantaggiosa ATTIVO!");
            PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
            if (manager != null)
            {
                List<PowerUp> options = manager.GetRandomPowerUps(1);
                if (options.Count > 0)
                {
                    PowerUp startingPowerUp = options[0];
                    startingPowerUp.Apply(this);
                    Debug.Log($"Ottenuto potenziamento iniziale: {startingPowerUp.displayName}");

                    // --- NUOVA LOGICA: INVIA LA NOTIFICA ALLA UI ---
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null)
                    {
                        uiManager.ShowNotification($"Starting PowerUP:\n{startingPowerUp.displayName}", 4f); // Mostra per 4 secondi
                    }
                }
            }
        }
        
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        UpdateAllUI();
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

    void OnDestroy()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
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

    public void CollectCoin(int amount)
    {
        sessionCoins += amount;
        OnSessionCoinsChanged?.Invoke(sessionCoins);
    }

    public void CollectSpecialCurrency(int amount)
    {
        sessionSpecialCurrency += amount;
        OnSessionSpecialCurrencyChanged?.Invoke(sessionSpecialCurrency);
    }

    private void ApplyPermanentUpgrades()
    {
        if (ProgressionManager.Instance == null) return;
        maxHealth += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Health);
        damage += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Damage);
        attackSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.AttackSpeed);
        moveSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.MoveSpeed);
        Debug.Log("Potenziamenti permanenti applicati!");
    }

    private void UpdateAllUI()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnXPChanged?.Invoke(currentXP, xpToLevelUp);
        OnLevelUp?.Invoke(level);
        OnSessionCoinsChanged?.Invoke(sessionCoins);
        OnSessionSpecialCurrencyChanged?.Invoke(sessionSpecialCurrency);
    }

    private void LevelUp()
    {
        level++;
        xpToLevelUp = Mathf.RoundToInt(xpToLevelUp * 1.2f);
        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
        {
            List<PowerUp> options = manager.GetRandomPowerUps(3);
            PowerUpUI.Instance.ShowPowerUpChoices(options, this);
        }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        float totalElapsed = 0f;
        while (totalElapsed < invulnerabilityTime)
        {
            float progress = totalElapsed / invulnerabilityTime;
            float currentBlinkInterval = Mathf.Lerp(startBlinkInterval, endBlinkInterval, progress);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
                yield return new WaitForSeconds(currentBlinkInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(currentBlinkInterval);
            }
            totalElapsed += (currentBlinkInterval * 2);
        }
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(SpecialUpgradeType.SecondChance) && secondChanceAvailable)
        {
            Debug.Log("SECONDA CHANCE ATTIVATA!");
            
            currentHealth = maxHealth / 2;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            StartCoroutine(InvulnerabilityCoroutine());
            
            secondChanceAvailable = false;

            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.UpdateSecondChanceUI(false);
            }
        }
        else
        {
            Debug.Log("Player morto, salvataggio valute di sessione.");
            lastSessionCoins = sessionCoins;
            lastSessionSpecialCurrency = sessionSpecialCurrency;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
}