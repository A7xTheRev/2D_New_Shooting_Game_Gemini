using UnityEngine;

// L'enum GameMode è stato rimosso da qui perché vive nel suo file GameMode.cs

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    // Dati persistenti per la sessione di gioco
    public WeaponData selectedWeapon;
    public ShipData selectedShip;
    public GameMode selectedGameMode;
    
    // Ora abbiamo riferimenti sia al Mondo che al Settore
    public WorldData selectedWorld;
    public SectorData selectedSector;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // RIMOSSA DA QUI
        }
        else
        {
            Destroy(gameObject);
        }
    }
}