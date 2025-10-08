using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyStats : MonoBehaviour
{
    [Header("Configurazione")]
    [Tooltip("La 'scheda' con tutte le statistiche di questo tipo di nemico.")]
    public EnemyData enemyData;

    // Statistiche caricate dall'EnemyData
    [HideInInspector] public int maxHealth;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public int contactDamage;
    [HideInInspector] public int projectileDamage;
    [HideInInspector] public float fireRate;
    [HideInInspector] public int coinReward;
    [HideInInspector] public int xpReward;
    [HideInInspector] public int specialCurrencyReward;
    [HideInInspector] public bool hasDeathAnimation;
    [HideInInspector] public string deathVFXTag;
    [HideInInspector] public float deathShakeDuration;
    [HideInInspector] public float deathShakeMagnitude;
    [HideInInspector] public GameObject burnVFX;
    [HideInInspector] public bool allowStatScaling;
    [HideInInspector] public Color flashColor;
    [HideInInspector] public float flashDuration;
    [HideInInspector] public float gemDropChance;
    [HideInInspector] public float healthDropChance;

    // L'unica statistica che cambia durante il gioco
    public int currentHealth;
    public event Action<int, int> OnHealthChanged;

    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private float originalMoveSpeed;

    private Coroutine flashCoroutine;
    private Coroutine burnCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine buffCoroutine;

    void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError("ATTENZIONE: Nessun EnemyData assegnato a " + gameObject.name);
            return;
        }

        // Carica tutti i dati base dalla "scheda"
        allowStatScaling = enemyData.allowStatScaling;
        maxHealth = enemyData.maxHealth;
        moveSpeed = enemyData.moveSpeed;
        contactDamage = enemyData.contactDamage;
        projectileDamage = enemyData.projectileDamage;
        fireRate = enemyData.fireRate;
        coinReward = enemyData.coinReward;
        xpReward = enemyData.xpReward;
        specialCurrencyReward = enemyData.specialCurrencyReward;
        hasDeathAnimation = enemyData.hasDeathAnimation;
        deathVFXTag = enemyData.deathVFXTag;
        deathShakeDuration = enemyData.deathShakeDuration;
        deathShakeMagnitude = enemyData.deathShakeMagnitude;
        burnVFX = enemyData.burnVFX;
        flashColor = enemyData.flashColor;
        flashDuration = enemyData.flashDuration;
        gemDropChance = enemyData.gemDropChance;
        healthDropChance = enemyData.healthDropChance;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Salviamo la velocità di movimento originale QUI, prima di ogni scaling
        originalMoveSpeed = moveSpeed; 
    }

    void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // --- NUOVO METODO CENTRALE PER LO SCALING ---
    public void ApplyStatScaling(float multiplier)
    {
        if (!allowStatScaling || multiplier <= 1f) return;

        // Applica il moltiplicatore a tutte le statistiche rilevanti
        maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        contactDamage = Mathf.RoundToInt(contactDamage * multiplier);
        projectileDamage = Mathf.RoundToInt(projectileDamage * multiplier);
        coinReward = Mathf.RoundToInt(coinReward * multiplier);
        xpReward = Mathf.RoundToInt(xpReward * multiplier);
        
        // Aggiorna la vita corrente per riflettere la nuova vita massima
        currentHealth = maxHealth;
        
        // Notifica la UI che la vita è cambiata
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    // --- FINE NUOVO METODO ---

    public void ApplySpeedBuff(float speedMultiplier, float duration, Color buffColor)
    {
        if (isDying) return;
        if (buffCoroutine != null) StopCoroutine(buffCoroutine);
        buffCoroutine = StartCoroutine(SpeedBuffCoroutine(speedMultiplier, duration, buffColor));
    }

    private IEnumerator SpeedBuffCoroutine(float multiplier, float duration, Color buffColor)
    {
        // Applica l'aumento di velocità e cambia colore
        moveSpeed = originalMoveSpeed * multiplier;
        if(spriteRenderer != null) spriteRenderer.color = buffColor;

        yield return new WaitForSeconds(duration);

        // Ripristina la velocità e il colore originali (se l'oggetto esiste ancora)
        if(this != null) 
        {
            moveSpeed = originalMoveSpeed;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }
        buffCoroutine = null;
    }
    // --- FINE NUOVA SEZIONE ---

    public void TakeDamage(int damageAmount, bool isCrit)
    {
        if (isDying) return;

        ShowDamageNumber(damageAmount, isCrit);
        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyHitSound);
        currentHealth -= damageAmount;
        if (currentHealth < 0) currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

     public void Heal(int amount)
    {
        if (isDying || currentHealth <= 0) return;
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Notifica la UI (es. la barra della vita) che la salute è cambiata
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Qui potremmo anche mostrare un numero verde per indicare la cura
    }

    public void ApplyBurn(float duration, int damagePerTick)
    {
        if (isDying) return;
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnEffect(duration, damagePerTick));
    }

    private IEnumerator BurnEffect(float duration, int damagePerTick)
    {
        GameObject vfxInstance = null;
        if (burnVFX != null)
        {
            vfxInstance = Instantiate(burnVFX, transform.position, Quaternion.identity, transform);
        }

        float timer = duration;
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer -= 1f;

            if (this != null && currentHealth > 0)
            {
                TakeDamage(damagePerTick, false);
            }
            else
            {
                if (vfxInstance != null) Destroy(vfxInstance);
                yield break; // Esce dalla coroutine se il nemico è morto
            }
        }

        if (vfxInstance != null) Destroy(vfxInstance);
        burnCoroutine = null;
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        // Se c'è già un rallentamento attivo, lo ferma prima di applicare quello nuovo
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowEffect(slowMultiplier, duration));
    }

    private IEnumerator SlowEffect(float multiplier, float duration)
    {
        // Salva il colore originale e applica una tinta blu
        Color initialColor = spriteRenderer.color;
        spriteRenderer.color = new Color(0.5f, 0.8f, 1f, initialColor.a); // Tinta azzurra

        // Applica il rallentamento
        moveSpeed = originalMoveSpeed * multiplier;

        // Aspetta per la durata dell'effetto
        yield return new WaitForSeconds(duration);

        // Ripristina la velocità e il colore originali
        if(this != null) // Controlla se l'oggetto esiste ancora
        {
            moveSpeed = originalMoveSpeed;
            spriteRenderer.color = initialColor;
        }
        slowCoroutine = null;
    }
    
    private void ShowDamageNumber(int damageAmount, bool isCrit)
    {
        GameObject numberObject = VFXPool.Instance.GetVFX("DamageNumber");
        if (numberObject != null)
        {
            Vector3 spawnPosition = transform.position + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), 0.5f, 0);
            spawnPosition.z = -1f;
            numberObject.transform.position = spawnPosition;
            DamageNumber dn = numberObject.GetComponent<DamageNumber>();
            if (dn != null)
            {
                dn.Show(damageAmount, isCrit);
            }
        }
    }

    private IEnumerator FlashEffect()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (this != null && spriteRenderer.color == flashColor)
        {
            spriteRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }

        private void GrantRewards(PlayerStats player)
    {
        if (player == null || LootManager.Instance == null) return;

        player.AddXP(xpReward);

        // --- NUOVA LOGICA DI DROP DINAMICA PER LE MONETE ---
            int amountToDrop = Mathf.RoundToInt(coinReward * player.coinDropMultiplier);
            
        // Gestisce le monete d'oro
        if (LootManager.Instance.coinGoldPrefab != null)
        {
            int goldValue = LootManager.Instance.coinGoldPrefab.GetComponent<Pickup>().value;
            int goldCoins = amountToDrop / goldValue;
            if (goldCoins > 0)
        {
                for (int i = 0; i < goldCoins; i++)
                    Instantiate(LootManager.Instance.coinGoldPrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
                amountToDrop %= goldValue;
            }
        }

        // Gestisce le monete d'argento
        if (LootManager.Instance.coinSilverPrefab != null)
        {
            int silverValue = LootManager.Instance.coinSilverPrefab.GetComponent<Pickup>().value;
            int silverCoins = amountToDrop / silverValue;
            if (silverCoins > 0)
            {
                for (int i = 0; i < silverCoins; i++)
                    Instantiate(LootManager.Instance.coinSilverPrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
                amountToDrop %= silverValue;
            }
            }

        // Gestisce le monete di bronzo
            if (amountToDrop > 0 && LootManager.Instance.coinBronzePrefab != null)
            {
                for (int i = 0; i < amountToDrop; i++)
                    Instantiate(LootManager.Instance.coinBronzePrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
            }
        // --- FINE LOGICA MONETE ---

        // --- NUOVA LOGICA DI DROP DINAMICA PER LE GEMME ---
        if (specialCurrencyReward > 0 && UnityEngine.Random.value < gemDropChance)
        {
            int gemsToDrop = specialCurrencyReward;

            // Gemme Gialle (valore più alto)
            if (LootManager.Instance.gemYellowPrefab != null)
            {
                int yellowValue = LootManager.Instance.gemYellowPrefab.GetComponent<Pickup>().value;
                int yellowGems = gemsToDrop / yellowValue;
                if (yellowGems > 0)
                {
                    for (int i = 0; i < yellowGems; i++)
                        Instantiate(LootManager.Instance.gemYellowPrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
                    gemsToDrop %= yellowValue;
                }
            }

            // Gemme Verdi (valore medio)
            if (LootManager.Instance.gemGreenPrefab != null)
            {
                int greenValue = LootManager.Instance.gemGreenPrefab.GetComponent<Pickup>().value;
                int greenGems = gemsToDrop / greenValue;
                if (greenGems > 0)
        {
                    for (int i = 0; i < greenGems; i++)
                        Instantiate(LootManager.Instance.gemGreenPrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
                    gemsToDrop %= greenValue;
                }
            }

            // Gemme Blu (valore base)
            if (gemsToDrop > 0 && LootManager.Instance.gemBluePrefab != null)
            {
                for (int i = 0; i < gemsToDrop; i++)
                    Instantiate(LootManager.Instance.gemBluePrefab, transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.5f, Quaternion.identity);
            }
        }
        // --- FINE LOGICA GEMME ---

        // Drop Vita (invariato)
        if (LootManager.Instance.healthPickupPrefab != null && UnityEngine.Random.value < healthDropChance)
        {
            Instantiate(LootManager.Instance.healthPickupPrefab, transform.position, Quaternion.identity);
        }

        // --- NUOVA LOGICA PER IL DROP DEI MODULI ---
    if (ProgressionManager.Instance != null && enemyData.moduleRarityDropChances.Count > 0)
        {
        // Controlla se il "tiro" generale per droppare un modulo ha successo
            if (UnityEngine.Random.value < enemyData.moduleDropChance)
            {
                // Chiama il nuovo metodo del ProgressionManager per ottenere un modulo
                ModuleData droppedModule = ProgressionManager.Instance.GetRandomModuleDrop(enemyData.moduleRarityDropChances);
                if (droppedModule != null)
                {
                    // Aggiungi il modulo all'inventario del giocatore
                    ProgressionManager.Instance.AddModule(droppedModule.moduleID, 1);
                    
                    // TODO in futuro: Istanziare un prefab "Pickup Modulo" qui
                    Debug.Log($"Nemico ha droppato il modulo: {droppedModule.moduleName}");
                }
            }
        }
        // --- FINE NUOVA LOGICA ---

        player.GetComponent<AbilityController>()?.AddChargeFromKill();
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        // Prima di tutto, controlla se c'è un modificatore e attiva il suo effetto di morte
        EliteModifier modifier = GetComponent<EliteModifier>();
        if (modifier != null)
        {
            modifier.OnDeath();
        }
        // Effetti comuni a tutte le morti (disattiva collider, shake, suono)
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (enemyData != null)
        {
            ProgressionManager.Instance?.AddEnemyKill(enemyData.name);
        }

        if (deathShakeDuration > 0f && deathShakeMagnitude > 0f)
        {
            CameraShake.Instance.StartShake(deathShakeDuration, deathShakeMagnitude);
        }

        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyDeathSound);

        // Controlla se questo oggetto fa parte di un Super Boss
        SuperBossAI parentBoss = GetComponentInParent<SuperBossAI>();
        if (parentBoss != null)
        {
            // --- LOGICA ANIMAZIONE E RICOMPENSE BOSS CORRETTA ---
            if (GetComponent<BossTurret>() != null)
            {
                // È una TORRETTA: notifica il boss e si distrugge. Nessuna ricompensa.
                parentBoss.TurretDestroyed();
                if (animator != null && hasDeathAnimation)
                    animator.SetTrigger("Die");
                else
                Destroy(gameObject);
            }
            else // È il Core
            {
                // È il CORE: rilascia le ricompense e poi distrugge l'intero boss.
                PlayerStats player = FindFirstObjectByType<PlayerStats>();
                if (player != null) GrantRewards(player);
                
                if (animator != null && hasDeathAnimation)
                    animator.SetTrigger("Die");
                else
                Destroy(parentBoss.gameObject);
            }
            return;
        }

        // --- LOGICA DI DIVISIONE MODULARE ---
        // Cerca il componente che definisce il comportamento di divisione
        SplittingBehavior splitter = GetComponent<SplittingBehavior>();
        if (splitter != null)
        {
            // Se sì, chiama il suo metodo Split() per generare i figli
            splitter.Split();
        }
        
        // --- Logica per i nemici normali ---
        PlayerStats normalPlayer = FindFirstObjectByType<PlayerStats>();
        if (normalPlayer != null)
        {
            GrantRewards(normalPlayer);
        }

        if (animator != null && hasDeathAnimation)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            if (!string.IsNullOrEmpty(deathVFXTag))
            {
                GameObject vfx = VFXPool.Instance.GetVFX(deathVFXTag);
                if (vfx != null)
                {
                    vfx.transform.position = transform.position;
                    vfx.transform.localScale = transform.localScale;
                }
            }
            Destroy(gameObject);
        }
    }

    public void OnDeathAnimationFinished()
    {
        // --- LOGICA AGGIUNTA PER GESTIRE LA DISTRUZIONE POST-ANIMAZIONE ---
        SuperBossAI parentBoss = GetComponentInParent<SuperBossAI>();
        if (parentBoss != null && GetComponent<BossTurret>() == null)
        {
            // Se sono il Core del boss, la mia animazione di morte distrugge l'intero boss.
            Destroy(parentBoss.gameObject);
        }
        else
        {
            // Se sono un nemico normale o una torretta, distruggo solo me stesso.
        Destroy(gameObject);
        }
    }
    
    public void SetOriginalColorAfterEliteTint(Color newColor)
    {
        spriteRenderer.color = newColor;
        originalColor = newColor;
    }
}