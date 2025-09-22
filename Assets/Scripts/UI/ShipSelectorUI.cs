using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShipSelectorUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image shipPreviewImage;
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipPlaystyleText; // --- NUOVO RIFERIMENTO ---
    public TextMeshProUGUI shipDescriptionText;
    public Button nextButton;
    public Button prevButton;
    
    [Header("Sezione Acquisto/Stato")]
    public GameObject actionButtonContainer; // Contenitore del pulsante
    public Button actionButton; // Pulsante che può essere "Acquista" o "Selezionato"
    public TextMeshProUGUI actionButtonText; // Testo del pulsante

    private int currentShipIndex = 0;
    private List<ShipData> allShips;

    void Start()
    {
        // Ottieni la lista di tutte le navicelle dal ProgressionManager
        if (ProgressionManager.Instance != null)
        {
            allShips = ProgressionManager.Instance.allShips;
        }

        // Collega i metodi ai pulsanti
        nextButton.onClick.AddListener(CycleNextShip);
        prevButton.onClick.AddListener(CyclePreviousShip);
        actionButton.onClick.AddListener(ActionButtonClicked);

        // Imposta la visualizzazione sulla navicella attualmente equipaggiata
        ShipData equippedShip = ProgressionManager.Instance.GetEquippedShip();
        if (equippedShip != null)
        {
            currentShipIndex = allShips.IndexOf(equippedShip);
        }

        UpdateUI();
    }

    private void CycleNextShip()
    {
        currentShipIndex++;
        if (currentShipIndex >= allShips.Count)
        {
            currentShipIndex = 0;
        }
        UpdateUI();
    }

    private void CyclePreviousShip()
    {
        currentShipIndex--;
        if (currentShipIndex < 0)
        {
            currentShipIndex = allShips.Count - 1;
        }
        UpdateUI();
    }

    // Unico metodo che gestisce il click del pulsante
    private void ActionButtonClicked()
    {
        ShipData currentShip = allShips[currentShipIndex];
        bool isUnlocked = ProgressionManager.Instance.IsShipUnlocked(currentShip.shipName);

        if (isUnlocked)
        {
            // Se è sbloccata, il click la equipaggia
            ProgressionManager.Instance.SetEquippedShip(currentShip);
        }
        else
        {
            // Se è bloccata, il click prova a comprarla
            ProgressionManager.Instance.UnlockShip(currentShip.shipName);
        }
        UpdateUI(); // Aggiorna la UI dopo l'azione
    }

    private void UpdateUI()
    {
        if (allShips == null || allShips.Count == 0) return;

        ShipData currentShip = allShips[currentShipIndex];
        ShipData equippedShip = ProgressionManager.Instance.GetEquippedShip();

        // Aggiorna le informazioni di base
        shipPreviewImage.sprite = currentShip.shipSprite;
        shipNameText.text = currentShip.shipName;
        shipDescriptionText.text = currentShip.description;

        // --- NUOVA LOGICA PER IL PLAYSTYLE ---
        if (shipPlaystyleText != null)
        {
            shipPlaystyleText.text = "Playstyle: " + currentShip.playstyle;
        }
        // --- FINE NUOVA LOGICA ---

        bool isUnlocked = ProgressionManager.Instance.IsShipUnlocked(currentShip.shipName);

        if (isUnlocked)
        {
            // Se la nave è sbloccata, il pulsante serve a selezionarla
            if (currentShip == equippedShip)
            {
                actionButton.interactable = false;
                actionButtonText.text = "EQUIPPED";
            }
            else
            {
                actionButton.interactable = true;
                actionButtonText.text = "SELECT";
            }
        }
        else
        {
            // Se la nave è bloccata, il pulsante serve a comprarla
            actionButton.interactable = ProgressionManager.Instance.GetSpecialCurrency() >= currentShip.costInGems;
            actionButtonText.text = "UNLOCK (" + currentShip.costInGems + " Gems)";
        }

        prevButton.interactable = allShips.Count > 1;
        nextButton.interactable = allShips.Count > 1;
    }
}