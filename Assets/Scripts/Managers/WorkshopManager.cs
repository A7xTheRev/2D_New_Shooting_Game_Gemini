using UnityEngine;
using System.Collections.Generic;
using System.Linq; // <-- 1. AGGIUNGI QUESTA RIGA ALL'INIZIO

public class WorkshopManager : MonoBehaviour
{
    [Header("Riferimenti Prefab")]
    [Tooltip("Il prefab per uno slot di equipaggiamento (PF_ModuleSlot).")]
    public GameObject moduleSlotPrefab;
    [Tooltip("Il prefab per un oggetto nell'inventario (PF_ModuleInventoryItem).")]
    public GameObject moduleInventoryItemPrefab;

    [Header("Contenitori UI")]
    [Tooltip("Il genitore dove verranno creati gli slot Offensivi.")]
    public Transform offensiveSlotsContainer;
    [Tooltip("Il genitore dove verranno creati gli slot Difensivi.")]
    public Transform defensiveSlotsContainer;
    [Tooltip("Il genitore dove verranno creati gli slot di Utilità.")]
    public Transform utilitySlotsContainer;
    [Tooltip("Il genitore dove verranno creati gli oggetti dell'inventario.")]
    public Transform inventoryItemsContainer;

    // --- NUOVO: Variabili di stato per l'interattività ---
    private string selectedModuleID = null; // Memorizza l'ID del modulo selezionato nell'inventario
    private List<ModuleInventoryItem> spawnedInventoryItems = new List<ModuleInventoryItem>(); // Lista di tutti gli item UI creati
    // --- FINE NUOVO ---
    
    // Questo metodo viene chiamato ogni volta che l'oggetto viene attivato.
    // Utile per aggiornare la UI quando apriamo il pannello dell'Officina.
    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            DrawWorkshopUI();
        }
        else
        {
            Debug.LogError("ProgressionManager non trovato! Impossibile disegnare la UI dell'Officina.");
        }
    }

    /// <summary>
    /// Metodo principale che orchestra il disegno dell'intera interfaccia.
    /// </summary>
    public void DrawWorkshopUI()
    {
        selectedModuleID = null; // Deseleziona tutto quando si ridisegna
        DrawSlots();
        DrawInventory();
    }

    /// <summary>
    /// Disegna gli slot di equipaggiamento (Offensivi, Difensivi, Utilità).
    /// </summary>
    private void DrawSlots()
    {
        // Pulisce i contenitori prima di ridisegnare per evitare duplicati
        ClearContainer(offensiveSlotsContainer);
        ClearContainer(defensiveSlotsContainer);
        ClearContainer(utilitySlotsContainer);

        // Ottiene il numero di slot sbloccati per ogni tipo dal ProgressionManager
        Dictionary<ModuleSlotType, int> unlockedSlots = ProgressionManager.Instance.GetUnlockedSlotsCount();
        
        // Disegna gli slot per ogni categoria
        DrawSlotsForType(ModuleSlotType.Offensive, unlockedSlots[ModuleSlotType.Offensive], offensiveSlotsContainer);
        DrawSlotsForType(ModuleSlotType.Defensive, unlockedSlots[ModuleSlotType.Defensive], defensiveSlotsContainer);
        DrawSlotsForType(ModuleSlotType.Utility, unlockedSlots[ModuleSlotType.Utility], utilitySlotsContainer);
    }

    /// <summary>
    /// Helper per disegnare gli slot di un tipo specifico.
    /// </summary>
    private void DrawSlotsForType(ModuleSlotType type, int count, Transform container)
    {
        List<string> equippedModules = ProgressionManager.Instance.GetEquippedModules(type);

        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = Instantiate(moduleSlotPrefab, container);
            ModuleSlot slotScript = slotGO.GetComponent<ModuleSlot>();

            // Controlla se c'è un modulo equipaggiato in questo slot
            if (i < equippedModules.Count && !string.IsNullOrEmpty(equippedModules[i]))
            {
                ModuleData equippedModuleData = ProgressionManager.Instance.GetModuleDataByID(equippedModules[i]);
                slotScript.Setup(equippedModuleData, type, i);
            }
            else
            {
                // Lo slot è sbloccato ma vuoto
                slotScript.Setup(null, type, i);
            }

            // --- NUOVO: Iscrizione all'evento di click dello slot ---
            slotScript.OnSlotClicked += HandleSlotClick;
            // --- FINE NUOVO ---
        }
    }

    /// <summary>
    /// Disegna tutti i moduli posseduti nell'inventario.
    /// </summary>
    private void DrawInventory()
    {
        ClearContainer(inventoryItemsContainer);
        spawnedInventoryItems.Clear(); // --- NUOVO: Pulisce la lista degli item UI ---

        // --- 2. MODIFICA QUESTA RIGA ---
        Dictionary<string, int> inventory = ProgressionManager.Instance.GetModuleInventory();

        // Ordina l'inventario per rarità e poi per nome per una visualizzazione più pulita
        var sortedInventory = inventory.Keys
            .Select(id => ProgressionManager.Instance.GetModuleDataByID(id))
            .Where(data => data != null)
            .OrderByDescending(data => data.rarity)
            .ThenBy(data => data.moduleName);

        foreach (ModuleData moduleData in sortedInventory)
        {
            int quantity = ProgressionManager.Instance.GetModuleCount(moduleData.moduleID);
            if (quantity > 0)
            {
                GameObject itemGO = Instantiate(moduleInventoryItemPrefab, inventoryItemsContainer);
                ModuleInventoryItem itemScript = itemGO.GetComponent<ModuleInventoryItem>();
                itemScript.Setup(moduleData, quantity);

                // Iscrizione a entrambi gli eventi dell'item
                itemScript.OnInventoryItemClicked += HandleInventoryItemClick;
                itemScript.OnFuseButtonClicked += HandleFuseClick; // --- NUOVO ---
                spawnedInventoryItems.Add(itemScript);
                // --- FINE NUOVO ---
            }
        }
    }
    
    /// <summary>
    /// Helper per pulire un contenitore da tutti i suoi figli.
    /// </summary>
    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // --- NUOVI METODI PER GESTIRE I CLICK ---

    /// <summary>
    /// Chiamato quando si clicca su un oggetto nell'inventario.
    /// </summary>
    private void HandleInventoryItemClick(string moduleID)
    {
        selectedModuleID = moduleID;

        // Aggiorna la UI per mostrare visivamente quale item è selezionato
        foreach (var item in spawnedInventoryItems)
        {
            if (item.GetModuleID() == moduleID)
            {
                item.Select();
            }
            else
            {
                item.Deselect();
            }
        }
    }

    /// <summary>
    /// Chiamato quando si clicca su uno slot di equipaggiamento.
    /// </summary>
    private void HandleSlotClick(ModuleSlotType slotType, int slotIndex)
    {
        // CASO 1: Stiamo cercando di EQUIPAGGIARE un modulo selezionato
        if (!string.IsNullOrEmpty(selectedModuleID))
        {
            ModuleData moduleToEquip = ProgressionManager.Instance.GetModuleDataByID(selectedModuleID);
            
            // Controlla se il tipo di modulo è compatibile con lo slot
            if (moduleToEquip != null && moduleToEquip.slotType == slotType)
            {
                ProgressionManager.Instance.EquipModule(selectedModuleID, slotIndex);
                DrawWorkshopUI(); // Ridisegna tutto per mostrare il cambiamento
            }
            else
            {
                // Feedback per l'utente (opzionale, es. suono di errore)
                Debug.Log("Slot non compatibile!");
            }
        }
        // CASO 2: Stiamo cercando di DE-EQUIPAGGIARE un modulo
        else 
        {
            ProgressionManager.Instance.UnequipModule(slotType, slotIndex);
            DrawWorkshopUI(); // Ridisegna tutto per mostrare il cambiamento
        }
    }

    // --- NUOVO METODO PER GESTIRE IL CLICK SUL PULSANTE DI FUSIONE ---
    private void HandleFuseClick(string moduleID)
    {
        Debug.Log($"Richiesta di fusione per il modulo: {moduleID}");
        
        // Chiama il metodo del ProgressionManager che fa tutto il lavoro
        bool success = ProgressionManager.Instance.FuseModules(moduleID);

        // Se la fusione è andata a buon fine, ridisegna l'intera UI per mostrare i cambiamenti
        if (success)
        {
            DrawWorkshopUI();
        }
        else
        {
            Debug.LogWarning($"Fusione fallita per {moduleID}. Controlla la logica del ProgressionManager.");
        }
    }
    // --- FINE NUOVI METODI ---
}