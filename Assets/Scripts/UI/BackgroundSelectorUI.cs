using UnityEngine;
using UnityEngine.UI;

public class BackgroundSelectorUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image backgroundPreview; // Immagine per mostrare l'anteprima
    public Button nextButton;
    public Button prevButton;

    void OnEnable()
    {
        // Aggiungi i listener ai pulsanti
        nextButton.onClick.AddListener(OnNext);
        prevButton.onClick.AddListener(OnPrevious);
        UpdatePreview();
    }

    void OnDisable()
    {
        // Rimuovi i listener per sicurezza
        nextButton.onClick.RemoveListener(OnNext);
        prevButton.onClick.RemoveListener(OnPrevious);
    }

    private void OnNext()
    {
        BackgroundManager.Instance.CycleNextBackground();
        UpdatePreview();
    }

    private void OnPrevious()
    {
        BackgroundManager.Instance.CyclePreviousBackground();
        UpdatePreview();
    }

    // Aggiorna l'anteprima con lo sfondo corrente
    private void UpdatePreview()
    {
        if (backgroundPreview != null && BackgroundManager.Instance != null)
        {
            Texture2D currentTex = BackgroundManager.Instance.GetCurrentBackgroundTexture();
            // Per usare una Texture2D in una Image UI, dobbiamo prima creare uno Sprite
            backgroundPreview.sprite = Sprite.Create(currentTex, new Rect(0, 0, currentTex.width, currentTex.height), new Vector2(0.5f, 0.5f));
        }
    }
}