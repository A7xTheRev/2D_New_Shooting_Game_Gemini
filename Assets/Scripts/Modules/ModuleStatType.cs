public enum ModuleStatType
{
    // Statistiche Offensive
    Damage,                 // Aggiunge un valore fisso a 'damage'
    AttackSpeed,            // Aggiunge una percentuale a 'attackSpeed'
    CritChance,             // Aggiunge una percentuale a 'critChance'
    CritDamage,             // Aggiunge una percentuale a 'critDamageMultiplier'

    // Statistiche Difensive
    MaxHealth,              // Aggiunge un valore fisso a 'maxHealth'
    HealthRegen,            // Aggiunge un valore fisso a 'healthRegenPerSecond'
    
    // Statistiche di Utilità
    MoveSpeed,              // Aggiunge una percentuale a 'moveSpeed'
    AbilityPower,           // Aggiunge un valore fisso a 'abilityPower'
    XPGain,                 // Aggiunge una percentuale a 'xpMultiplier'
    CoinGain                // Aggiunge una percentuale a 'coinDropMultiplier'
}