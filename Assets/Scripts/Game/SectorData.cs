using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Sector Data", menuName = "Game Data/Sector Data")]
public class SectorData : ScriptableObject
{
    [Header("Informazioni Settore")]
    public string sectorName;
    public int numberOfLevels = 5;

    [Header("Contenuti del Settore")]
    [Tooltip("La lista dei prefab dei nemici che possono apparire in questo settore.")]
    public List<GameObject> availableEnemies; // Modificato da EnemyData a GameObject
    [Tooltip("La lista dei prefab dei nemici Elite per questo settore.")]
    public List<GameObject> availableElites; // Modificato da EnemyData a GameObject
    [Tooltip("Il prefab del Super Boss alla fine del settore.")]
    public GameObject guardianBossPrefab;

    [Header("Aspetto Visivo")]
    [Tooltip("La texture di sfondo per questo settore.")]
    public Texture2D backgroundTexture;
}