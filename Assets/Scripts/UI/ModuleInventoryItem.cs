// Nome File: ModuleInventoryItem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Necessario per usare 'Action'

public class ModuleInventoryItem : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image rarityBorder;
    public Image moduleIcon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI statText; // --- NUOVO: Riferimento al testo della statistica ---
    public Button itemButton;
    public GameObject selectionOutline; // Un bordo visivo per mostrare quando l'oggetto è selezionato
    public GameObject fusionReadyIndicator; // --- NUOVO: Riferimento all'icona di fusione ---

    [Header("Colori Rarità")]
    public Color commonColor = Color.white;
    public Color rareColor = Color.cyan;
    public Color epicColor = Color.magenta;
    public Color legendaryColor = Color.yellow;
    public Color mythicColor = Color.red;

    // Evento che notifica il manager quando questo item viene cliccato
    public event Action<string> OnInventoryItemClicked;

    // Stato interno
    private string currentModuleID;

    void Awake()
    {
        // Aggiunge un listener al pulsante che scatena l'evento OnInventoryItemClicked
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => OnInventoryItemClicked?.Invoke(currentModuleID));
        }
        Deselect();
    }

    /// <summary>
    /// Configura l'aspetto e i dati di questo item dell'inventario.
    /// </summary>
    /// <param name="moduleData">I dati del modulo da mostrare.</param>
    /// <param name="quantity">La quantità di questo modulo posseduta.</param>
    public void Setup(ModuleData moduleData, int quantity)
    {
        this.currentModuleID = moduleData.moduleID;
        this.moduleIcon.sprite = moduleData.icon;

        // --- NUOVO: Imposta il testo della statistica ---
        this.statText.text = moduleData.statToModify.ToString();
        
        // Imposta il colore del bordo in base alla rarità
        switch (moduleData.rarity)
        {
            case ModuleRarity.Common: rarityBorder.color = commonColor; break;
            case ModuleRarity.Rare: rarityBorder.color = rareColor; break;
            case ModuleRarity.Epic: rarityBorder.color = epicColor; break;
            case ModuleRarity.Legendary: rarityBorder.color = legendaryColor; break;
            case ModuleRarity.Mythic: rarityBorder.color = mythicColor; break;
        }

        // Mostra la quantità solo se ne possediamo più di uno
        if (quantity > 1)
        {
            quantityText.text = $"x{quantity}";
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }

        // --- NUOVO: Logica per mostrare/nascondere l'icona di fusione ---
        if (fusionReadyIndicator != null)
        {
            bool canFuse = (quantity >= 3 && moduleData.fusionResult != null);
            fusionReadyIndicator.SetActive(canFuse);
        }
        // --- FINE NUOVO ---
    }

    public string GetModuleID()
    {
        return currentModuleID;
    }

    public void Select()
    {
        if (selectionOutline != null) selectionOutline.SetActive(true);
    }

    public void Deselect()
    {
        if (selectionOutline != null) selectionOutline.SetActive(false);
    }
}