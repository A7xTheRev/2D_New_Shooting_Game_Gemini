using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Questa piccola classe serve per associare un modulo al suo "peso" (probabilità)
// Deve essere [System.Serializable] per essere visibile nell'Inspector di Unity
[System.Serializable]
public class LootDrop
{
    [Tooltip("Il ModuleData che può essere droppato.")]
    public ModuleData moduleData;

    [Tooltip("Peso del drop. Un valore più alto aumenta la probabilità rispetto agli altri elementi nella stessa tabella.")]
    [Range(0.1f, 100f)]
    public float weight;
}

[CreateAssetMenu(fileName = "LootTable_", menuName = "Astro Survivor/Loot Table")]
public class LootTable : ScriptableObject
{
    [Tooltip("La lista di tutti i possibili drop contenuti in questa tabella.")]
    public List<LootDrop> possibleDrops;

    /// <summary>
    /// Estrae un modulo casuale dalla tabella basandosi sui pesi assegnati.
    /// </summary>
    /// <returns>Il ModuleData del modulo estratto, o null se la tabella è vuota.</returns>
    public ModuleData GetRandomDrop()
    {
        if (possibleDrops == null || possibleDrops.Count == 0)
        {
            return null;
        }

        // Calcola il peso totale di tutti i possibili drop
        float totalWeight = possibleDrops.Sum(drop => drop.weight);

        // Genera un numero casuale tra 0 e il peso totale
        float randomValue = Random.Range(0f, totalWeight);

        // Scorre i drop e sottrae il loro peso dal valore casuale.
        // Il primo drop per cui il valore casuale diventa <= 0 è quello scelto.
        foreach (var drop in possibleDrops)
        {
            if (randomValue <= drop.weight)
            {
                return drop.moduleData;
            }
            randomValue -= drop.weight;
        }

        // Fallback nel caso improbabile di errori con i float
        return null;
    }
}