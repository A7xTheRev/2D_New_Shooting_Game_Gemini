using UnityEngine;

/// <summary>
/// Definisce i tipi di ricompensa che un giocatore può ricevere al level up del pilota.
/// </summary>
public enum PilotRewardType
{
    Coins,
    Gems,
    ModuleSlot,
    // Futuri tipi di ricompensa
    // ShipUnlock,
    // Lootbox
}

/// <summary>
/// Struttura dati che definisce una singola ricompensa per un determinato livello pilota.
/// Configurabile dall'Inspector del ProgressionManager.
/// </summary>
[System.Serializable]
public class PilotLevelReward
{
    public int level;
    public PilotRewardType rewardType;
    public int amount; // Usato per Coins e Gems
    public ModuleSlotType moduleSlotType; // Usato per ModuleSlot
    public Sprite rewardIcon; // Icona da mostrare nel popup
}