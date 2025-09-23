using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ScrollRect))]
public class SnapController : MonoBehaviour
{
    public ShipSelectorUI shipSelectorUI; // Riferimento al manager principale
    public float snapSpeed = 10f;
    
    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private List<RectTransform> shipItems;
    
    private bool isSnapping = false;
    private int nearestItemIndex = -1;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentPanel = scrollRect.content;
    }

    void Update()
    {
        if (isSnapping)
        {
            // Interrompi lo snap se l'utente tocca di nuovo lo schermo
            if (Input.GetMouseButtonDown(0))
            {
                isSnapping = false;
                return;
            }

            // Calcola la posizione target e muoviti fluidamente
            float targetX = -shipItems[nearestItemIndex].anchoredPosition.x;
            Vector2 targetPosition = new Vector2(targetX, contentPanel.anchoredPosition.y);
            contentPanel.anchoredPosition = Vector2.Lerp(contentPanel.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            // Se siamo abbastanza vicini, ferma lo snap
            if (Vector2.Distance(contentPanel.anchoredPosition, targetPosition) < 1f)
            {
                contentPanel.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    // Questo metodo viene chiamato quando l'utente finisce di trascinare
    public void OnEndDrag()
    {
        float minDistance = float.MaxValue;
        int newNearestIndex = 0;

        for (int i = 0; i < shipItems.Count; i++)
        {
            // Calcola la distanza di ogni navicella dal centro dello schermo
            float distance = Mathf.Abs(contentPanel.anchoredPosition.x + shipItems[i].anchoredPosition.x);
            if (distance < minDistance)
            {
                minDistance = distance;
                newNearestIndex = i;
            }
        }
        
        // Se la navicella più vicina è cambiata, avvisa il manager
        if (newNearestIndex != nearestItemIndex)
        {
            nearestItemIndex = newNearestIndex;
            if (shipSelectorUI != null)
            {
                shipSelectorUI.UpdateUIForShipIndex(nearestItemIndex);
            }
        }
        
        isSnapping = true;
    }

    // Metodo per inizializzare il controller con le navicelle create
    public void Initialize(List<RectTransform> items)
    {
        shipItems = items;
        // Simula un "drag" iniziale per centrare la prima nave
        OnEndDrag();
    }
}