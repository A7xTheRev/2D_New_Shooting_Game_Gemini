using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System; // Aggiunto per poter usare "Action"

[RequireComponent(typeof(ScrollRect))]
public class SnapController : MonoBehaviour
{
    public float snapSpeed = 10f;
    
    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private List<RectTransform> listItems;

    // Callback generica per notificare il cambio di selezione a chiunque lo usi
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
            // Aggiunge il listener per l'evento OnEndDrag direttamente da codice
            var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
            entry.callback.AddListener((data) => { OnEndDrag(); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void Update()
    {
        if (isSnapping && hasBeenInitialized)
        {
            if (Input.GetMouseButton(0)) // Usa GetMouseButton per un controllo più fluido
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

    // Metodo di inizializzazione aggiornato per accettare la callback
    public void Initialize(List<RectTransform> items, Action<int> callback)
    {
        listItems = items;
        onSelectionChangedCallback = callback;
        hasBeenInitialized = true;
        
        // Forza un aggiornamento iniziale per centrare il primo elemento
        // e notificare il listener
        nearestItemIndex = -1; // Forza l'aggiornamento
        OnEndDrag();
    }
}