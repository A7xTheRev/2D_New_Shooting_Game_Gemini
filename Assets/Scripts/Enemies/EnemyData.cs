using UnityEngine;

// Questo attributo ci permette di creare nuovi "Enemy Data" dal menu di Unity
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Scaling")]
    public bool allowStatScaling = true;

    [Header("Statistiche base")]
    public int maxHealth = 50;
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public int projectileDamage = 10;
    public float fireRate = 3f;

    [Header("Ricompense e Drop")]
    public int coinReward = 5;
    public int xpReward = 20;
    public int specialCurrencyReward = 0; // Gemme garantite (es. per i boss)

    [Range(0f, 1f)]
    [Tooltip("La probabilità (da 0 a 1) che questo nemico rilasci una gemma alla morte.")]
    public float gemDropChance = 0.05f; // 5% di probabilità
    
    [Range(0f, 1f)]
    [Tooltip("La probabilità (da 0 a 1) che questo nemico rilasci un medikit.")]
    public float healthDropChance = 0.02f; // 2% di probabilità

    [Header("Animazione")]
    public bool hasDeathAnimation = false;

    [Header("Effetto Visivo (Hit)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    [Header("Effetto Visivo (Morte)")]
    public string deathVFXTag = "EnemyExplosion";
    public float deathShakeDuration = 0f;
    public float deathShakeMagnitude = 0f;

    [Header("Effetti di Stato")]
    public GameObject burnVFX;

    // --- NUOVA SEZIONE BESTIARIO ---
    [Header("Bestiario")]
    [Tooltip("L'icona/sprite di questo nemico che apparirà nel codex.")]
    public Sprite enemySprite;
    [Tooltip("Se spuntato, questo nemico avrà una voce nel codex.")]
    public bool hasCodexEntry = true;
    [Tooltip("Il numero di uccisioni richieste per sbloccare la voce completa.")]
    public int codexKillRequirement = 50;
    [TextArea]
    [Tooltip("La descrizione che apparirà nel codex una volta sbloccato.")]
    public string codexDescription;
    [Tooltip("La ricompensa in monete (una tantum) per il completamento.")]
    public int codexCoinReward = 100;
    [Tooltip("La ricompensa in gemme (una tantum) per il completamento.")]
    public int codexGemReward = 5;

    // --- NUOVA SEZIONE PER I DROP DEI MODULI ---
    [Header("Module Drops")]
    [Tooltip("La tabella di loot usata per determinare QUALE modulo droppare.")]
    public LootTable moduleLootTable;
    [Range(0f, 1f)]
    [Tooltip("La probabilità (da 0 a 1) che questo nemico droppi un modulo dalla sua loot table.")]
    public float moduleDropChance = 0.01f; // Es. 1% di base
    // --- FINE NUOVA SEZIONE ---
}