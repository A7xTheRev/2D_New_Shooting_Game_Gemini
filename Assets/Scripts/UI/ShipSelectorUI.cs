using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShipSelectorUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipPlaystyleText;
    public TextMeshProUGUI shipDescriptionText;
    
    [Header("Area di Scorrimento")]
    [Tooltip("Il pannello 'Content' dello Scroll View che contiene le anteprime.")]
    public Transform contentPanel;
    [Tooltip("Il prefab dell'oggetto UI che mostra l'anteprima della nave.")]
    public GameObject shipPreviewPrefab;
    [Tooltip("Il riferimento allo script SnapController sullo Scroll View.")]
    public SnapController snapController;

    [Header("Sezione Acquisto/Stato")]
    public GameObject actionButtonContainer;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private int currentShipIndex = 0;
    private List<ShipData> allShips;
    private List<RectTransform> shipPreviewItems = new List<RectTransform>();

    void Start()
    {
        if (ProgressionManager.Instance != null)
        {
            allShips = ProgressionManager.Instance.allShips;
        }

        // Popola l'area di scorrimento con le anteprime delle navicelle
        PopulateScrollView();

        // Collega il metodo al pulsante di azione
        actionButton.onClick.AddListener(ActionButtonClicked);
        
        // Inizializza lo snap controller, passandogli le anteprime create
        if (snapController != null)
        {
            snapController.Initialize(shipPreviewItems);
        }
    }
    
    void PopulateScrollView()
    {
        // Pulisce eventuali anteprime vecchie
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        shipPreviewItems.Clear();

        // Crea un'anteprima per ogni navicella disponibile
        foreach (ShipData ship in allShips)
        {
            GameObject itemObj = Instantiate(shipPreviewPrefab, contentPanel);
            // Imposta lo sprite corretto sull'anteprima
            itemObj.GetComponent<Image>().sprite = ship.shipSprite;
            shipPreviewItems.Add(itemObj.GetComponent<RectTransform>());
        }
    }

    // Questo metodo viene chiamato dallo SnapController quando la nave al centro cambia
    public void UpdateUIForShipIndex(int index)
    {
        currentShipIndex = index;
        if (allShips == null || currentShipIndex >= allShips.Count) return;

        ShipData currentShip = allShips[currentShipIndex];
        ShipData equippedShip = ProgressionManager.Instance.GetEquippedShip();
        
        // Aggiorna tutti i testi
        shipNameText.text = currentShip.shipName;
        shipPlaystyleText.text = "Playstyle: " + currentShip.playstyle;
        shipDescriptionText.text = currentShip.description;
        
        bool isUnlocked = ProgressionManager.Instance.IsShipUnlocked(currentShip.shipName);

        if (isUnlocked)
        {
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
            actionButton.interactable = ProgressionManager.Instance.GetSpecialCurrency() >= currentShip.costInGems;
            actionButtonText.text = "UNLOCK (" + currentShip.costInGems + " Gems)";
        }
    }

    private void ActionButtonClicked()
    {
        if (allShips == null || currentShipIndex >= allShips.Count) return;

        ShipData currentShip = allShips[currentShipIndex];
        bool isUnlocked = ProgressionManager.Instance.IsShipUnlocked(currentShip.shipName);

        if (isUnlocked)
        {
            ProgressionManager.Instance.SetEquippedShip(currentShip);
        }
        else
        {
            ProgressionManager.Instance.UnlockShip(currentShip.shipName);
        }
        // Richiama l'aggiornamento della UI per riflettere il nuovo stato
        UpdateUIForShipIndex(currentShipIndex);
    }
}