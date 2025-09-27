using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpButtonUI : MonoBehaviour
{
    [Header("Riferimenti UI del Pulsante")]
    public Image powerUpIcon; // L'immagine per l'icona
    public TextMeshProUGUI powerUpNameText; // Il testo per il nome
    public TextMeshProUGUI powerUpDescriptionText; // Il testo per la descrizione

    // Questo metodo popola il pulsante con i dati di un power-up
    public void Setup(PowerUpEffect powerUp)
    {
        if (powerUpIcon != null)
        {
            powerUpIcon.sprite = powerUp.icon;
            // Se non c'è un'icona assegnata nel PowerUpManager, nascondi l'immagine
            powerUpIcon.gameObject.SetActive(powerUp.icon != null);
        }

        if (powerUpNameText != null)
        {
            powerUpNameText.text = powerUp.displayName;
        }

        if (powerUpDescriptionText != null)
        {
            powerUpDescriptionText.text = powerUp.description;
        }
    }
}