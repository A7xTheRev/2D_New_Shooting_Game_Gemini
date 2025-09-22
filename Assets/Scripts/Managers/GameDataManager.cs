using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    // Qui salveremo l'arma scelta dal giocatore nel menu
    public WeaponData selectedWeapon;

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