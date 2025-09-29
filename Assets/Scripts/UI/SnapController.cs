using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class SnapController : MonoBehaviour
{
    public float snapSpeed = 10f;
    
    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private List<RectTransform> listItems;
    private Action<int> onSelectionChangedCallback; 
    
    private bool isSnapping = false;
    private int nearestItemIndex = -1;
    private bool hasBeenInitialized = false;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if(scrollRect != null)
        {
        contentPanel = scrollRect.content;
            var eventTrigger = gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = gameObject.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) => { OnEndDrag(); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void Update()
    {
        if (isSnapping && hasBeenInitialized)
        {
            if (Input.GetMouseButton(0))
            {
                isSnapping = false;
                return;
            }

            if (listItems == null || nearestItemIndex < 0 || nearestItemIndex >= listItems.Count)
            {
                isSnapping = false;
                return;
            }

            float targetX = -listItems[nearestItemIndex].anchoredPosition.x;
            Vector2 targetPosition = new Vector2(targetX, contentPanel.anchoredPosition.y);
            contentPanel.anchoredPosition = Vector2.Lerp(contentPanel.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            if (Vector2.Distance(contentPanel.anchoredPosition, targetPosition) < 1f)
            {
                contentPanel.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    public void OnEndDrag()
    {
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
        
        if (newNearestIndex != nearestItemIndex)
        {
            nearestItemIndex = newNearestIndex;
            // Chiama la funzione che ci è stata passata, chiunque essa sia
            onSelectionChangedCallback?.Invoke(nearestItemIndex); 
        }
        
        isSnapping = true;
    }

    // --- METODO INITIALIZE AGGIORNATO ---
    // Ora accetta un 'startingIndex' opzionale per partire da un elemento specifico.
    public void Initialize(List<RectTransform> items, Action<int> callback, int startingIndex = 0)
    {
        listItems = items;
        onSelectionChangedCallback = callback;
        hasBeenInitialized = true;
        
        // Controlla che l'indice sia valido
        if (startingIndex < 0 || startingIndex >= items.Count)
        {
            startingIndex = 0;
        }

            nearestItemIndex = startingIndex;
        
        // **LA CORREZIONE FONDAMENTALE È QUI**
        // Forziamo il pannello a posizionarsi istantaneamente sull'elemento iniziale.
        // Questo viene eseguito una sola volta, quindi non c'è bisogno di aspettare il prossimo frame.
        Canvas.ForceUpdateCanvases();
        if (listItems != null && startingIndex < listItems.Count)
        {
        contentPanel.anchoredPosition = new Vector2(-listItems[startingIndex].anchoredPosition.x, contentPanel.anchoredPosition.y);
        }

        // Notifica subito il listener per aggiornare la UI (descrizioni, highlight, etc.)
        onSelectionChangedCallback?.Invoke(nearestItemIndex);
    }
}