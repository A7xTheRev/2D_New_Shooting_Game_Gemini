using UnityEngine;
using System.Collections.Generic;

// L'enum ci servirà in futuro, ma per ora lo lasciamo qui come predisposizione.
public enum WaveArchetype
{
    Mixed // Per ora useremo solo questo
}

[CreateAssetMenu(fileName = "New Sector Data", menuName = "Game Data/Sector Data")]
public class SectorData : ScriptableObject
{
    [Header("Informazioni Settore")]
    public string sectorName;
    public int numberOfWaves = 5;

    [Header("Contenuti del Settore")]
    [Tooltip("La lista dei prefab dei nemici che possono apparire in questo settore.")]
    public List<GameObject> availableEnemies;
    [Tooltip("La lista dei prefab dei nemici Elite per questo settore.")]
    public List<GameObject> availableElites;
    [Tooltip("Il prefab del Super Boss alla fine del settore.")]
    public GameObject guardianBossPrefab;

    [Header("Aspetto Visivo")]
    [Tooltip("La texture di sfondo per questo settore.")]
    public Texture2D backgroundTexture;

    // --- NUOVA SEZIONE PER LE RICOMPENSE DEI MODULI ---
    [Header("Module Rewards (Rarity-Based)")]
    [Tooltip("Definisce le probabilità (pesi) per ogni rarità dei moduli dati come ricompensa.")]
    public List<RarityDropChance> victoryRarityDropChances;
    
    [Tooltip("Quanti moduli vengono assegnati al giocatore alla vittoria.")]
    [Range(0, 10)]
    public int moduleRewardsCount = 1;
    // --- FINE NUOVA SEZIONE ---
}