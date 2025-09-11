using UnityEngine;

// Enum = elenco di opzioni
public enum PowerUpType
{
    // Vecchi
    IncreaseDamage,
    IncreaseAttackSpeed,
    ExtraXP,
    ExtraProjectiles,
    BounceEnemy,
    BounceWall,

    // Nuovi
    IncreaseMaxHealth,
    HealthRegen,
    IncreaseMoveSpeed,
    IncreaseCritChance,
    IncreaseProjectileSize,
    IncreaseCoinDrop
}

[System.Serializable]
public class PowerUp
{
    public PowerUpType type;
    public string displayName;
    public float value;
    
    public void Apply(PlayerStats player)
    {
        switch(type)
        {
            // --- VECCHI CASE ---
            case PowerUpType.IncreaseDamage:
                player.damage += Mathf.RoundToInt(value);
                break;
            case PowerUpType.IncreaseAttackSpeed:
                player.attackSpeed += value;
                break;
            case PowerUpType.ExtraXP:
                player.xpMultiplier += value;
                break;
            case PowerUpType.ExtraProjectiles:
                player.projectileCount += Mathf.RoundToInt(value);
                break;
            case PowerUpType.BounceEnemy:
                player.bounceCountEnemy += Mathf.RoundToInt(value);
                break;
            case PowerUpType.BounceWall:
                player.bounceCountWall += Mathf.RoundToInt(value);
                break;
                
            // --- NUOVI CASE ---
            case PowerUpType.IncreaseMaxHealth:
                int healthBonus = Mathf.RoundToInt(value);
                player.maxHealth += healthBonus;
                player.Heal(healthBonus); // Cura anche il giocatore
                break;
            case PowerUpType.HealthRegen:
                player.healthRegenPerSecond += value;
                break;
            case PowerUpType.IncreaseMoveSpeed:
                player.moveSpeed += value;
                break;
            case PowerUpType.IncreaseCritChance:
                // Aumenta la probabilità di critico (es. value=0.1 per +10%)
                player.critChance += value;
                break;
            case PowerUpType.IncreaseProjectileSize:
                // Aumenta il moltiplicatore della dimensione (es. value=0.2 per +20%)
                player.projectileSizeMultiplier += value;
                break;
            case PowerUpType.IncreaseCoinDrop:
                // Aumenta il moltiplicatore delle monete (es. value=0.25 per +25%)
                player.coinDropMultiplier += value;
                break;
        }
    }
}