// Nome File: RarityDropChance.cs
using UnityEngine;

[System.Serializable]
public class RarityDropChance
{
    public ModuleRarity rarity;

    [Tooltip("Peso del drop. Un valore più alto aumenta la probabilità rispetto agli altri elementi nella stessa lista.")]
    [Range(0.1f, 100f)]
    public float weight;
}