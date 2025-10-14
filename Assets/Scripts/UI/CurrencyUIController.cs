using UnityEngine;
using TMPro;
using DG.Tweening; // Aggiungi la direttiva per usare DOTween

public class CurrencyUIController : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;

    [Header("Impostazioni Animazione")]
    [Tooltip("La durata totale in secondi dell'animazione di conteggio.")]
    public float animationDuration = 1.5f;

    // Variabili per memorizzare i valori attualmente visualizzati
    private int displayedCoins;
    private int displayedGems;

    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            // 1. Leggi i valori attuali (che sono ancora quelli "vecchi" perché questo script
            //    verrà eseguito prima dell'applicazione delle ricompense).
            displayedCoins = ProgressionManager.Instance.GetCoins();
            displayedGems = ProgressionManager.Instance.GetSpecialCurrency();

            // 2. Imposta immediatamente il testo con i valori iniziali.
            if (coinsText != null) coinsText.text = displayedCoins.ToString("N0"); // "N0" formatta il numero, es: 1,000
            if (gemsText != null) gemsText.text = displayedGems.ToString("N0");

            // 3. Iscriviti all'evento per ascoltare i cambiamenti futuri.
            ProgressionManager.OnValuesChanged += UpdateDisplayAnimated;
        }
        else
        {
            // Se il ProgressionManager non è pronto, imposta testi di default
            if (coinsText != null) coinsText.text = "0";
            if (gemsText != null) gemsText.text = "0";
        }
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateDisplayAnimated;
        }

        // Interrompe tutte le animazioni DOTween associate a questo oggetto per sicurezza
        // Questo previene errori se l'oggetto viene disattivato mentre un'animazione è in corso
        transform.DOKill();
    }

    // Questo metodo viene ora chiamato SOLO dall'evento OnValuesChanged
    private void UpdateDisplayAnimated()
    {
        if (ProgressionManager.Instance == null) return;

        // Recupera i nuovi valori target dal manager
        int newCoinsValue = ProgressionManager.Instance.GetCoins();
        int newGemsValue = ProgressionManager.Instance.GetSpecialCurrency();

        // Anima il conteggio delle monete dal valore ATTUALMENTE visualizzato al nuovo valore
        if (coinsText != null && displayedCoins != newCoinsValue)
        {
            // DOTween.To() è un tween generico. Anima un valore da A a B in un dato tempo.
            // Getter: () => displayedCoins -> da dove parte l'animazione
            // Setter: x => { displayedCoins = x; coinsText.text = x.ToString(); } -> cosa fare ad ogni frame
            // End Value: newCoinsValue -> il valore da raggiungere
            // Duration: animationDuration -> la durata
            DOTween.To(() => displayedCoins, x => {
                displayedCoins = x;
                coinsText.text = x.ToString("N0");
            }, newCoinsValue, animationDuration).SetEase(Ease.OutSine);
        }

        // Anima il conteggio delle gemme
        if (gemsText != null && displayedGems != newGemsValue)
        {
            DOTween.To(() => displayedGems, x => {
                displayedGems = x;
                gemsText.text = x.ToString("N0");
            }, newGemsValue, animationDuration).SetEase(Ease.OutSine);
        }
    }
}