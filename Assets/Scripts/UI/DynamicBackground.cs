using UnityEngine;
using UnityEngine.UI;

// Assicura che questo script sia sempre applicato a un oggetto con un componente Image
[RequireComponent(typeof(Image))]
public class DynamicBackground : MonoBehaviour
{
    private Image backgroundImage;

    void Awake()
    {
        // Trova il componente Image su questo stesso oggetto
        backgroundImage = GetComponent<Image>();
    }
    
    // --- NUOVI METODI ---
    // Quando questo oggetto diventa attivo, inizia ad ascoltare i cambiamenti.
    void OnEnable()
    {
        if (BackgroundManager.Instance != null)
        {
            BackgroundManager.OnBackgroundChanged += UpdateBackground;
        }
    }

    // Quando viene disattivato, smette di ascoltare per evitare errori.
    void OnDisable()
    {
        if (BackgroundManager.Instance != null)
        {
            BackgroundManager.OnBackgroundChanged -= UpdateBackground;
        }
    }
    // --- FINE NUOVI METODI ---

    void Start()
    {
        // Chiama il manager per impostare lo sfondo corretto all'avvio della scena
        UpdateBackground();
    }

    // Questo metodo può essere chiamato anche da altri script se necessario
    public void UpdateBackground()
    {
        // Controlla se il BackgroundManager esiste
        if (BackgroundManager.Instance != null)
        {
            // Chiedi al manager quale texture usare
            Texture2D currentTex = BackgroundManager.Instance.GetCurrentBackgroundTexture();
            
            if (currentTex != null && backgroundImage != null)
            {
                // Converte la Texture2D in uno Sprite e la applica al componente Image
                backgroundImage.sprite = Sprite.Create(currentTex, new Rect(0, 0, currentTex.width, currentTex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}