using UnityEngine;

[CreateAssetMenu(fileName = "MOD_", menuName = "Astro Survivor/Module Data")]
public class ModuleData : ScriptableObject
{
    [Header("Identificazione Modulo")]
    [Tooltip("ID unico per il modulo (es. 'mod_damage_common'). Usato per salvataggi e inventario.")]
    public string moduleID;
    
    [Tooltip("Nome visualizzato in gioco.")]
    public string moduleName;
    
    [Tooltip("Descrizione del bonus che fornisce il modulo.")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icona del modulo.")]
    public Sprite icon;

    [Header("Proprietà di Gioco")]
    [Tooltip("Rarità del modulo, influenza il colore e la potenza.")]
    public ModuleRarity rarity;

    [Tooltip("Tipo di slot in cui questo modulo può essere equipaggiato.")]
    public ModuleSlotType slotType;

    [Header("Bonus Statistiche")]
    [Tooltip("La statistica specifica del giocatore che questo modulo andrà a modificare.")]
    public ModuleStatType statToModify;

    [Tooltip("Il valore del bonus. Per le percentuali, usare valori decimali (es. 0.1 per +10%).")]
    public float bonusValue;

    [Header("Sistema di Fusione (Crafting)")]
    [Tooltip("Il modulo di rarità superiore che si ottiene fondendo 3 moduli di questo tipo. Lasciare vuoto se è la rarità massima.")]
    public ModuleData fusionResult;
}