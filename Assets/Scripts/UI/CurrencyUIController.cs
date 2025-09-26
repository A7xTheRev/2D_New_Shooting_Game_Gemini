using UnityEngine;
using TMPro;

public class CurrencyUIController : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;

    // Quando il pannello diventa attivo, inizia ad ascoltare i cambiamenti
    void OnEnable()
    {
        // Si iscrive all'evento del ProgressionManager.
        // Ogni volta che OnValuesChanged viene lanciato, il nostro metodo UpdateDisplay verrà chiamato.
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateDisplay;
        }
        // Aggiorna subito i valori all'attivazione
        UpdateDisplay();
    }

    // Quando il pannello viene disattivato, smette di ascoltare
    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateDisplay;
        }
    }

    // Metodo che aggiorna i testi della UI
    private void UpdateDisplay()
    {
        if (ProgressionManager.Instance == null) return;

        if (coinsText != null)
        {
            coinsText.text = ProgressionManager.Instance.GetCoins().ToString();
        }

        if (gemsText != null)
        {
            gemsText.text = ProgressionManager.Instance.GetSpecialCurrency().ToString();
        }
    }
}