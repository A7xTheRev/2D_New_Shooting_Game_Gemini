using UnityEngine;
using System.Collections.Generic;

public class MissionsUI : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Il prefab della singola riga di missione (con lo script MissionUIEntry).")]
    public GameObject missionEntryPrefab;
    [Tooltip("Il container (es. il Content di uno ScrollView) dove verranno create le righe.")]
    public Transform contentContainer;

    void OnEnable()
    {
        // Iscriviti all'evento del ProgressionManager per aggiornarti automaticamente
        ProgressionManager.OnValuesChanged += PopulateMissions;
        PopulateMissions();
    }

    void OnDisable()
    {
        // Annulla l'iscrizione quando il pannello viene disattivato
        ProgressionManager.OnValuesChanged -= PopulateMissions;
    }

    public void PopulateMissions()
    {
        if (ProgressionManager.Instance == null) return;

        // Pulisci la lista precedente
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Prendi la lista di tutte le missioni
        List<MissionData> allMissions = ProgressionManager.Instance.allMissions;

        // Per ogni missione, crea un elemento UI e configuralo
        foreach (MissionData mission in allMissions)
        {
            GameObject entryObj = Instantiate(missionEntryPrefab, contentContainer);
            MissionUIEntry entryUI = entryObj.GetComponent<MissionUIEntry>();
            if (entryUI != null)
            {
                entryUI.Setup(mission);
            }
        }
    }
}