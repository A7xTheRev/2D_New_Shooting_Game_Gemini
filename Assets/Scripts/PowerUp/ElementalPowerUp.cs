using UnityEngine;

// --- CLASSE SPECIALIZZATA PER POTENZIAMENTI ELEMENTALI ---
[CreateAssetMenu(fileName = "New Elemental PowerUp", menuName = "PowerUps/Elemental Effect")]
public class ElementalPowerUp : PowerUpEffect
{
    [Header("Impostazioni Effetto Incendiario")]
    public float burnDuration;
    [Tooltip("Il danno è calcolato come: Ability Power * questo moltiplicatore, al secondo.")]
    public float burnDamageMultiplier;

    [Header("Impostazioni Effetto Congelante")]
    public float slowDuration;
    [Tooltip("Moltiplicatore velocità di movimento (es. 0.5 per rallentare del 50%).")]
    public float slowMultiplier;

    [Header("Impostazioni Fulmine a Catena")]
    [Tooltip("Il prefab dell'effetto visivo del fulmine (un LineRenderer con lo script ChainLightningVFX).")]
    public GameObject chainLightningVFXPrefab;
    [Tooltip("Numero di nemici aggiuntivi che il fulmine può colpire.")]
    public int chainCount;
    [Tooltip("Moltiplicatore del danno per il colpo iniziale del fulmine, basato sull'Ability Power.")]
    public float initialChainDamageMultiplier = 1.5f;
    [Tooltip("Moltiplicatore del danno per ogni salto successivo (es. 0.7 per il 70% del danno precedente).")]
    public float chainDamageMultiplier;

    public override void Apply(PlayerStats player)
    {
        switch (type)
        {
            case PowerUpType.IncendiaryRounds:
                player.hasIncendiaryRounds = true;
                player.burnDuration = burnDuration;
                player.burnDamageMultiplier = burnDamageMultiplier;
                break;
            case PowerUpType.CryoRounds:
                player.hasCryoRounds = true;
                player.cryoSlowDuration = slowDuration;
                player.cryoSlowMultiplier = slowMultiplier;
                break;
            case PowerUpType.ChainLightning:
                player.hasChainLightning = true;
                player.chainCount = chainCount;
                player.initialChainDamageMultiplier = initialChainDamageMultiplier;
                player.chainDamageMultiplier = chainDamageMultiplier;
                // Passiamo il riferimento al prefab al giocatore
                player.chainLightningVFXPrefab = chainLightningVFXPrefab; 
                break;
        }
    }
}