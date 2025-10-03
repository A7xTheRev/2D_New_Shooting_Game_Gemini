using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BestiaryUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Transform entryContainer;
    public GameObject bestiaryEntryPrefab;

    [Header("Configurazione Dati")]
    [Tooltip("Trascina qui TUTTI gli asset EnemyData che devono apparire nel bestiario.")]
    public List<EnemyData> allBestiaryEnemies;

    void OnEnable()
    {
        PopulateBestiary();
    }

    void PopulateBestiary()
    {
        // Pulisci le voci vecchie
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        // Ordina i nemici per requisito di uccisioni (o per nome, come preferisci)
        var sortedEnemies = allBestiaryEnemies.Where(e => e.hasBestiaryEntry).OrderBy(e => e.bestiaryKillRequirement).ToList();

        foreach (EnemyData enemy in sortedEnemies)
        {
            GameObject entryObj = Instantiate(bestiaryEntryPrefab, entryContainer);
            BestiaryEntryUI entryUI = entryObj.GetComponent<BestiaryEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(enemy);
            }
        }
    }
}