// Nome File: ModuleSlot.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Necessario per usare 'Action'

public class ModuleSlot : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image slotBackground;
    public Image moduleIcon;
    public GameObject emptySlotIndicator; // Un oggetto "+" o simile da mostrare quando lo slot è vuoto
    public Button slotButton;
    public TextMeshProUGUI statText; // --- NUOVO: Riferimento al testo della statistica ---
    public Image highlightImage; // --- NUOVO: Riferimento a un'immagine per l'highlight ---

    [Header("Colori Tipi di Slot")]
    public Color offensiveColor = new Color(1f, 0.6f, 0.6f, 0.5f); // Rosso semitrasparente
    public Color defensiveColor = new Color(0.6f, 0.8f, 1f, 0.5f); // Blu semitrasparente
    public Color utilityColor = new Color(0.6f, 1f, 0.6f, 0.5f);   // Verde semitrasparente

    // Evento che notifica il manager quando questo slot viene cliccato
    public event Action<ModuleSlotType, int> OnSlotClicked;

    // --- CORRECTED: 'slotType' needs to be declared here as a private field ---
    private ModuleSlotType slotType;
    private int slotIndex;
    private string currentModuleID;

    void Awake()
    {
        // Aggiunge un listener al pulsante che scatena l'evento OnSlotClicked
        if (slotButton != null)
        {
            // --- CORRECTED: The listener now correctly references the private 'slotType' field ---
            slotButton.onClick.AddListener(() => OnSlotClicked?.Invoke(this.slotType, this.slotIndex));
        }
        // --- NUOVO: Assicura che l'highlight sia spento all'inizio ---
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }

    public void Setup(ModuleData moduleData, ModuleSlotType type, int index)
    {
        this.slotType = type;
        this.slotIndex = index;

        // Imposta il colore di sfondo in base al tipo di slot
        switch (type)
        {
            case ModuleSlotType.Offensive: slotBackground.color = offensiveColor; break;
            case ModuleSlotType.Defensive: slotBackground.color = defensiveColor; break;
            case ModuleSlotType.Utility: slotBackground.color = utilityColor; break;
        }

        // Controlla se lo slot è equipaggiato o vuoto
        if (moduleData != null)
        {
            // Slot equipaggiato: mostra l'icona del modulo
            this.currentModuleID = moduleData.moduleID;
            moduleIcon.sprite = moduleData.icon;
            moduleIcon.gameObject.SetActive(true);
            emptySlotIndicator.SetActive(false);

            // --- NUOVO: Mostra la statistica del modulo ---
            statText.text = moduleData.statToModify.ToString();
            statText.gameObject.SetActive(true);
            // --- FINE NUOVO ---
        }
        else
        {
            // Slot vuoto: mostra l'indicatore di slot vuoto
            this.currentModuleID = null;
            moduleIcon.gameObject.SetActive(false);
            emptySlotIndicator.SetActive(true);

            // --- NUOVO: Nasconde il testo ---
            statText.gameObject.SetActive(false);
        }
    }

    // --- NUOVO METODO PER GESTIRE L'HIGHLIGHT ---
    public void SetHighlight(bool isActive)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(isActive);
        }
    }

    // --- NEW: The public getter method, correctly placed ---
    public ModuleSlotType GetSlotType()
    {
        return this.slotType;
    }
}