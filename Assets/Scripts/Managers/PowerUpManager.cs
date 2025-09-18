using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    public List<PowerUp> allPowerUps; // Lista di tutti i powerup disponibili nel gioco

    // Metodo modificato per essere più intelligente
    public List<PowerUp> GetRandomPowerUps(int count, PlayerStats player)
    {
        // 1. Crea una lista di potenziamenti "eleggibili"
        List<PowerUp> eligiblePowerUps = new List<PowerUp>();

        // 2. Controlla ogni potenziamento disponibile nel gioco
        foreach (PowerUp powerUp in allPowerUps)
        {
            bool isEligible = true;

            // --- Logica dei Prerequisiti ---
            // Se il potenziamento ha un prerequisito...
            if (powerUp.hasPrerequisite)
            {
                // ...controlla se il giocatore ha ottenuto quel prerequisito.
                // Se non ce l'ha, il potenziamento non è eleggibile.
                if (!player.acquiredPowerUps.Contains(powerUp.prerequisite))
                {
                    isEligible = false;
                }
            }

            // --- Logica dei Potenziamenti Unici ---
            // Se il potenziamento è unico (può essere preso una sola volta)...
            if (powerUp.isUnique)
            {
                // ...controlla se il giocatore lo ha già.
                // Se lo ha già, non è più eleggibile.
                if (player.acquiredPowerUps.Contains(powerUp.type))
                {
                    isEligible = false;
                }
            }

            // 3. Se il potenziamento ha superato tutti i controlli, aggiungilo alla lista
            if (isEligible)
            {
                eligiblePowerUps.Add(powerUp);
            }
        }

        // 4. Ora, estrai a caso dalla lista degli eleggibili
        List<PowerUp> options = new List<PowerUp>();
        List<PowerUp> tempList = new List<PowerUp>(eligiblePowerUps);

        for (int i = 0; i < count && tempList.Count > 0; i++)
        {
            int index = Random.Range(0, tempList.Count);
            options.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return options;
    }
}