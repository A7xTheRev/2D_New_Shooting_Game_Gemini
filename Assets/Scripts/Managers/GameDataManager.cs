using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    // Dati persistenti per la sessione di gioco
    public WeaponData selectedWeapon;
    public ShipData selectedShip; // Aggiunto per coerenza futura
    public GameMode selectedGameMode;
    public SectorData selectedSector;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}