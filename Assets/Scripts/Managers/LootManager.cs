using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Prefab Monete")]
    public GameObject coinBronzePrefab; // Valore 1
    public GameObject coinSilverPrefab; // Valore 10
    public GameObject coinGoldPrefab;   // Valore 50

    [Header("Prefab Gemme")]
    public GameObject gemBluePrefab;    // Valore 1
    public GameObject gemGreenPrefab;   // Valore 5
    public GameObject gemYellowPrefab;  // Valore 20

    [Header("Prefab Utilità")]
    public GameObject healthPickupPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}