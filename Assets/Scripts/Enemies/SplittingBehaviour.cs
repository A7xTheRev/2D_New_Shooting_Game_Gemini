using UnityEngine;

// Questo script NON muove l'oggetto. Aggiunge solo la capacità di "dividersi".
public class SplittingBehavior : MonoBehaviour
{
    [Header("Impostazioni Divisione")]
    [Tooltip("Il prefab del nemico più piccolo da generare.")]
    public GameObject minionPrefab;
    [Tooltip("Quanti mini-nemici generare.")]
    public int numberOfMinions = 3;

    public void Split()
    {
        if (minionPrefab == null)
        {
            Debug.LogError("Minion Prefab non assegnato su " + gameObject.name);
            return;
        }

        for (int i = 0; i < numberOfMinions; i++)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            Instantiate(minionPrefab, spawnPos, Quaternion.identity);
        }
    }
}