using UnityEngine;

[CreateAssetMenu(fileName = "New Player Data", menuName = "Game Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Statistiche Base")]
    public int maxHealth = 100;
    public int damage = 12;
    public int abilityPower = 15;
    public float attackSpeed = 1.2f;
    public float moveSpeed = 5f;

    [Header("Statistiche di Combattimento Secondarie")]
    public float critChance = 0f;
    public float critDamageMultiplier = 2f;
    public float projectileSizeMultiplier = 1f;

    [Header("Danno e Invulnerabilità")]
    public float invulnerabilityTime = 1.5f;
    public float startBlinkInterval = 0.2f;
    public float endBlinkInterval = 0.05f;

    [Header("Effetti Visivi al Colpo")]
    public float hitShakeDuration = 0.2f;
    public float hitShakeMagnitude = 0.1f;
}