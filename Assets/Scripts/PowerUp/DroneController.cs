using UnityEngine;
using System.Collections.Generic;

public class DroneController : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject dronePrefab;
    private PlayerStats playerStats;
    
    [Header("Posizionamento")]
    public List<Vector2> droneOffsets;

    private List<CombatDrone> activeDrones = new List<CombatDrone>();
    private int lastDroneCount = 0;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        // --- MODIFICA APPLICATA QUI ---
        // Ora controlla la variabile corretta: combatDroneLevel
        int targetDroneCount = playerStats.combatDroneLevel; 
        // --- FINE MODIFICA ---

        if (targetDroneCount != lastDroneCount)
        {
            UpdateDroneCount(targetDroneCount);
            lastDroneCount = targetDroneCount;
        }

        for (int i = 0; i < activeDrones.Count; i++)
        {
            if (activeDrones[i] != null)
            {
                Vector2 targetPosition = (Vector2)transform.position + droneOffsets[i];
                activeDrones[i].SetTargetPosition(targetPosition);
            }
        }
    }

    void UpdateDroneCount(int targetCount)
    {
        while (activeDrones.Count > targetCount)
        {
            Destroy(activeDrones[0].gameObject);
            activeDrones.RemoveAt(0);
        }

        while (activeDrones.Count < targetCount)
        {
            int droneIndex = activeDrones.Count;
            if (droneIndex >= droneOffsets.Count)
            {
                Debug.LogWarning("Non ci sono abbastanza offset definiti nel DroneController per un nuovo drone!");
                break;
            }
            GameObject droneObj = Instantiate(dronePrefab, transform.position + (Vector3)droneOffsets[droneIndex], Quaternion.identity);
            CombatDrone drone = droneObj.GetComponent<CombatDrone>();
            if (drone != null)
            {
                drone.Initialize(playerStats, droneOffsets[droneIndex]);
                activeDrones.Add(drone);
            }
        }
    }
}