using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

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

    [Header("Pannelli")]
    public ModuleInfoPanel moduleInfoPanel;
    
    // --- MODIFICA: Variabili di stato per la nuova modalità equipaggiamento ---
    private bool isEquipMode = false;
    private string moduleIDToEquip;
    private List<ModuleSlot> spawnedSlots = new List<ModuleSlot>();
    // --- FINE MODIFICA ---
    
    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            ExitEquipMode(); // Assicura di non essere in modalità equipaggiamento
            DrawWorkshopUI();
            if (moduleInfoPanel != null) moduleInfoPanel.Hide(); // --- NEW: Ensure panel is hidden on enable
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
        // --- MODIFICA: Rimossa la riga che causava l'errore ---
        // selectedModuleID = null; 
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
        spawnedSlots.Clear(); // Pulisce la lista degli slot

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
            spawnedSlots.Add(slotScript); // Aggiunge lo slot alla lista
        }
    }

    /// <summary>
    /// Disegna tutti i moduli posseduti nell'inventario.
    /// </summary>
    private void DrawInventory()
    {
        ClearContainer(inventoryItemsContainer);

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
                itemScript.OnInventoryItemClicked += HandleInventoryItemClick;
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

    // --- LOGICA CLICK COMPLETAMENTE RISCRITTA ---

    /// <summary>
    /// Chiamato quando si clicca un item nell'inventario. Apre il pannello dettagli.
    /// </summary>
    private void HandleInventoryItemClick(string moduleID)
    {
        // Se siamo in modalità equipaggiamento, ignora i click sull'inventario
        if (isEquipMode) return;

        ModuleData moduleData = ProgressionManager.Instance.GetModuleDataByID(moduleID);
        if (moduleInfoPanel == null || moduleData == null) return;

        Action onEquip = () => EnterEquipMode(moduleID);
        Action onFuse = null;
        if (ProgressionManager.Instance.GetModuleCount(moduleID) >= 3 && moduleData.fusionResult != null)
        {
            // La logica di fusione ora è gestita da HandleFuseClick, 
            // ma potremmo volerla anche qui nel pannello. Per ora la lasciamo sull'item.
            onFuse = () => {
                ProgressionManager.Instance.FuseModules(moduleID);
                DrawWorkshopUI();
            };
        }
        
        moduleInfoPanel.Show(moduleData, onEquip, null, onFuse);
    }

    /// <summary>
    /// Chiamato quando si clicca uno slot. Apre il pannello dettagli se pieno.
    /// </summary>
    private void HandleSlotClick(ModuleSlotType slotType, int slotIndex)
    {
        // CASO 1: Siamo in Modalità Equipaggiamento
        if (isEquipMode)
        {
            ModuleData moduleToEquip = ProgressionManager.Instance.GetModuleDataByID(moduleIDToEquip);
            if (moduleToEquip != null && moduleToEquip.slotType == slotType)
            {
                // Esegui l'equipaggiamento/swap
                ProgressionManager.Instance.EquipModule(moduleIDToEquip, slotIndex);
                ExitEquipMode();
                DrawWorkshopUI();
            }
        }
        // CASO 2: Non siamo in Modalità Equipaggiamento, apriamo solo i dettagli
        else
        {
            List<string> equippedModules = ProgressionManager.Instance.GetEquippedModules(slotType);
            if (slotIndex < equippedModules.Count && !string.IsNullOrEmpty(equippedModules[slotIndex]))
            {
                string moduleID = equippedModules[slotIndex];
                ModuleData moduleData = ProgressionManager.Instance.GetModuleDataByID(moduleID);
                if (moduleInfoPanel == null || moduleData == null) return;

                // Definisce l'azione di rimozione
                Action onUnequip = () => {
                    ProgressionManager.Instance.UnequipModule(slotType, slotIndex);
                    DrawWorkshopUI();
                };
                
                moduleInfoPanel.Show(moduleData, null, onUnequip, null);
            }
        }
    }

    // --- NUOVI METODI PER LA MODALITÀ EQUIPAGGIAMENTO ---

    private void EnterEquipMode(string moduleID)
    {
        isEquipMode = true;
        moduleIDToEquip = moduleID;

        ModuleData moduleToEquip = ProgressionManager.Instance.GetModuleDataByID(moduleID);
        if (moduleToEquip == null)
        {
            ExitEquipMode();
            return;
        }

        // Evidenzia solo gli slot compatibili
        foreach (var slot in spawnedSlots)
        {
            // La riga seguente è stata corretta per accedere al tipo di slot tramite reflection o un getter pubblico.
            // Poiché ModuleSlot ha il campo `slotType` privato, aggiungiamo un getter.
            // Per ora, assumiamo di aver aggiunto `public ModuleSlotType GetSlotType() { return slotType; }` a ModuleSlot.cs
            // EDIT: Modifico direttamente ModuleSlot.cs per rendere i campi accessibili
            if (slot.GetSlotType() == moduleToEquip.slotType)
            {
                slot.SetHighlight(true);
            }
            else
            {
            slot.SetHighlight(false);
            }
        }
    }

    private void ExitEquipMode()
    {
        isEquipMode = false;
        moduleIDToEquip = null;
        
        // Disattiva tutti gli highlight
        foreach (var slot in spawnedSlots)
        {
            slot.SetHighlight(false);
        }
    }
}