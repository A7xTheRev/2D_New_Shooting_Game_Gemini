using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EliteStats : MonoBehaviour
{
    [Header("Moltiplicatori Statistiche Elite")]
    public float healthMultiplier = 3f;
    public float damageMultiplier = 2f;
    public float speedMultiplier = 1.1f;
    public float attackSpeedMultiplier = 1.5f;

    [Header("Moltiplicatori Ricompense Elite")]
    public float coinMultiplier = 5f;
    public float xpMultiplier = 4f;

    // --- NUOVA SEZIONE ---
    [Header("Aspetto Visivo Elite")]
    [Tooltip("Il colore che assumerà lo sprite di questo nemico Elite.")]
    public Color eliteColorTint = Color.red;
    // --- FINE NUOVA SEZIONE ---

    // --- METODO RINOMINATO DA Awake A Start ---
    // In questo modo, siamo sicuri che venga eseguito DOPO l'Awake di EnemyStats,
    // che ha già caricato i dati base dall'EnemyData.
    void Start()
    {
        EnemyStats stats = GetComponent<EnemyStats>();

        // Applica i moltiplicatori alle statistiche
        stats.maxHealth = Mathf.RoundToInt(stats.maxHealth * healthMultiplier);
        stats.currentHealth = stats.maxHealth; // Assicurati di aggiornare anche la vita corrente
        stats.contactDamage = Mathf.RoundToInt(stats.contactDamage * damageMultiplier);
        stats.projectileDamage = Mathf.RoundToInt(stats.projectileDamage * damageMultiplier);
        stats.moveSpeed *= speedMultiplier;
        
        if (attackSpeedMultiplier > 0)
        {
            stats.fireRate /= attackSpeedMultiplier;
        }
        
        // Applica i moltiplicatori alle ricompense
        stats.coinReward = Mathf.RoundToInt(stats.coinReward * coinMultiplier);
        stats.xpReward = Mathf.RoundToInt(stats.xpReward * xpMultiplier);
        stats.specialCurrencyReward = Mathf.RoundToInt(stats.specialCurrencyReward * coinMultiplier);

        // Aumenta la scala
        transform.localScale *= 1.3f;

        // --- NUOVA LOGICA PER IL COLORE ---
        // Cerca lo SpriteRenderer e applica la tinta di colore
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Salva il colore originale modificato, per il flash
            stats.SetOriginalColorAfterEliteTint(eliteColorTint);
        }
        // --- FINE NUOVA LOGICA ---
    }
}