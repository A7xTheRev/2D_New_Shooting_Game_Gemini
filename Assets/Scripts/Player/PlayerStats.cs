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

    // --- VARIABILE PER GLI OBIETTIVI ---
    [HideInInspector]
    public bool tookDamageThisRun = false;

    // --- STATO ATTUALE IN PARTITA ---
    public int currentHealth;
    public int currentXP = 0;
    public int xpToLevelUp = 50;
    public int level = 1;
    public float levelUpPanelDelay = 0.5f;
    public int sessionCoins = 0;
    public int sessionSpecialCurrency = 0;

    // --- NUOVO: Variabili per i moltiplicatori di danno ---
    [HideInInspector] public float globalDamageMultiplier;
    [HideInInspector] public float bounceDamageMultiplier;
    // --- FINE NUOVO ---

    [HideInInspector]
    public bool isWeaponDisabled = false;

    // --- NUOVE VARIABILI PER LA GESTIONE DEI LEVEL UP ---
    private int pendingLevelUps = 0;
    private bool isShowingPowerUpPanel = false;

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

    [HideInInspector] public float burnDuration;
    [HideInInspector] public float burnDamageMultiplier;
    [HideInInspector] public float cryoSlowDuration;
    [HideInInspector] public float cryoSlowMultiplier;
    [HideInInspector] public int chainCount;
    [HideInInspector] public float initialChainDamageMultiplier;
    [HideInInspector] public float chainDamageMultiplier;
    [HideInInspector] public GameObject chainLightningVFXPrefab;

    [HideInInspector] public List<PowerUpType> acquiredPowerUps = new List<PowerUpType>();

    // --- NUOVO REGISTRO PER CONTARE I POTENZIAMENTI ---
    [HideInInspector]
    public Dictionary<PowerUpType, int> powerUpTracker = new Dictionary<PowerUpType, int>();
    // --- FINE NUOVO REGISTRO ---

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnSessionCoinsChanged;
    public event Action<int> OnSessionSpecialCurrencyChanged;
    // --- NUOVO EVENTO PER LA UI ---
    public event Action<bool> OnWeaponDisabledStateChanged;
    // --- FINE NUOVO EVENTO ---

    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float regenTimer;
    private int playerLayer;
    private int enemyLayer;
    private int enemyProjectilesLayer;
    private bool secondChanceAvailable = true;
    private Coroutine weaponDisableCoroutine;

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
        // --- NUOVO: Reset dei moltiplicatori a inizio partita ---
        globalDamageMultiplier = 1f;
        bounceDamageMultiplier = 1f;
        // --- FINE NUOVO ---

        ApplyPermanentUpgrades();
        tookDamageThisRun = false;
        
        if (ProgressionManager.Instance != null && ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.StartingPowerUp))
        {
            PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
            if (manager != null)
            {
                List<PowerUpEffect> options = manager.GetRandomPowerUps(3, this);
                if (options.Count > 0)
                {
                    // Usa il nuovo metodo per acquisire il potenziamento
                    AcquirePowerUp(options[0]); 
                    UIManager uiManager = FindFirstObjectByType<UIManager>();
                    if (uiManager != null) { uiManager.ShowNotification($"Starting Power-Up:\n{options[0].displayName}", 4f); }
                }
            }
        }
        currentHealth = maxHealth;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) { originalColor = spriteRenderer.color; }
        UpdateAllUI();
    }
    
    // --- NUOVO: Metodo per calcolare il danno base corretto ---
    /// <summary>
    /// Calcola il danno base del giocatore, tenendo conto dei moltiplicatori globali (es. penalità da ExtraProjectile).
    /// </summary>
    /// <returns>Il danno base attuale per un nuovo proiettile.</returns>
    public int GetCurrentDamage()
    {
        return Mathf.RoundToInt(damage * globalDamageMultiplier);
    }
    // --- FINE NUOVO ---

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
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Pickup pickup = other.GetComponent<Pickup>();
        if (pickup != null)
        {
            switch (pickup.type)
            {
                case Pickup.PickupType.Coin: CollectCoin(pickup.value); break;
                case Pickup.PickupType.Gem: CollectSpecialCurrency(pickup.value); break;
                case Pickup.PickupType.Health: Heal(pickup.value); break;
            }
            pickup.Collect();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;
        // Se subiamo danno per la prima volta, registralo.
        tookDamageThisRun = true;

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

        // Continua a salire di livello finché c'è abbastanza XP
        while (currentXP >= xpToLevelUp)
        {
            currentXP -= xpToLevelUp;
            LevelUp();
        }
        OnXPChanged?.Invoke(currentXP, xpToLevelUp);
    }
    
    // Rimosso OnLevelUp?.Invoke(level) da AddXP per evitare chiamate duplicate

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
    
    // --- NUOVO METODO PUBBLICO PER ACQUISIRE POTENZIAMENTI ---
    public void AcquirePowerUp(PowerUpEffect powerUp)
    {
        // 1. Applica l'effetto del potenziamento
        powerUp.Apply(this);

        // 2. Registra che abbiamo acquisito questo tipo di potenziamento
        if (!acquiredPowerUps.Contains(powerUp.type))
        {
            acquiredPowerUps.Add(powerUp.type);
        }

        // 3. Aggiorna il contatore per questo tipo di potenziamento
        if (powerUpTracker.ContainsKey(powerUp.type))
        {
            powerUpTracker[powerUp.type]++;
        }
        else
        {
            powerUpTracker[powerUp.type] = 1;
        }

        // Ora questo log viene mostrato solo se l'interruttore corrispondente è attivo.
        if (DebugManager.Instance != null && DebugManager.Instance.showPowerUpAcquisitionLogs)
        {
            Debug.Log($"Acquisito potenziamento: {powerUp.displayName}. Conteggio attuale: {powerUpTracker[powerUp.type]}");
        }
    }
    
    // --- NUOVO METODO PUBBLICO PER IL DEBUFF ---
    public void ApplyWeaponDisable(float duration)
    {
        // Se c'è già un debuff attivo, lo "rinfreschiamo" riavviando la coroutine
        // con la nuova durata, per evitare sovrapposizioni.
        if (weaponDisableCoroutine != null)
        {
            StopCoroutine(weaponDisableCoroutine);
        }
        weaponDisableCoroutine = StartCoroutine(WeaponDisableCoroutine(duration));
    }
    // --- FINE NUOVO METODO ---

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
        pendingLevelUps++; // Aggiungi un level up alla coda
        xpToLevelUp = Mathf.RoundToInt(xpToLevelUp * 1.2f);
        AudioManager.Instance.PlaySound(AudioManager.Instance.levelUpSound);
        OnLevelUp?.Invoke(level);

        // Se un processo di level up non è GIA' in corso, avvialo.
        if (!isShowingPowerUpPanel)
        {
        StartCoroutine(ShowLevelUpPanelSequence());
        }
    }

    private IEnumerator ShowLevelUpPanelSequence()
    {
        isShowingPowerUpPanel = true;
        yield return new WaitForSecondsRealtime(levelUpPanelDelay);

        // Cicla finché ci sono level up in sospeso
        while (pendingLevelUps > 0)
        {
            Time.timeScale = 0f; // Pausa all'inizio di ogni scelta

        PlayerController playerController = GetComponent<PlayerController>();
        WeaponEvolutionData availableEvolution = EvolutionManager.Instance.CheckForAvailableEvolutions(this, playerController.GetCurrentWeaponData());

        // Se c'è un'evoluzione disponibile...
        if (availableEvolution != null)
        {
            // ...mostra la scelta speciale per l'evoluzione!
            PowerUpUI.Instance.ShowEvolutionChoice(availableEvolution, this, playerController);
        }
        else // Altrimenti, procedi con i normali potenziamenti
        {
            PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
            if (manager != null)
            {
                List<PowerUpEffect> options = manager.GetRandomPowerUps(3, this);
                PowerUpUI.Instance.ShowPowerUpChoices(options, this);
            }
        }

            // Attendi finché la UI non ci dice che il giocatore ha scelto
            yield return new WaitUntil(() => PowerUpUI.Instance.hasMadeChoice);

            // Rimuovi un level up dalla coda dopo che la scelta è stata fatta
            pendingLevelUps--;
        }

        // Una volta finiti tutti i level up, nascondi il pannello e riprendi il gioco
        PowerUpUI.Instance.HidePanel();
        Time.timeScale = 1f;
        isShowingPowerUpPanel = false;
    }

    private IEnumerator InvulnerabilityCoroutine()
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
                yield return new WaitForSecondsRealtime(currentBlinkInterval);
                spriteRenderer.color = originalColor;
                yield return new WaitForSecondsRealtime(currentBlinkInterval);
            }
            totalElapsed += (currentBlinkInterval * 2);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyProjectilesLayer, false);
        isInvulnerable = false;
    }

    private IEnumerator ShieldCoroutine(float duration)
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
    
    // --- NUOVA COROUTINE PER IL DEBUFF ---
    private IEnumerator WeaponDisableCoroutine(float duration)
    {
        isWeaponDisabled = true;
        OnWeaponDisabledStateChanged?.Invoke(true); // Notifica la UI
        Debug.Log("Arma disabilitata!");

        yield return new WaitForSeconds(duration);
        
        // Controlla che il componente esista ancora (il giocatore potrebbe essere morto)
        if(this != null)
        {
            isWeaponDisabled = false;
            OnWeaponDisabledStateChanged?.Invoke(false); // Notifica la UI
            Debug.Log("Arma riabilitata!");
        }
        weaponDisableCoroutine = null;
    }
    // --- FINE NUOVA COROUTINE ---

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
}