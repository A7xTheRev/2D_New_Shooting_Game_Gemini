using UnityEngine;

[CreateAssetMenu(fileName = "New Ship Data", menuName = "Game Data/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Informazioni Base")]
    public string shipName;
    [Tooltip("Breve descrizione dello stile di gioco di questa navicella.")]
    public string playstyle;
    [TextArea]
    public string description;

    [Header("Riferimenti")]
    [Tooltip("Lo sprite da mostrare nei menu e nell'interfaccia utente.")]
    public Sprite shipSprite; // Per la UI
    [Tooltip("Il prefab completo della navicella da usare in gioco.")]
    public GameObject shipPrefab; // Il prefab da generare in gioco

    [Header("Statistiche")]
    [Tooltip("La 'scheda' con le statistiche di base di questa navicella.")]
    public PlayerData baseStats;

    [Header("Negozio")]
    public int costInGems = 10;
    public bool isDefaultShip = false;
}