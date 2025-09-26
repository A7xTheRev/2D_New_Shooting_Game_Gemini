using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SectorButtonUI : MonoBehaviour
{
    [Header("Riferimenti Base")]
    public Button button;
    public TextMeshProUGUI sectorNameText;

    [Header("Stato Bloccato")]
    public GameObject lockIndicator; // Es. un'immagine di un lucchetto

    [Header("Stato Sbloccato (Stelle)")]
    public GameObject starsContainer; // L'oggetto che contiene le immagini delle stelle
    public List<Image> starImages;    // La lista delle 3 immagini per le stelle

    public Sprite starFilledSprite;
    public Sprite starEmptySprite;

    public void Setup(string name, bool isUnlocked, int starCount)
    {
        sectorNameText.text = name;
        button.interactable = isUnlocked;

        if (isUnlocked)
        {
            // Se sbloccato, nascondi il lucchetto e mostra le stelle
            if (lockIndicator != null) lockIndicator.SetActive(false);
            if (starsContainer != null) starsContainer.SetActive(true);

            // Aggiorna le immagini delle stelle in base al conteggio
            for (int i = 0; i < starImages.Count; i++)
            {
                if (i < starCount)
                {
                    starImages[i].sprite = starFilledSprite;
                }
                else
                {
                    starImages[i].sprite = starEmptySprite;
                }
            }
        }
        else
        {
            // Se bloccato, mostra il lucchetto e nascondi le stelle
            if (lockIndicator != null) lockIndicator.SetActive(true);
            if (starsContainer != null) starsContainer.SetActive(false);
        }
    }
}