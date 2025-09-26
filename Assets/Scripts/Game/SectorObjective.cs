// Questo non è un MonoBehaviour, quindi non va aggiunto a nessun oggetto.
// È una definizione che useremo in altri script.

// L'attributo [Flags] permette all'editor di Unity di gestire meglio
// questo enum come una serie di opzioni combinabili.
[System.Flags]
public enum SectorObjective
{
    // Usiamo potenze di 2 per i valori
    NONE = 0,                 // 0000 in binario
    SECTOR_COMPLETED = 1,     // 0001 in binario
    HEALTH_OVER_70_PERCENT = 2, // 0010 in binario
    NO_DAMAGE_TAKEN = 4       // 0100 in binario
    // Futuro obiettivo: KILLED_50_ENEMIES = 8 (1000 in binario)
    // Futuro obiettivo: FOUND_METEORITE = 16 (10000 in binario)
}