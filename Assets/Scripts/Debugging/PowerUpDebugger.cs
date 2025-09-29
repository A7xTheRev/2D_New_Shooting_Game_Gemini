using UnityEngine;
using System.Text;

// IT: Aggiungi questo attributo per poter aggiungere il componente al volo dall'editor
[AddComponentMenu("Debugging/PowerUp Tracker Display")]
public class PowerUpDebugger : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        // Trova automaticamente lo script PlayerStats su questo oggetto
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PowerUpDebugger non ha trovato un componente PlayerStats su questo oggetto!");
            enabled = false; // Disattiva lo script se non trova il PlayerStats
        }
    }

    // Update viene chiamato ogni frame
    void Update()
    {
        // Premi il tasto 'P' sulla tastiera per stampare lo stato attuale dei potenziamenti
        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintPowerUpStatus();
        }
    }

    private void PrintPowerUpStatus()
    {
        if (playerStats == null || playerStats.powerUpTracker == null) return;

        // StringBuilder è un modo efficiente per costruire stringhe di testo
        StringBuilder statusText = new StringBuilder();
        statusText.AppendLine("--- STATO POTENZIAMENTI ATTUALE ---");

        if (playerStats.powerUpTracker.Count == 0)
        {
            statusText.AppendLine("Nessun potenziamento acquisito.");
        }
        else
        {
            // Itera attraverso ogni potenziamento contato e lo aggiunge alla stringa
            foreach (var entry in playerStats.powerUpTracker)
            {
                // Esempio di riga: "IncreaseDamage: 2"
                statusText.AppendLine($"{entry.Key}: {entry.Value}");
            }
        }
        
        // Stampa il risultato finale nella console di Unity
        Debug.Log(statusText.ToString());
    }
}