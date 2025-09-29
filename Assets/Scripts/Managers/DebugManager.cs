using UnityEngine;

public class DebugManager : MonoBehaviour
{
    // Creiamo un Singleton per accedervi facilmente da qualsiasi script
    public static DebugManager Instance { get; private set; }

    [Header("IT: Interruttori per i Log")]
    [Tooltip("IT: Se spuntato, mostra i log relativi al progresso delle missioni.")]
    public bool showMissionLogs = true;
    
    [Tooltip("IT: Se spuntato, mostra i log relativi ai progressi dei settori.")]
    public bool showSectorProgressLogs = true;

    // --- NUOVO INTERRUTTORE ---
    [Tooltip("IT: Se spuntato, mostra i log ogni volta che un potenziamento viene acquisito.")]
    public bool showPowerUpAcquisitionLogs = true;
    // --- FINE NUOVO INTERRUTTORE ---

    [Tooltip("IT: Se spuntato, aggiunge il debugger dei potenziamenti al giocatore (premi 'P' in gioco per vedere i log).")]
    public bool enablePowerUpTracker = false;
    // In futuro, potremmo aggiungere altri interruttori qui
    // public bool showCombatLogs = false;
    // public bool showSpawningLogs = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}