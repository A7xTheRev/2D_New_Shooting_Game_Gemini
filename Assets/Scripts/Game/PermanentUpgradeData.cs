using UnityEngine;

[CreateAssetMenu(fileName = "NewPermanentUpgrade", menuName = "My Game/Permanent Upgrade")]
public class PermanentUpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public Sprite icon;
    [TextArea]
    public string description;

    [Header("Logic")]
    public PermanentUpgradeType upgradeType;

    [Header("Progression")]
    public int baseCost = 50;
    public float costMultiplier = 1.5f;
    public float bonusPerLevel = 5f;
    public int maxLevel = 10;

    public int GetCostForLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return int.MaxValue;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }
}
