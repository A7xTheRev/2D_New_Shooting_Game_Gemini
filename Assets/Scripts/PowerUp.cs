using UnityEngine;

// Enum = elenco di opzioni
public enum PowerUpType
{
    IncreaseDamage,      // Aumenta il danno del player
    IncreaseAttackSpeed, // Aumenta la velocità di attacco
    ExtraXP,             // Aumenta XP guadagnata
    ExtraProjectiles,    // Aumenta numero di proiettili base
    BounceEnemy,         // Rimbalzo da nemico a nemico
    BounceWall           // Rimbalzo contro i muri
}

// Classe PowerUp che definisce un singolo powerup
[System.Serializable] // Serve per farlo vedere nell'Inspector di Unity
public class PowerUp
{
    public PowerUpType type;   // Tipo di powerup, scelto dall'enum
    public string displayName; // Nome da mostrare al player
    public float value;        // Valore dell’incremento (es: +10 danno)
    
    // Metodo per applicare il powerup al player
    public void Apply(PlayerStats player)
    {
        switch(type)
        {
            case PowerUpType.IncreaseDamage:
                player.damage += Mathf.RoundToInt(value);
                break;
            case PowerUpType.IncreaseAttackSpeed:
                player.attackSpeed += value;
                break;
            case PowerUpType.ExtraXP:
                player.xpMultiplier += value; // Questo campo lo aggiungeremo al PlayerStats
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
        }
    }
}
