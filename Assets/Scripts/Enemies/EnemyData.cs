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
    [Tooltip("L'icona/sprite di questo nemico che apparirà nel bestiario.")]
    public Sprite enemySprite;
    [Tooltip("Se spuntato, questo nemico avrà una voce nel bestiario.")]
    public bool hasBestiaryEntry = true;
    [Tooltip("Il numero di uccisioni richieste per sbloccare la voce completa.")]
    public int bestiaryKillRequirement = 50;
    [TextArea]
    [Tooltip("La descrizione che apparirà nel bestiario una volta sbloccato.")]
    public string bestiaryDescription;
    [Tooltip("La ricompensa in monete (una tantum) per il completamento.")]
    public int bestiaryCoinReward = 100;
    [Tooltip("La ricompensa in gemme (una tantum) per il completamento.")]
    public int bestiaryGemReward = 5;
    // --- FINE NUOVA SEZIONE ---
}