using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class AutoResizeIcons : MonoBehaviour
{
    [Tooltip("Dimensione minima icona (px)")]
    public float minIconSize = 60f;

    [Tooltip("Rapporto massimo rispetto all'altezza del container (es. 0.9 => 90% dell'altezza)")]
    [Range(0.2f, 1f)]
    public float maxHeightRatio = 0.9f;

    [Tooltip("Se true e le icone non entrano nemmeno con minIconSize, abilita scroll se è assegnato uno ScrollRect")]
    public bool allowScrollIfOverflow = true;

    [Tooltip("Se >=0 overridea lo spacing del HorizontalLayoutGroup")]
    public float spacingOverride = -1f;

    [Tooltip("Optional: assegna il ScrollRect se vuoi fallback scroll quando le icone non entrano")]
    public ScrollRect scrollRect = null;

    private HorizontalLayoutGroup layout;
    private RectTransform rect;

    void OnEnable() { InitAndUpdate(); }
    void Start() { InitAndUpdate(); }

    void InitAndUpdate()
    {
        layout = GetComponent<HorizontalLayoutGroup>();
        rect = GetComponent<RectTransform>();
        if (layout == null)
        {
            Debug.LogWarning("[AutoResizeIcons] manca HorizontalLayoutGroup sul GameObject.");
            return;
        }
        // Assicurati che il layout non forzi l'espansione (altrimenti non rispetta preferredWidth)
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        UpdateIconSizes();
    }

    void OnRectTransformDimensionsChange()
    {
        // chiamato quando cambia la larghezza/altezza del container
        UpdateIconSizes();
    }

    public void UpdateIconSizes()
    {
        if (layout == null || rect == null) InitAndUpdate();
        if (layout == null || rect == null) return;

        // raccogli figli attivi
        List<RectTransform> children = new List<RectTransform>();
        foreach (Transform t in transform)
        {
            if (!t.gameObject.activeInHierarchy) continue;
            RectTransform rt = t as RectTransform;
            if (rt != null) children.Add(rt);
        }

        int count = children.Count;
        if (count == 0) return;

        float spacing = (spacingOverride >= 0f) ? spacingOverride : layout.spacing;
        float padding = layout.padding.left + layout.padding.right;
        float containerWidth = rect.rect.width;
        float containerHeight = rect.rect.height;

        // calcolo spazio disponibile:
        float availableWidth = containerWidth - padding - spacing * (count - 1);

        // dimensione ideale = spazio disponibile diviso numero di icone
        float idealSize = availableWidth / count;

        // non vogliamo che l'icona sia più alta del container: limitiamo con maxByHeight
        float maxByHeight = containerHeight * maxHeightRatio;

        float targetSize;
        bool needsScroll = false;

        if (idealSize >= minIconSize)
        {
            // c'è spazio: usa idealSize (ma non oltre maxByHeight)
            targetSize = Mathf.Min(idealSize, maxByHeight);
        }
        else
        {
            // lo spazio non basta: usa minIconSize; se ancora non entra, fallback scroll
            targetSize = minIconSize;
            float requiredWidth = count * targetSize + spacing * (count - 1) + padding;
            if (requiredWidth > containerWidth)
            {
                needsScroll = true;
            }
        }

        // Applica la dimensione tramite LayoutElement ai figli (così LayoutGroup la rispetta)
        foreach (RectTransform child in children)
        {
            LayoutElement le = child.GetComponent<LayoutElement>();
            if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = Mathf.Floor(targetSize);   // arrotondo per sicurezza
            le.preferredHeight = Mathf.Floor(targetSize);
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;
        }

        // Forza rebuild del layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        // Gestione fallback scroll: se necessario e se hai assegnato un ScrollRect lo espando
        if (needsScroll && allowScrollIfOverflow && scrollRect != null)
        {
            float requiredWidth = count * targetSize + spacing * (count - 1) + padding;
            // assegna la larghezza del content (ScrollRect.content dovrebbe essere questo stesso RectTransform o il suo wrapper)
            RectTransform contentRT = scrollRect.content;
            if (contentRT != null)
            {
                contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredWidth);
                // abilita scrolling orizzontale
                scrollRect.horizontal = true;
                // opzionale: disabilita vertical scrolling
                scrollRect.vertical = false;
            }
        }
        else if (scrollRect != null)
        {
            // ripristina il content alla larghezza del container (comportamento normale)
            RectTransform contentRT = scrollRect.content;
            if (contentRT != null)
                contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth);
        }
    }
}
