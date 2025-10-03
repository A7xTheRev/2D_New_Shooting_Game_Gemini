using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CodexUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Transform entryContainer;
    public GameObject codexEntryPrefab;

    [Header("Configurazione Dati")]
    [Tooltip("Trascina qui TUTTI gli asset EnemyData che devono apparire nel codex.")]
    public List<EnemyData> allCodexEnemies;

    void OnEnable()
    {
     if (Application.isPlaying)
        {
            PopulateCodex();
        }
    }

    void PopulateCodex()
    {
        // Pulisci le voci vecchie
        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        var sortedEnemies = allCodexEnemies.Where(e => e.hasCodexEntry).OrderBy(e => e.codexKillRequirement).ThenBy(e => e.name).ToList();

        foreach (EnemyData enemy in sortedEnemies)
        {
            GameObject entryObj = Instantiate(codexEntryPrefab, entryContainer);
            CodexEntryUI entryUI = entryObj.GetComponent<CodexEntryUI>();
            if (entryUI != null)
            {
                entryUI.Setup(enemy);
            }
        }
    }
}