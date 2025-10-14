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
    private int displayedCoins = 0;
    private int displayedGems = 0;

    void OnEnable()
    {
        if (ProgressionManager.Instance != null)
        {
            // Imposta i valori iniziali senza animazione
            InitializeDisplay();
            // Poi, iscriviti per ascoltare i futuri cambiamenti
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

    // Imposta i valori iniziali della UI senza animazione
    private void InitializeDisplay()
    {
        if (ProgressionManager.Instance == null) return;

        displayedCoins = ProgressionManager.Instance.GetCoins();
        displayedGems = ProgressionManager.Instance.GetSpecialCurrency();

        if (coinsText != null)
        {
            coinsText.text = displayedCoins.ToString();
        }

        if (gemsText != null)
        {
            gemsText.text = displayedGems.ToString();
        }
    }

    // Aggiorna i testi della UI con un'animazione di conteggio
    private void UpdateDisplayAnimated()
    {
        if (ProgressionManager.Instance == null) return;

        // Recupera i nuovi valori target
        int newCoinsValue = ProgressionManager.Instance.GetCoins();
        int newGemsValue = ProgressionManager.Instance.GetSpecialCurrency();

        // Anima il conteggio delle monete
        if (coinsText != null && displayedCoins != newCoinsValue)
        {
            // DOTween.To() è un tween generico. Anima un valore da A a B in un dato tempo.
            // Getter: () => displayedCoins -> da dove parte l'animazione
            // Setter: x => { displayedCoins = x; coinsText.text = x.ToString(); } -> cosa fare ad ogni frame
            // End Value: newCoinsValue -> il valore da raggiungere
            // Duration: animationDuration -> la durata
            DOTween.To(() => displayedCoins, x => {
                displayedCoins = x;
                coinsText.text = x.ToString();
            }, newCoinsValue, animationDuration).SetEase(Ease.OutSine);
        }

        // Anima il conteggio delle gemme
        if (gemsText != null && displayedGems != newGemsValue)
        {
            DOTween.To(() => displayedGems, x => {
                displayedGems = x;
                gemsText.text = x.ToString();
            }, newGemsValue, animationDuration).SetEase(Ease.OutSine);
        }
    }
}