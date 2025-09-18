using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyStats : MonoBehaviour
{
    [Header("Scaling")]
    public bool allowStatScaling = true;

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
    public bool hasDeathAnimation = false;
    
    [Header("Effetto Visivo (Hit)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    [Header("Effetto Visivo (Morte)")]
    public string deathVFXTag = "EnemyExplosion";
    public float deathShakeDuration = 0f;
    public float deathShakeMagnitude = 0f;

    [Header("Effetti di Stato")]
    public GameObject burnVFX;
    
    public event Action<int, int> OnHealthChanged;
    
    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine burnCoroutine;
    private Animator animator;

    void Awake()
    {
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
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
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

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
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
        if (deathShakeDuration > 0f && deathShakeMagnitude > 0f) CameraShake.Instance.StartShake(deathShakeDuration, deathShakeMagnitude);
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
            return; // Esci dal metodo per non dare ricompense multiple.
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
}