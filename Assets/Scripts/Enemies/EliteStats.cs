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

    void Awake()
    {
        EnemyStats stats = GetComponent<EnemyStats>();

        stats.maxHealth = Mathf.RoundToInt(stats.maxHealth * healthMultiplier);
        stats.contactDamage = Mathf.RoundToInt(stats.contactDamage * damageMultiplier);
        stats.projectileDamage = Mathf.RoundToInt(stats.projectileDamage * damageMultiplier);
        stats.moveSpeed *= speedMultiplier;
        
        if (attackSpeedMultiplier > 0)
        {
            stats.fireRate /= attackSpeedMultiplier;
        }
        
        stats.coinReward = Mathf.RoundToInt(stats.coinReward * coinMultiplier);
        stats.xpReward = Mathf.RoundToInt(stats.xpReward * xpMultiplier);
        stats.specialCurrencyReward = Mathf.RoundToInt(stats.specialCurrencyReward * coinMultiplier);

        transform.localScale *= 1.3f;
    }
}