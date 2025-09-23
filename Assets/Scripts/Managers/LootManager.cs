using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Prefab dei Raccoglibili")]
    public GameObject coinPickupPrefab;
    public GameObject gemPickupPrefab;    // NUOVO
    public GameObject healthPickupPrefab; // NUOVO

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