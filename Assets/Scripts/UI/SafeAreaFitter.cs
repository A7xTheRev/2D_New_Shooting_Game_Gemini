// Nome File: SafeAreaFitter.cs
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    // Potremmo anche chiamarlo in Update se l'area sicura potesse cambiare dinamicamente (es. rotazione schermo)
    // Ma per ora, all'avvio è sufficiente.
    /*
    void Update()
    {
        if (Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea();
        }
    }
    */

    /// <summary>
    /// Applica i margini della Safe Area al RectTransform di questo pannello.
    /// </summary>
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Converte il rettangolo della safe area (in pixel) in ancore normalizzate (0-1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // Assicurati di non dividere per zero se lo schermo non è ancora pronto
        if (Screen.width == 0 || Screen.height == 0)
        {
            return;
        }

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Applica le ancore calcolate al pannello
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        Debug.Log($"Safe Area applicata. Ancore Min: {anchorMin}, Ancore Max: {anchorMax}");
    }
}