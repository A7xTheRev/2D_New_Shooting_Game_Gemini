using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificativo")]
    [Tooltip("Il nome dell'arma, deve corrispondere a quello usato nei pool e nei menu (es. 'Standard', 'Laser', 'Missile').")]
    public string weaponName;

    [Header("Informazioni UI")]
    [TextArea]
    public string description; 

    [Header("Statistiche di Base")]
    [Tooltip("Moltiplicatore del danno base del giocatore (1 = 100%).")]
    public float damageMultiplier = 1f;
    [Tooltip("Moltiplicatore della velocità dei proiettili (1 = 100%).")]
    public float projectileSpeedMultiplier = 1f;
    [Tooltip("Raggio del danno ad area all'impatto (0 = nessun danno ad area).")]
    public float areaDamageRadius = 0f;
    [Tooltip("Il tag del VFX di impatto da usare per questa arma (deve esistere nel VFXPool).")]
    public string impactVFXTag;
}