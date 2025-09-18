using UnityEngine;

public enum AbilityID
{
    None,
    LaserBeam,
    EnergyShield,
    SecondChance,
    StartingPowerUp,
    PowerUpReroll
}

public enum ChargeType
{
    Time,
    Kills,
    DamageDealt
}

[CreateAssetMenu(fileName = "New Special Ability", menuName = "Abilities/Special Ability")]
public class SpecialAbility : ScriptableObject
{
    [Header("Identificatore Unico")]
    public AbilityID abilityID;
    public AbilityBehaviorType behaviorType;

    [Header("Informazioni Base")]
    public string abilityName;
    [TextArea]
    public string description;
    public Sprite icon;
    public bool isDefaultAbility = false;

    [Header("Negozio")]
    public int cost = 1;

    [Header("Logica di Carica")]
    public ChargeType chargeType = ChargeType.Time;
    public float maxCharge = 100f;
    public float chargePerSecond = 10f;
    public float chargePerKill = 20f;
    public float chargePerDamage = 0.1f;

    [Header("Effetto dell'Abilità")]
    public GameObject abilityPrefab;
    public float duration = 2f;
    
    [Header("Scaling con le Statistiche")]
    public float damageMultiplier = 5f;
    public float durationBonusPerPower = 0.04f;
}

public enum AbilityBehaviorType { Active, Passive }