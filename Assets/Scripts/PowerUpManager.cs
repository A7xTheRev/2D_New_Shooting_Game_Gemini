using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    public List<PowerUp> allPowerUps; // Lista di tutti i powerup disponibili

    // Restituisce 'count' powerup casuali dal pool
    public List<PowerUp> GetRandomPowerUps(int count)
    {
        if (allPowerUps == null || allPowerUps.Count == 0)
        {
            Debug.LogWarning("AllPowerUps è vuota!");
            return new List<PowerUp>();
        }

        List<PowerUp> options = new List<PowerUp>();
        List<PowerUp> tempList = new List<PowerUp>(allPowerUps);

        for (int i = 0; i < count && tempList.Count > 0; i++)
        {
            int index = Random.Range(0, tempList.Count);
            options.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        // Debug: stampo le scelte
        Debug.Log("PowerUp scelti: " + string.Join(", ", options.Select(p => p.displayName)));

        return options;
    }
}
