using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    [Header("Impostazioni Editor")]
    [Tooltip("IT: La cartella principale che contiene tutti gli asset dei potenziamenti (inclusi quelli nelle sottocartelle).")]
    public Object powerUpsFolder;
    
    public List<PowerUpEffect> allPowerUps;

    public List<PowerUpEffect> GetRandomPowerUps(int count, PlayerStats player)
    {
        List<PowerUpEffect> eligiblePowerUps = new List<PowerUpEffect>();

        // Crea una lista dei potenziamenti che il giocatore ha già.
        List<PowerUpEffect> acquiredEffects = new List<PowerUpEffect>();
        foreach (PowerUpType type in player.acquiredPowerUps)
        {
            PowerUpEffect effect = allPowerUps.Find(p => p != null && p.type == type);
            if (effect != null)
            {
                acquiredEffects.Add(effect);
            }
        }

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

            // 3. Controllo Esclusione Reciproca
            if (isEligible)
            {
            bool isExcluded = false;
                // Controlla se QUESTO potenziamento è escluso da qualcosa che il giocatore HA.
                foreach (PowerUpEffect acquired in acquiredEffects)
                {
                    if (acquired.mutuallyExclusivePowerUps != null && acquired.mutuallyExclusivePowerUps.Contains(powerUp))
            {
                        isExcluded = true;
                        break;
                    }
                }
                
                // Controlla anche il contrario: se qualcosa che il giocatore HA è escluso da QUESTO potenziamento.
                 if (!isExcluded && powerUp.mutuallyExclusivePowerUps != null)
                 {
                    foreach(PowerUpEffect excluded in powerUp.mutuallyExclusivePowerUps)
                {
                        if(player.acquiredPowerUps.Contains(excluded.type))
                        {
                            isExcluded = true;
                            break; 
                        }
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
        }
        
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