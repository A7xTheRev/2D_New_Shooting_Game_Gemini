using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("La 'scheda' con tutte le statistiche di base del giocatore.")]
    public PlayerData playerData;

    // --- STATISTICHE DI BASE (ora lette dal PlayerData) ---
    // Le teniamo pubbliche ma nascoste dall'inspector, perché rappresentano lo stato ATTUALE in partita
    [HideInInspector] public int maxHealth;
    [HideInInspector] public int damage;
    [HideInInspector] public int abilityPower;
    [HideInInspector] public float attackSpeed;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float critChance;
    [HideInInspector] public float critDamageMultiplier;
    [HideInInspector] public float projectileSizeMultiplier;
    [HideInInspector] public float invulnerabilityTime;
    [HideInInspector] public float startBlinkInterval;
    [HideInInspector] public float endBlinkInterval;
    [HideInInspector] public float hitShakeDuration;
    [HideInInspector] public float hitShakeMagnitude;

    // --- STATO ATTUALE IN PARTITA ---
    public int currentHealth;
    public int currentXP = 0;
    public int xpToLevelUp = 50;
    public int level = 1;
    public float levelUpPanelDelay = 0.5f;
    public int sessionCoins = 0;
    public int sessionSpecialCurrency = 0;

    [Header("PowerUp Accumulati")]
    public float xpMultiplier = 1f;
    public int projectileCount = 1;
    public int bounceCountEnemy = 0;
    public int bounceCountWall = 0;
    public float healthRegenPerSecond = 0f;
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

    [HideInInspector] public List<PowerUpType> acquiredPowerUps = new List<PowerUpType>();

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
        // Carica tutte le statistiche di base dalla "scheda"
        if (playerData == null)
        {
            Debug.LogError("ATTENZIONE: Nessun PlayerData assegnato al PlayerStats!");
            return;
        }
        LoadStatsFromData();

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        enemyProjectilesLayer = LayerMask.NameToLayer("EnemyProjectile");
    }

    void LoadStatsFromData()
    {
        maxHealth = playerData.maxHealth;
        damage = playerData.damage;
        abilityPower = playerData.abilityPower;
        attackSpeed = playerData.attackSpeed;
        moveSpeed = playerData.moveSpeed;
        critChance = playerData.critChance;
        critDamageMultiplier = playerData.critDamageMultiplier;
        projectileSizeMultiplier = playerData.projectileSizeMultiplier;
        invulnerabilityTime = playerData.invulnerabilityTime;
        startBlinkInterval = playerData.startBlinkInterval;
        endBlinkInterval = playerData.endBlinkInterval;
        hitShakeDuration = playerData.hitShakeDuration;
        hitShakeMagnitude = playerData.hitShakeMagnitude;
    }

    void Start()
    {
        ApplyPermanentUpgrades();
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.StartingPowerUp))
        {
            PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
            if (manager != null)
            {
                List<PowerUp> options = manager.GetRandomPowerUps(3, this);
                if (options.Count > 0)
                {
                    PowerUp startingPowerUp = options[0];
                    startingPowerUp.Apply(this);
                    acquiredPowerUps.Add(startingPowerUp.type);
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null) { uiManager.ShowNotification($"Starting Power-Up:\n{startingPowerUp.displayName}", 4f); }
                }
            }
        }
        currentHealth = maxHealth;

        // --- MODIFICA APPLICATA QUI ---
        // Cerca lo SpriteRenderer anche negli oggetti figli.
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // --- FINE MODIFICA ---

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
        ProgressionManager.Instance?.AddCoinsCollected(amount);
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

            // --- RIGA AGGIUNTA QUI ---
            // Assicuriamoci che il tempo torni normale prima di cambiare scena.
            Time.timeScale = 1f;

            StageManager stageManager = FindFirstObjectByType<StageManager>();
            int currentWave = (stageManager != null) ? stageManager.stageNumber : 1;
            float timeSurvived = (stageManager != null) ? stageManager.GetSurvivalTime() : 0f;

            // Passiamo anche il tempo di sopravvivenza
            GameOverManager.SetEndGameStats(currentWave, sessionCoins, sessionSpecialCurrency, timeSurvived);

            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }

    public void InitializeFromData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogError("Tentativo di inizializzare PlayerStats con dati nulli!");
            return;
        }
        playerData = data;
        LoadStatsFromData();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Controlla se l'oggetto con cui abbiamo colliso ha un componente Pickup
        Pickup pickup = other.GetComponent<Pickup>();
        if (pickup != null)
        {
            // Controlla il tipo di pickup e agisci di conseguenza
            switch (pickup.type)
            {
                case Pickup.PickupType.Coin:
                    CollectCoin(pickup.value);
                    break;
                case Pickup.PickupType.Gem:
                    CollectSpecialCurrency(pickup.value);
                    break;
                case Pickup.PickupType.Health:
                    Heal(pickup.value);
                    break;
            }

            // "Consuma" l'oggetto
            pickup.Collect();
        }
    }
}