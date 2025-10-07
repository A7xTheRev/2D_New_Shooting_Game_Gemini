using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class TabMenuController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string name;
        public Button button;
        public RectTransform panel;
        [HideInInspector] public CanvasGroup canvasGroup;
        [HideInInspector] public Image iconImage;
    }

    [Header("Tabs Settings")]
    public Tab[] tabs;
    [Tooltip("Indice del tab da mostrare di default all'avvio")]
    public int defaultTab = 0;

    [Header("Animazioni")]
    public float transitionDuration = 0.5f;
    public float iconScaleSelected = 1.2f;
    public Color selectedColor = Color.white;
    public Color deselectedColor = new Color(1, 1, 1, 0.5f);

    private int currentTab = 0;
    private bool isTransitioning = false;

    void Start()
    {
        // Imposta i tab
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].panel.GetComponent<CanvasGroup>() == null)
                tabs[i].panel.gameObject.AddComponent<CanvasGroup>();
            tabs[i].canvasGroup = tabs[i].panel.GetComponent<CanvasGroup>();

            tabs[i].iconImage = tabs[i].button.GetComponentInChildren<Image>();

            int index = i;
            tabs[i].button.onClick.AddListener(() => OnTabSelected(index));

            // Tutti disattivi all'inizio
            tabs[i].panel.gameObject.SetActive(false);
            tabs[i].canvasGroup.alpha = 0;
            tabs[i].button.transform.localScale = Vector3.one;
            tabs[i].iconImage.color = deselectedColor;
        }

        // 🔹 Mostra tab di default
        defaultTab = Mathf.Clamp(defaultTab, 0, tabs.Length - 1);
        ShowDefaultTab(defaultTab);
    }

    void ShowDefaultTab(int index)
    {
        currentTab = index;
        var tab = tabs[index];

        tab.panel.gameObject.SetActive(true);
        tab.panel.anchoredPosition = Vector2.zero;
        tab.canvasGroup.alpha = 1;
        tab.button.transform.localScale = Vector3.one * iconScaleSelected;
        tab.iconImage.color = selectedColor;
    }

    void OnTabSelected(int newIndex)
    {
        if (newIndex == currentTab || isTransitioning) return;
        StartCoroutine(SwitchTab(newIndex));
    }

    IEnumerator SwitchTab(int newIndex)
    {
        isTransitioning = true;

        RectTransform currentPanel = tabs[currentTab].panel;
        RectTransform nextPanel = tabs[newIndex].panel;
        CanvasGroup currentCG = tabs[currentTab].canvasGroup;
        CanvasGroup nextCG = tabs[newIndex].canvasGroup;

        float direction = (newIndex > currentTab) ? 1 : -1;
        float width = Screen.width;

        nextPanel.anchoredPosition = new Vector2(direction * width, 0);
        nextPanel.gameObject.SetActive(true);
        nextCG.alpha = 0;

        Sequence seq = DOTween.Sequence();
        seq.Join(currentPanel.DOAnchorPosX(-direction * width, transitionDuration).SetEase(Ease.InOutQuad));
        seq.Join(nextPanel.DOAnchorPosX(0, transitionDuration).SetEase(Ease.InOutQuad));
        seq.Join(currentCG.DOFade(0, transitionDuration * 0.8f));
        seq.Join(nextCG.DOFade(1, transitionDuration * 0.8f));

        // Aggiorna icone
        tabs[currentTab].button.transform.DOScale(1f, 0.25f);
        tabs[newIndex].button.transform.DOScale(iconScaleSelected, 0.25f);
        tabs[currentTab].iconImage.DOColor(deselectedColor, 0.25f);
        tabs[newIndex].iconImage.DOColor(selectedColor, 0.25f);

        yield return seq.WaitForCompletion();

        currentPanel.gameObject.SetActive(false);
        isTransitioning = false;
        currentTab = newIndex;
    }
}
