using UnityEngine;

// --- CLASSE SPECIALIZZATA PER POTENZIAMENTI DI STATISTICHE ---
[CreateAssetMenu(fileName = "New Stat Boost PowerUp", menuName = "PowerUps/Stat Boost")]
public class StatBoostPowerUp : PowerUpEffect
{
    [Header("Impostazioni Statistica")]
    public float amount;

    public override void Apply(PlayerStats player)
    {
        // La vecchia logica di Apply() per le statistiche ora vive qui
        switch (type)
        {
            case PowerUpType.IncreaseDamage: player.damage += Mathf.RoundToInt(amount); break;
            case PowerUpType.IncreaseAttackSpeed: player.attackSpeed += amount; break;
            case PowerUpType.ExtraProjectiles: player.projectileCount += Mathf.RoundToInt(amount); break;
            case PowerUpType.IncreaseMaxHealth: player.maxHealth += Mathf.RoundToInt(amount); player.Heal(Mathf.RoundToInt(amount)); break;
            case PowerUpType.HealthRegen: player.healthRegenPerSecond += amount; break;
            case PowerUpType.IncreaseMoveSpeed: player.moveSpeed += amount; break;
            case PowerUpType.IncreaseCritChance: player.critChance += amount; break;
            case PowerUpType.IncreaseProjectileSize: player.projectileSizeMultiplier += amount; break;
            case PowerUpType.IncreaseCoinDrop: player.coinDropMultiplier += amount; break;
            case PowerUpType.IncreaseAbilityPower: player.abilityPower += Mathf.RoundToInt(amount); break;
            case PowerUpType.ExtraXP: player.xpMultiplier += amount; break;
            case PowerUpType.BounceEnemy: player.bounceCountEnemy += Mathf.RoundToInt(amount); break;
            case PowerUpType.BounceWall: player.bounceCountWall += Mathf.RoundToInt(amount); break;
            case PowerUpType.HomingMissile: player.homingMissileLevel++; break;
            case PowerUpType.HomingMissileBarrage: player.homingMissileCount++; break;
            case PowerUpType.HomingMissileFastReload: player.homingMissileCooldownMultiplier *= amount; break;
            case PowerUpType.CombatDrone: player.combatDroneLevel++; break;
            case PowerUpType.DroneCompanion: player.combatDroneLevel++; break;
            case PowerUpType.DroneFastReload: player.combatDroneFireRateMultiplier *= amount; break;
            case PowerUpType.DronePiercingShots: player.dronesHavePiercingShots = true; break;
        }
    }
}

