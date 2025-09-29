using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // Controlla quale navicella è stata selezionata
        if (ProgressionManager.Instance != null)
        {
            ShipData selectedShip = ProgressionManager.Instance.GetEquippedShip();
            if (selectedShip != null && selectedShip.shipPrefab != null)
            {
                // 1. Crea un'istanza del prefab della navicella scelta
                GameObject playerInstance = Instantiate(selectedShip.shipPrefab, transform.position, Quaternion.identity);
                
                // 2. Trova il suo componente PlayerStats
                PlayerStats stats = playerInstance.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    // 3. Inizializzalo con le statistiche corrette prese dalla ShipData
                    stats.InitializeFromData(selectedShip.baseStats);
                }

                if (DebugManager.Instance != null && DebugManager.Instance.enablePowerUpTracker)
                {
                    // Se sì, aggiunge il componente di debug al giocatore appena creato.
                    playerInstance.AddComponent<PowerUpDebugger>();
                    Debug.Log("PowerUpDebugger aggiunto al giocatore. Premi 'P' per vedere i potenziamenti.");
                }
                Debug.Log("Spawning ship: " + selectedShip.shipName);
            }
            else
            {
                Debug.LogError("Nessuna navicella valida trovata da spawnare!");
            }
        }
    }
}