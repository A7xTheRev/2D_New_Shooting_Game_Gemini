using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // 👈 serve per DoTween

[ExecuteAlways]
public class AutoResizeIconsBounce : MonoBehaviour
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

    [Header("Animazione")]
    public bool enableBounce = true;
    public float bounceDuration = 0.3f;
    public float bounceOvershoot = 1.1f;

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
            Debug.LogWarning("[AutoResizeIconsBounce] manca HorizontalLayoutGroup sul GameObject.");
            return;
        }

        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        UpdateIconSizes();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateIconSizes();
    }

    public void UpdateIconSizes()
    {
        if (layout == null || rect == null) InitAndUpdate();
        if (layout == null || rect == null) return;

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

        float availableWidth = containerWidth - padding - spacing * (count - 1);
        float idealSize = availableWidth / count;
        float maxByHeight = containerHeight * maxHeightRatio;

        float targetSize;
        bool needsScroll = false;

        if (idealSize >= minIconSize)
        {
            targetSize = Mathf.Min(idealSize, maxByHeight);
        }
        else
        {
            targetSize = minIconSize;
            float requiredWidth = count * targetSize + spacing * (count - 1) + padding;
            if (requiredWidth > containerWidth)
                needsScroll = true;
        }

        // Applica la dimensione + effetto bounce 👇
        foreach (RectTransform child in children)
        {
            LayoutElement le = child.GetComponent<LayoutElement>();
            if (le == null) le = child.gameObject.AddComponent<LayoutElement>();

            float oldW = le.preferredWidth;
            float newW = Mathf.Floor(targetSize);

            le.preferredWidth = newW;
            le.preferredHeight = newW;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // 👇 Bounce animation con DoTween
            if (enableBounce && Application.isPlaying)
            {
                child.DOKill(); // cancella eventuali tween precedenti
                float scaleTarget = bounceOvershoot; // leggermente più grande
                child.localScale = Vector3.one; // reset scala base
                child.DOScale(scaleTarget, bounceDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        child.DOScale(1f, bounceDuration * 0.5f).SetEase(Ease.OutBack);
                    });
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        if (needsScroll && allowScrollIfOverflow && scrollRect != null)
        {
            float requiredWidth = count * targetSize + spacing * (count - 1) + padding;
            RectTransform contentRT = scrollRect.content;
            if (contentRT != null)
            {
                contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredWidth);
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
            }
        }
        else if (scrollRect != null)
        {
            RectTransform contentRT = scrollRect.content;
            if (contentRT != null)
                contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, containerWidth);
        }
    }
}
