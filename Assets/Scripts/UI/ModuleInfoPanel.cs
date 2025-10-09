// Nome File: ModuleInfoPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModuleInfoPanel : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI moduleNameText;
    public TextMeshProUGUI rarityText;
    public Image moduleIcon;
    public Image rarityBorder; // Bordo per l'icona, per coerenza visiva
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public Button closeButton;

    // Riferimenti ai colori delle rarità dal ModuleInventoryItem per coerenza
    private ModuleInventoryItem rarityColors; 

    void Awake()
    {
        // Aggiunge la funzionalità al pulsante di chiusura
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        // Prendiamo un riferimento a un ModuleInventoryItem per "rubargli" i colori delle rarità
        // Questo evita di doverli definire in due posti diversi
        rarityColors = GetComponentInChildren<ModuleInventoryItem>(true);
        if (rarityColors == null)
        {
            // Se non lo trova, creiamo un'istanza temporanea solo per i colori
            GameObject tempItem = new GameObject("TempRarityColorHolder");
            rarityColors = tempItem.AddComponent<ModuleInventoryItem>();
        }

        // Assicura che il pannello sia nascosto all'avvio
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Mostra il pannello e lo popola con i dati di un modulo.
    /// </summary>
    public void Show(ModuleData moduleData)
    {
        if (moduleData == null) return;

        gameObject.SetActive(true);

        // Popola i campi UI
        moduleNameText.text = moduleData.moduleName;
        rarityText.text = moduleData.rarity.ToString();
        moduleIcon.sprite = moduleData.icon;
        descriptionText.text = moduleData.description;

        // Formatta il testo del bonus
        statsText.text = FormatBonusText(moduleData);

        // Imposta il colore della rarità sul testo e sul bordo
        Color color = GetRarityColor(moduleData.rarity);
        rarityText.color = color;
        if(rarityBorder != null) rarityBorder.color = color;
    }

    /// <summary>
    /// Nasconde il pannello.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Helper per formattare il testo del bonus in modo leggibile (es. +5% o +20).
    /// </summary>
    private string FormatBonusText(ModuleData data)
    {
        string statName = data.statToModify.ToString();
        string bonusString;

        // Controlla se la statistica è una percentuale
        switch (data.statToModify)
        {
            case ModuleStatType.AttackSpeed:
            case ModuleStatType.CritChance:
            case ModuleStatType.CritDamage:
            case ModuleStatType.MoveSpeed:
            case ModuleStatType.XPGain:
            case ModuleStatType.CoinGain:
                bonusString = $"+{data.bonusValue:P0}"; // P0 formatta in percentuale senza decimali
                break;
            default: // Altrimenti è un valore fisso
                bonusString = $"+{data.bonusValue}";
                break;
        }

        return $"{statName} {bonusString}";
    }

    /// <summary>
    /// Helper per ottenere il colore corretto per una data rarità.
    /// </summary>
    private Color GetRarityColor(ModuleRarity rarity)
    {
        if(rarityColors == null) return Color.white;
        
        switch (rarity)
        {
            case ModuleRarity.Common: return rarityColors.commonColor;
            case ModuleRarity.Rare: return rarityColors.rareColor;
            case ModuleRarity.Epic: return rarityColors.epicColor;
            case ModuleRarity.Legendary: return rarityColors.legendaryColor;
            case ModuleRarity.Mythic: return rarityColors.mythicColor;
            default: return Color.white;
        }
    }
}