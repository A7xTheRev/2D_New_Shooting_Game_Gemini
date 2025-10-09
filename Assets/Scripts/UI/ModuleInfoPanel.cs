// Nome File: ModuleInfoPanel.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening; // Aggiunto per le animazioni

public class ModuleInfoPanel : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI moduleNameText;
    public TextMeshProUGUI rarityText;
    public Image moduleIcon;
    public Image rarityBorder;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI statsText;
    public Button closeButton;

    // --- NUOVO: Riferimenti ai pulsanti di azione ---
    [Header("Pulsanti di Azione")]
    public Button equipButton;
    public Button unequipButton;
    public Button fuseButton;
    
    [Header("Animazione")]
    public CanvasGroup canvasGroup;
    public RectTransform panelTransform;
    public float animationDuration = 0.3f;

    private ModuleInventoryItem rarityColors; 

    void Awake()
    {
        // Aggiunge la funzionalità al pulsante di chiusura
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        // Inizializza i componenti per l'animazione
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (panelTransform == null) panelTransform = GetComponent<RectTransform>();
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
    /// Mostra il pannello e lo configura in base al contesto.
    /// </summary>
    /// <param name="moduleData">I dati del modulo da mostrare.</param>
    /// <param name="onEquip">L'azione da eseguire quando si preme "Equipaggia". Se null, il pulsante viene nascosto.</param>
    /// <param name="onUnequip">L'azione da eseguire quando si preme "Rimuovi". Se null, il pulsante viene nascosto.</param>
    /// <param name="onFuse">L'azione da eseguire quando si preme "Fondi". Se null, il pulsante viene nascosto.</param>
    public void Show(ModuleData moduleData, Action onEquip, Action onUnequip, Action onFuse)
    {
        if (moduleData == null) return;

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

        // Configura i pulsanti di azione
        // Usa una "lambda expression" per assicurarsi che il pannello si chiuda dopo l'azione
        
        // Pulsante Equipaggia
        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(onEquip != null);
            if (onEquip != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(() => { onEquip.Invoke(); Hide(); });
            }
        }

        // Pulsante Rimuovi
        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(onUnequip != null);
            if (onUnequip != null)
            {
                unequipButton.onClick.RemoveAllListeners();
                unequipButton.onClick.AddListener(() => { onUnequip.Invoke(); Hide(); });
            }
        }
        
        // Pulsante Fondi
        if (fuseButton != null)
        {
            fuseButton.gameObject.SetActive(onFuse != null);
            if (onFuse != null)
            {
                fuseButton.onClick.RemoveAllListeners();
                fuseButton.onClick.AddListener(() => { onFuse.Invoke(); Hide(); });
            }
        }
        
        // Avvia l'animazione di apertura
        AnimateShow();
    }

    /// <summary>
    /// Nasconde il pannello con un'animazione.
    /// </summary>
    public void Hide()
    {
        AnimateHide();
    }
    
    // --- NUOVI METODI PER L'ANIMAZIONE ---
    private void AnimateShow()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        panelTransform.localScale = Vector3.one * 0.9f;

        canvasGroup.DOFade(1, animationDuration).SetEase(Ease.OutQuad);
        panelTransform.DOScale(1, animationDuration).SetEase(Ease.OutBack);
    }

    private void AnimateHide()
    {
        canvasGroup.DOFade(0, animationDuration).SetEase(Ease.InQuad);
        panelTransform.DOScale(0.9f, animationDuration).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    // --- FINE NUOVI METODI ---

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