using UnityEngine;
using System;
using System.Collections;

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

    [Header("Effetto Visivo (Hit)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    [Header("Effetto Visivo (Morte)")]
    public string deathVFXTag = "EnemyExplosion";
    public float deathShakeDuration = 0f;
    public float deathShakeMagnitude = 0f;

    public event Action<int, int> OnHealthChanged;
    
    private Animator animator;
    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // --- LOGICA DEL COLORE RIMOSSA DA QUI ---
    }

    void Start()
    {
        // --- LOGICA DEL COLORE SPOSTATA QUI ---
        // Ora salviamo il colore in Start(). Questo avviene DOPO che EliteStats
        // ha già colorato il nemico di viola nel suo Awake().
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        // --- FINE SPOSTAMENTO ---

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
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffect());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
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
            spriteRenderer.color = originalColor; // Ora originalColor è viola!
        }
        flashCoroutine = null;
    }

    public void Die()
    {
        if (isDying) return;
        isDying = true;

        if (deathShakeDuration > 0f && deathShakeMagnitude > 0f)
        {
            CameraShake.Instance.StartShake(deathShakeDuration, deathShakeMagnitude);
        }

        if (!string.IsNullOrEmpty(deathVFXTag))
        {
            GameObject vfx = VFXPool.Instance.GetVFX(deathVFXTag);
            if (vfx != null)
            {
            vfx.transform.position = transform.position;
            vfx.transform.localScale = transform.localScale;
            }
        }

        AudioManager.Instance.PlaySound(AudioManager.Instance.enemyHitSound);

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
        
            Destroy(gameObject);
    }
}