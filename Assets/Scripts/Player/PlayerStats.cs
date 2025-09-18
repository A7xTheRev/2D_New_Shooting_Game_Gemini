using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // ... (variabili esistenti) ...
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

    // --- SEZIONE MISSILI AMPLIATA ---
    [Header("Statistiche Missili a Ricerca")]
    public int homingMissileLevel = 0;
    public int homingMissileCount = 1;
    public float homingMissileCooldownMultiplier = 1f;
    
    // --- NUOVA SEZIONE PER I DRONI ---
    [Header("Statistiche Droni da Combattimento")]
    public int combatDroneLevel = 0;
    public float combatDroneFireRateMultiplier = 1f;
    public bool dronesHavePiercingShots = false;
    
    // --- NUOVA SEZIONE ---
    [Header("Potenziamenti Elementali")]
    public bool hasIncendiaryRounds = false;
    public bool hasCryoRounds = false;
    public bool hasChainLightning = false;
    // --- FINE NUOVA SEZIONE ---

    [HideInInspector]
    public List<PowerUpType> acquiredPowerUps = new List<PowerUpType>();
    
    // [Per completezza, ecco l'intero script aggiornato]
    [Header("Statistiche base")]
    public int maxHealth = 100;
    public int currentHealth;
    public int damage = 12;
    public int abilityPower = 15;
    public float attackSpeed = 1.2f;
    public float moveSpeed = 5f;

    [Header("Progressione in partita")]
    public int currentXP = 0;
    public int xpToLevelUp = 50;
    public int level = 1;
    public float levelUpPanelDelay = 0.5f;

    [Header("Coins")]
    public int sessionCoins = 0;
    public static int lastSessionCoins = 0;

    [Header("Valuta Speciale")]
    public int sessionSpecialCurrency = 0;
    public static int lastSessionSpecialCurrency = 0;

    [Header("Danno e invulnerabilità")]
    public float invulnerabilityTime = 1.5f;
    public float startBlinkInterval = 0.2f; 
    public float endBlinkInterval = 0.05f;

    [Header("Effetti Visivi")]
    public float hitShakeDuration = 0.2f;
    public float hitShakeMagnitude = 0.1f;

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
    private int enemyProjectilesLayer;
    private bool secondChanceAvailable = true;

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        enemyProjectilesLayer = LayerMask.NameToLayer("EnemyProjectile");
    }

    void Start()
    {
        ApplyPermanentUpgrades();
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.StartingPowerUp))
        {
            PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
            if (manager != null)
            {
                List<PowerUp> options = manager.GetRandomPowerUps(3, this); // Passiamo 'this'
                if (options.Count > 0)
                {
                    PowerUp startingPowerUp = options[0];
                    startingPowerUp.Apply(this);
                    acquiredPowerUps.Add(startingPowerUp.type); // Registra il potenziamento
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null) { uiManager.ShowNotification($"Starting Power-Up:\n{startingPowerUp.displayName}", 4f); }
                }
            }
        }
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) { originalColor = spriteRenderer.color; }
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
        if (playerLayer != -1 && enemyLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        if (playerLayer != -1 && enemyProjectilesLayer != -1)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, false);
    }
    
    public void TakeDamage(int amount) 
    {
        if (isInvulnerable) return;
        
        CameraShake.Instance.StartShake(hitShakeDuration, hitShakeMagnitude);
        
        AudioManager.Instance.PlaySound(AudioManager.Instance.playerHitSound);
        
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0) Die();
        else StartCoroutine(InvulnerabilityCoroutine());
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

    public void ActivateTemporaryInvulnerability(float duration)
    {
        StartCoroutine(ShieldCoroutine(duration));
    }
    
    private void ApplyPermanentUpgrades()
    {
        if (ProgressionManager.Instance == null) return;
        maxHealth += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Health);
        damage += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.Damage);
        attackSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.AttackSpeed);
        moveSpeed += ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.MoveSpeed);
        abilityPower += (int)ProgressionManager.Instance.GetTotalBonus(PermanentUpgradeType.AbilityPower);
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
        AudioManager.Instance.PlaySound(AudioManager.Instance.levelUpSound);
        StartCoroutine(ShowLevelUpPanelSequence());
    }
    
    private IEnumerator ShowLevelUpPanelSequence()
    {
        yield return new WaitForSecondsRealtime(levelUpPanelDelay);
        
        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
        {
            List<PowerUp> options = manager.GetRandomPowerUps(3, this); // Passiamo 'this'
            PowerUpUI.Instance.ShowPowerUpChoices(options, this);
        }
    }
    
    private System.Collections.IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, true);

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
        Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, false);
        isInvulnerable = false;
    }

    private System.Collections.IEnumerator ShieldCoroutine(float duration)
    {
        Debug.Log("SCUDO ATTIVATO!");
        isInvulnerable = true;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, true);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }

        yield return new WaitForSeconds(duration);

        if (this != null)
        {
            Debug.Log("SCUDO DISATTIVATO!");
            isInvulnerable = false;
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, false);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    private void Die()
    {
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.SecondChance) && secondChanceAvailable)
        {
            secondChanceAvailable = false;
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null) { uiManager.UpdateSecondChanceUI(false); }
            
            currentHealth = maxHealth / 2;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            StartCoroutine(InvulnerabilityCoroutine());
        }
        else
        {
            Debug.Log("Player morto");
        
            // --- LOGICA DI SALVATAGGIO MODIFICATA ---
            // Troviamo lo StageManager per sapere a che ondata siamo arrivati
            StageManager stageManager = FindFirstObjectByType<StageManager>();
            int currentWave = (stageManager != null) ? stageManager.stageNumber : 1;
        
            // Usiamo il nuovo metodo statico per passare tutti i dati alla schermata di Game Over
            GameOverManager.SetEndGameStats(currentWave, sessionCoins, sessionSpecialCurrency);
            // --- FINE MODIFICA ---

            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
}