using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections; // Aggiunto per le Coroutine
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class SnapController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //================================================================================
    // Configurazione Pubblica (Visibile nell'Inspector)
    //================================================================================

    [Header("Configurazione dell'Aggancio (Snap)")]
    [Tooltip("La velocità con cui l'elemento si aggancia al centro. Un valore più alto corrisponde a uno scatto più rapido e deciso.")]
    public float snapSpeed = 10f;

    [Tooltip("La distanza (in pixel) dal punto di aggancio sotto la quale lo snap si considera completato e il movimento si ferma. Un valore piccolo come 1.0 è solitamente sufficiente.")]
    public float stopSnappingDistance = 1f;

    [Header("Debug")]
    [Tooltip("Se spuntato, lo script scriverà dei log di diagnostica nella Console per aiutarti a capire cosa sta facendo (es. quando inizia/finisce un drag, quale elemento viene selezionato).")]
    public bool showDebugLogs = false;

    [Space]
    [Header("Stato Interno (Sola Lettura)")]
    [Tooltip("DEBUG: L'indice dell'elemento attualmente considerato il più vicino al centro.")]
    [SerializeField, Range(-1, 100)] private int _debug_nearestItemIndex = -1;
    [Tooltip("DEBUG: Indica se lo script sta attualmente forzando l'aggancio al centro.")]
    [SerializeField] private bool _debug_isSnapping = false;

    //================================================================================
    // Variabili Private
    //================================================================================

    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private List<RectTransform> listItems;
    private Action<int> onSelectionChangedCallback;

    private bool hasBeenInitialized = false;

    //================================================================================
    // Metodi di Unity (Awake, Update)
    //================================================================================

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            contentPanel = scrollRect.content;
        }
        else
        {
            Debug.LogError("[SnapController] ScrollRect non trovato! Assicurati che lo script sia sullo stesso oggetto dello ScrollRect.");
        }
    }

    void Update()
    {
        if (!_debug_isSnapping || !hasBeenInitialized || Input.GetMouseButton(0))
        {
            return;
        }

        if (listItems == null || _debug_nearestItemIndex < 0 || _debug_nearestItemIndex >= listItems.Count)
        {
            _debug_isSnapping = false;
            return;
        }

        float targetX = -listItems[_debug_nearestItemIndex].anchoredPosition.x;
        Vector2 targetPosition = new Vector2(targetX, contentPanel.anchoredPosition.y);

        contentPanel.anchoredPosition = Vector2.Lerp(contentPanel.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

        if (Vector2.Distance(contentPanel.anchoredPosition, targetPosition) < stopSnappingDistance)
        {
            contentPanel.anchoredPosition = targetPosition;
            _debug_isSnapping = false;
            if (showDebugLogs) Debug.Log("[SnapController] Snap completato.");
        }
    }

    //================================================================================
    // Gestione degli Eventi di Drag
    //================================================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (showDebugLogs) Debug.Log("[SnapController] Inizio Drag.");
        _debug_isSnapping = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (showDebugLogs) Debug.Log("[SnapController] Fine Drag. Calcolo l'elemento più vicino...");

        if (listItems == null || listItems.Count == 0 || !hasBeenInitialized) return;

        float minDistance = float.MaxValue;
        int newNearestIndex = 0;

        for (int i = 0; i < listItems.Count; i++)
        {
            float distance = Mathf.Abs(contentPanel.anchoredPosition.x + listItems[i].anchoredPosition.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                newNearestIndex = i;
            }
        }

        if (newNearestIndex != _debug_nearestItemIndex)
        {
            if (showDebugLogs) Debug.Log($"[SnapController] Nuovo elemento selezionato! Indice: {newNearestIndex}");
            _debug_nearestItemIndex = newNearestIndex;
            onSelectionChangedCallback?.Invoke(_debug_nearestItemIndex);
        }
        else
        {
             if (showDebugLogs) Debug.Log($"[SnapController] L'elemento è rimasto lo stesso (Indice: {newNearestIndex}). Riancoro.");
        }

        scrollRect.velocity = Vector2.zero;
        _debug_isSnapping = true;
    }

    //================================================================================
    // Metodo di Inizializzazione e Coroutine
    //================================================================================

    public void Initialize(List<RectTransform> items, Action<int> callback, int startingIndex = 0)
    {
        listItems = items;
        onSelectionChangedCallback = callback;
        hasBeenInitialized = true;

        if (showDebugLogs) Debug.Log($"[SnapController] Inizializzato con {items.Count} elementi.");

        if (startingIndex < 0 || startingIndex >= items.Count)
        {
            Debug.LogWarning($"[SnapController] Indice di partenza ({startingIndex}) non valido. Imposto a 0.");
            startingIndex = 0;
        }

        _debug_nearestItemIndex = startingIndex;

        StartCoroutine(InitializePositionCoroutine(startingIndex));

        onSelectionChangedCallback?.Invoke(_debug_nearestItemIndex);
    }

    private IEnumerator InitializePositionCoroutine(int startingIndex)
    {
        yield return new WaitForEndOfFrame();

        if (listItems != null && startingIndex < listItems.Count)
        {
            contentPanel.anchoredPosition = new Vector2(-listItems[startingIndex].anchoredPosition.x, contentPanel.anchoredPosition.y);
            if (showDebugLogs) Debug.Log($"[SnapController] Posizione visiva impostata sull'elemento con indice {startingIndex}.");
        }
    }
}
