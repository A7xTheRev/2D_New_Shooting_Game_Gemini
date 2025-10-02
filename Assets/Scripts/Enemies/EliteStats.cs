using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EliteStats : MonoBehaviour
{
    // Queste variabili ora non hanno più bisogno di essere pubbliche,
    // perché verranno impostate tramite il metodo Initialize.
    private float healthMultiplier = 3f;
    private float damageMultiplier = 2f;
    private float speedMultiplier = 1.1f;
    private float attackSpeedMultiplier = 1.5f;
    private float coinMultiplier = 5f;
    private float xpMultiplier = 4f;
    private Color eliteColorTint = Color.red;

    // NUOVO METODO PUBBLICO
    // Questo metodo viene chiamato dallo StageManager per configurare l'Elite
    public void Initialize(float hpMult, float dmgMult, float spdMult, float atkSpdMult, float coinMult, float xpMult, Color color)
    {
        healthMultiplier = hpMult;
        damageMultiplier = dmgMult;
        speedMultiplier = spdMult;
        attackSpeedMultiplier = atkSpdMult;
        coinMultiplier = coinMult;
        xpMultiplier = xpMult;
        eliteColorTint = color;
    }

    void Start()
    {
        EnemyStats stats = GetComponent<EnemyStats>();

        // Applica i moltiplicatori alle statistiche
        stats.maxHealth = Mathf.RoundToInt(stats.maxHealth * healthMultiplier);
        stats.currentHealth = stats.maxHealth;
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

        // Applica la tinta di colore
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Salva il colore originale modificato, per il flash
            stats.SetOriginalColorAfterEliteTint(eliteColorTint);
        }
    }
}