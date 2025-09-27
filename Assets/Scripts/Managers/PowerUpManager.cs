using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    // --- NUOVA VARIABILE PER LA CARTELLA ---
    [Header("Impostazioni Editor")]
    [Tooltip("IT: La cartella principale che contiene tutti gli asset dei potenziamenti (inclusi quelli nelle sottocartelle).")]
    public Object powerUpsFolder; // Usiamo Object per poterci trascinare una cartella
    // --- FINE NUOVA VARIABILE ---
    
    public List<PowerUpEffect> allPowerUps;

    public List<PowerUpEffect> GetRandomPowerUps(int count, PlayerStats player)
    {
        List<PowerUpEffect> eligiblePowerUps = new List<PowerUpEffect>();

        foreach (PowerUpEffect powerUp in allPowerUps)
        {
            if (powerUp == null)
            {
                Debug.LogWarning("Trovato uno slot vuoto nella lista 'allPowerUps' del PowerUpManager. Controlla l'Inspector!");
                continue;
            }

            bool isEligible = true;

            // 1. Controllo Prerequisiti
            if (powerUp.hasPrerequisite)
            {
                if (powerUp.prerequisite == null || !player.acquiredPowerUps.Contains(powerUp.prerequisite.type))
                {
                    isEligible = false;
                }
            }

            // 2. Controllo Unicità
            if (isEligible && powerUp.isUnique)
            {
                if (player.acquiredPowerUps.Contains(powerUp.type))
                {
                    isEligible = false;
                }
            }

            // 3. NUOVO CONTROLLO: Esclusione Reciproca
            if (isEligible)
            {
            bool isExcluded = false;
                // Controlla se QUESTO potenziamento è incompatibile con qualcosa che il giocatore ha già
            if (powerUp.mutuallyExclusivePowerUps != null && powerUp.mutuallyExclusivePowerUps.Count > 0)
            {
                // Controlla se il giocatore ha già un potenziamento che è nella lista di esclusione di QUESTO potenziamento.
                foreach (PowerUpEffect excludedPowerUp in powerUp.mutuallyExclusivePowerUps)
                {
                        if (excludedPowerUp != null && player.acquiredPowerUps.Contains(excludedPowerUp.type))
                        {
                            isExcluded = true;
                            break; 
                        }
                    }
                }
                
                // Controlla anche il contrario: se il giocatore ha un potenziamento che esclude QUESTO
                foreach(PowerUpType acquiredType in player.acquiredPowerUps)
                {
                    PowerUpEffect acquiredEffect = allPowerUps.Find(p => p.type == acquiredType);
                    if(acquiredEffect != null && acquiredEffect.mutuallyExclusivePowerUps != null && acquiredEffect.mutuallyExclusivePowerUps.Contains(powerUp))
                    {
                        isExcluded = true;
                        break; 
                    }
                }

                if (isExcluded)
                {
                    isEligible = false;
                }
            }

            if (isEligible)
            {
                eligiblePowerUps.Add(powerUp);
            }

            // Se ha superato tutti i controlli, è eleggibile
            eligiblePowerUps.Add(powerUp);
        }
        
        // --- FINE LOGICA AGGIORNATA ---

        // Logica di estrazione casuale (invariata)
        List<PowerUpEffect> options = new List<PowerUpEffect>();
        List<PowerUpEffect> tempList = new List<PowerUpEffect>(eligiblePowerUps);

        for (int i = 0; i < count && tempList.Count > 0; i++)
        {
            int index = Random.Range(0, tempList.Count);
            options.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return options;
    }
}