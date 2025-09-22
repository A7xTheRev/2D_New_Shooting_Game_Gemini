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

    [Header("Ricompense")]
    public int coinReward = 5;
    public int xpReward = 20;
    public int specialCurrencyReward = 0;

    [Header("Animazione")]
    public bool hasDeathAnimation = false;

    // --- SEZIONE AGGIUNTA ---
    [Header("Effetto Visivo (Hit)")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    // --- FINE SEZIONE ---
    
    [Header("Effetto Visivo (Morte)")]
    public string deathVFXTag = "EnemyExplosion";
    public float deathShakeDuration = 0f;
    public float deathShakeMagnitude = 0f;

    [Header("Effetti di Stato")]
    public GameObject burnVFX;
}