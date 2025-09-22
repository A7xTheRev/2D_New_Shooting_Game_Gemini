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
    // --- VARIABILI AGGIUNTE ---
    [HideInInspector] public Color flashColor;
    [HideInInspector] public float flashDuration;
    // --- FINE AGGIUNTA ---

    // L'unica statistica che cambia durante il gioco
    public int currentHealth;

    public event Action<int, int> OnHealthChanged;

    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine burnCoroutine;
    private Animator animator;

    void Awake()
    {
        if (enemyData == null)
        {
            Debug.LogError("ATTENZIONE: Nessun EnemyData assegnato a " + gameObject.name);
            return;
        }

        // Carica tutti i dati dalla "scheda"
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
        // --- RIGHE AGGIUNTE ---
        flashColor = enemyData.flashColor;
        flashDuration = enemyData.flashDuration;
        // --- FINE AGGIUNTA ---

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
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

    public void ApplyBurn(float duration)
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnEffect(duration));
    }

    private IEnumerator BurnEffect(float duration)
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        int burnDamage = 5;
        if (player != null)
        {
            burnDamage = Mathf.Max(3, Mathf.RoundToInt(player.abilityPower * 0.25f));
        }

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
                int currentBurnDamage = Mathf.Max(1, burnDamage);
                TakeDamage(currentBurnDamage, false);
            }
            else
            {
                break;
            }
        }

        if (vfxInstance != null) Destroy(vfxInstance);
        burnCoroutine = null;
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
        if (this != null)
        {
            spriteRenderer.color = originalColor;
        }
        flashCoroutine = null;
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        // Effetti comuni a tutte le morti (disattiva collider, shake, suono)
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (deathShakeDuration > 0f && deathShakeMagnitude > 0f)
        {
            CameraShake.Instance.StartShake(deathShakeDuration, deathShakeMagnitude);
        }

        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyDeathSound);

        // Controlla se questo oggetto fa parte di un Super Boss
        SuperBossAI parentBoss = GetComponentInParent<SuperBossAI>();
        if (parentBoss != null)
        {
            // È una TORRETTA o il CORE?
            if (GetComponent<BossTurret>() != null)
            {
                // È una TORRETTA. Notifica il boss e distruggi solo questo oggetto.
                parentBoss.TurretDestroyed();
                if (!string.IsNullOrEmpty(deathVFXTag)) { /* Logica esplosione torretta */ }
                Destroy(gameObject);
            }
            else
            {
                // È il CORE. Distruggi l'intero oggetto del boss.
                if (!string.IsNullOrEmpty(deathVFXTag)) { /* Logica esplosione boss finale */ }
                Destroy(parentBoss.gameObject);
            }
            return;
        }

        // --- Se non è parte di un boss, è un nemico normale. Esegui la logica standard. ---

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            int finalCoinReward = Mathf.RoundToInt(coinReward * player.coinDropMultiplier);
            GameManager.Instance?.EnemyDefeated(finalCoinReward, xpReward, specialCurrencyReward);
            player.GetComponent<AbilityController>()?.AddChargeFromKill();
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
        Destroy(gameObject);
    }
    
    public void SetOriginalColorAfterEliteTint(Color newColor)
    {
        spriteRenderer.color = newColor;
        originalColor = newColor;
    }
}