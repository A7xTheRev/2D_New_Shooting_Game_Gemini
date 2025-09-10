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
    public float xpMultiplier = 1f;       // Modifica XP guadagnata
    public int projectileCount = 1;       // Numero di proiettili base
    public int bounceCountEnemy = 0;      // Rimbalzi verso nemici
    public int bounceCountWall = 0;       // Rimbalzi contro muri

    [Header("Coins")]
    public int sessionCoins = 0;
    public static int lastSessionCoins = 0;

    [Header("Danno e invulnerabilità")]
    public float invulnerabilityTime = 1f; // secondi di invulnerabilità
    private bool isInvulnerable = false;

    // 🔥 Aggiunte per effetto visivo
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnSessionCoinsChanged;

    void Start()
    {
        currentHealth = maxHealth;

        // Prende il renderer del player
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        UpdateAllUI();

        UIManager ui = GameObject.FindObjectOfType<UIManager>();
        if (ui != null)
        {
            OnLevelUp += ui.UpdateLevelUI;
            OnXPChanged += ui.UpdateXPUI;
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
        if (isInvulnerable) return; // Se è invulnerabile, ignora il danno

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
                // Rosso e semitrasparente
                spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
                yield return new WaitForSeconds(0.1f);

                // Colore originale
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
        // Applica il moltiplicatore XP
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

        // Mostra powerup
        PowerUpManager manager = FindObjectOfType<PowerUpManager>();
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

    public void EndSession()
    {
        lastSessionCoins = sessionCoins; // 🔥 Salva i coins ottenuti in questa partita
        ProgressionManager.Instance.AddCoins(sessionCoins);
        sessionCoins = 0;
        OnSessionCoinsChanged?.Invoke(sessionCoins);
    }

    private void Die()
    {
        Debug.Log("Player morto");

        // Salva i coin della partita per GameOver
        lastSessionCoins = sessionCoins;

        // Non chiamiamo EndSession() qui!
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }
}
