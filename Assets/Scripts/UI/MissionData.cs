using UnityEngine;

// Definiamo i tipi di obiettivi che una missione può avere
public enum MissionType
{
    KILL_ENEMIES_TOTAL,      // Uccidi un numero totale di nemici (qualsiasi tipo)
    KILL_ENEMIES_OF_TYPE,    // Uccidi un numero di nemici di un tipo specifico
    COLLECT_COINS_TOTAL,     // Raccogli un numero totale di monete (in più partite)
    SURVIVE_MINUTES_ENDLESS, // Sopravvivi per X minuti in una singola partita in modalità Endless
    COMPLETE_SECTOR          // Completa un settore specifico
}

[CreateAssetMenu(fileName = "Mission_New", menuName = "Game Data/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identificativo Unico")]
    [Tooltip("Un ID testuale unico per questa missione (es. 'kill_100_kamikaze'). Non cambiarlo dopo averlo impostato!")]
    public string missionID;

    [Header("Informazioni UI")]
    public string title;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Obiettivo")]
    public MissionType missionType;
    public int targetValue; // L'obiettivo numerico (es. 1000 uccisioni, 600 secondi)

    [Tooltip("Usato solo se il MissionType è KILL_ENEMIES_OF_TYPE. Deve corrispondere al nome del prefab del nemico.")]
    public string targetEnemyID; // Es. "Enemy_Kamikaze_Prefab"

    [Header("Ricompensa")]
    [Tooltip("Quante gemme si ottengono al completamento.")]
    public int gemReward;
}