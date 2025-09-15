using UnityEngine;

// Definisce i tipi di statistiche che possiamo potenziare
public enum PermanentUpgradeType
{
    Health,
    Damage,
    AttackSpeed,
    MoveSpeed,
    AbilityPower
}

[System.Serializable]
public class PermanentUpgrade
{
    public string upgradeName;
    public PermanentUpgradeType upgradeType;
    public int baseCost = 50; // Costo per il primo livello
    public float costMultiplier = 1.5f; // Di quanto aumenta il costo a ogni livello
    public float bonusPerLevel = 5f; // Il bonus fornito da ogni livello
    public int maxLevel = 10;
    
    [HideInInspector] // Questo campo non apparirà nell'editor, è gestito dal codice
    public int currentLevel = 0;

    // Metodo per calcolare il costo del prossimo livello
    public int GetNextLevelCost()
    {
        if (currentLevel >= maxLevel) return int.MaxValue; // Se è al massimo, il costo è "infinito"
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }
}