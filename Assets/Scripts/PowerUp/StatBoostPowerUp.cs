using UnityEngine;

// --- CLASSE SPECIALIZZATA PER POTENZIAMENTI DI STATISTICHE ---
[CreateAssetMenu(fileName = "New Stat Boost PowerUp", menuName = "PowerUps/Stat Boost")]
public class StatBoostPowerUp : PowerUpEffect
{
    [Header("Impostazioni Statistica")]
    public float amount;

    // --- NUOVO: Campi per le penalità di bilanciamento ---
    [Header("Global Modifiers")]
    [Tooltip("IT: Moltiplicatore di danno globale applicato al giocatore quando questo potenziamento viene scelto (es. 0.8 per un -20% di danno). Lasciare a 1 per nessun effetto.")]
    [Range(0.1f, 2f)]
    public float globalDamagePenalty = 1f;

    [Header("Bounce Settings")]
    [Tooltip("IT: Moltiplicatore di danno per ogni rimbalzo successivo (es. 0.75 per fare il 75% del danno precedente). Lasciare a 1 per nessun effetto.")]
    [Range(0.1f, 1f)]
    public float bounceDamageMultiplier = 1f;
    // --- FINE NUOVO ---

    public override void Apply(PlayerStats player)
    {
        // --- NUOVO: Applica la penalità di danno globale se presente ---
        if (globalDamagePenalty != 1f)
        {
            player.globalDamageMultiplier *= globalDamagePenalty;
        }
        // --- FINE NUOVO ---

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
            
            // --- MODIFICA: Aggiunta logica per il moltiplicatore di danno al rimbalzo ---
            case PowerUpType.BounceEnemy:
                player.bounceCountEnemy += Mathf.RoundToInt(amount);
                // Assegna il moltiplicatore di danno al giocatore.
                // Se prendiamo un nuovo powerup di rimbalzo, teniamo la penalità peggiore (la più bassa).
                if (bounceDamageMultiplier < player.bounceDamageMultiplier)
                {
                    player.bounceDamageMultiplier = bounceDamageMultiplier;
                }
                break;
            // --- FINE MODIFICA ---

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