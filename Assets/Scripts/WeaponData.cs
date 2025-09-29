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
    public Sprite weaponIcon;

    [Header("IT: Riferimenti di Gioco")]
    [Tooltip("IT: Il prefab del proiettile che questa arma spara.")]
    public GameObject projectilePrefab; // <-- NUOVO CAMPO FONDAMENTALE

    [Header("Statistiche di Fuoco")]
    [Tooltip("Colpi al secondo. La velocità d'attacco del giocatore agirà come moltiplicatore di questo valore.")]
    public float fireRate = 1.2f;
    [Tooltip("Numero di proiettili sparati in una singola raffica.")]
    public int projectileCount = 1;
    [Tooltip("Angolo di dispersione (in gradi) se si spara più di un proiettile.")]
    public float spreadAngle = 0f;

    [Header("Statistiche dei Proiettili")]
    [Tooltip("Moltiplicatore del danno base del giocatore (1 = 100%).")]
    public float damageMultiplier = 1f;
    [Tooltip("Moltiplicatore della velocità dei proiettili (1 = 100%).")]
    public float projectileSpeedMultiplier = 1f;
    [Tooltip("Raggio del danno ad area all'impatto (0 = nessun danno ad area).")]
    public float areaDamageRadius = 0f;
    [Tooltip("Il tag del VFX di impatto da usare (deve esistere nel VFXPool).")]
    public string impactVFXTag;
    [Tooltip("Quanti nemici può attraversare un proiettile (0 = si ferma al primo).")]
    public int pierceCount = 0;
    [Tooltip("Secondi prima che il proiettile scompaia da solo.")]
    public float projectileLifetime = 5f;
}