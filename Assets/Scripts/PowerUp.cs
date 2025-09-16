using UnityEngine;

public enum PowerUpType
{

    // Generici
    IncreaseDamage,
    IncreaseAttackSpeed,
    ExtraXP,
    ExtraProjectiles,
    BounceEnemy,
    BounceWall,
    IncreaseMaxHealth,
    HealthRegen,
    IncreaseMoveSpeed,
    IncreaseCritChance,
    IncreaseProjectileSize,
    IncreaseCoinDrop,
    IncreaseAbilityPower,
    
    // Armi Base
    HomingMissile,

    // Potenziamenti HomingMissile
    HomingMissileBarrage, // Lancia +1 missile
    HomingMissileFastReload // Riduce il cooldown

}

[System.Serializable]
public class PowerUp
{
    public PowerUpType type;
    public string displayName;
    [TextArea] public string description;
    public float value;
    
    public PowerUpType prerequisite = PowerUpType.IncreaseDamage;
    public bool hasPrerequisite = false;
    
    // --- NUOVO CAMPO ---
    [Tooltip("Se spuntato, questo potenziamento può essere ottenuto una sola volta per partita.")]
    public bool isUnique = false;
    // --- FINE NUOVO CAMPO ---
    
    public void Apply(PlayerStats player)
    {
        switch (type)
        {
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
            case PowerUpType.IncreaseMaxHealth:
                int healthBonus = Mathf.RoundToInt(value);
                player.maxHealth += healthBonus;
                player.Heal(healthBonus);
                break;
            case PowerUpType.HealthRegen:
                player.healthRegenPerSecond += value;
                break;
            case PowerUpType.IncreaseMoveSpeed:
                player.moveSpeed += value;
                break;
            case PowerUpType.IncreaseCritChance:
                player.critChance += value;
                break;
            case PowerUpType.IncreaseProjectileSize:
                player.projectileSizeMultiplier += value;
                break;
            case PowerUpType.IncreaseCoinDrop:
                player.coinDropMultiplier += value;
                break;
            case PowerUpType.IncreaseAbilityPower:
                player.abilityPower += Mathf.RoundToInt(value);
                break;
            case PowerUpType.HomingMissile:
                player.homingMissileLevel++;
                break;
            case PowerUpType.HomingMissileBarrage:
                player.homingMissileCount++;
                break;
            case PowerUpType.HomingMissileFastReload:
                player.homingMissileCooldownMultiplier *= value;
                break;
        }
    }
}