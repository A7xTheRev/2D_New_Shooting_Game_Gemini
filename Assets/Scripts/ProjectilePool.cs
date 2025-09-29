using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    private static ProjectilePool _instance;
    public static ProjectilePool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ProjectilePool>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProjectilePool (Auto-Generated)");
                    _instance = go.AddComponent<ProjectilePool>();
                }
            }
            return _instance;
        }
    }

    [Header("Pools per Tipo di Arma")]
    [Tooltip("IT: Puoi pre-caricare le pool per le armi iniziali qui per ottimizzare il primo sparo. Le altre verranno create al volo.")]
    public List<WeaponProjectilePool> weaponPools = new List<WeaponProjectilePool>();

    private Dictionary<string, WeaponProjectilePool> poolDictionary = new Dictionary<string, WeaponProjectilePool>();

    void Awake()
    {
        // --- LOGICA SINGLETON AGGIUNTA ---
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Inizializza tutte le pool definite nell'editor
        foreach (var pool in weaponPools)
        {
            // Il tuo metodo SetParentTransform non è necessario se passi il transform in Initialize
            pool.Initialize(transform);
            poolDictionary.Add(pool.weaponName, pool);
        }
    }

    // --- METODO GETPROJECTILE AGGIORNATO PER ESSERE DINAMICO ---
    public GameObject GetProjectileForWeapon(WeaponData weaponData)
    {
        // Se non abbiamo una pool per quest'arma, la creiamo al volo!
        if (!poolDictionary.ContainsKey(weaponData.weaponName))
    {
            CreateNewPoolForWeapon(weaponData);
        }

        // Ora che siamo sicuri che la pool esista, la usiamo
        if (poolDictionary.TryGetValue(weaponData.weaponName, out WeaponProjectilePool pool))
        {
            return pool.GetProjectile();
        }

        // Questo errore apparirà solo se la creazione della pool fallisce (es. prefab mancante)
        Debug.LogError($"Non è stato possibile trovare o creare una pool per l'arma: {weaponData.weaponName}");
        return null;
    }

    // --- NUOVO METODO PRIVATO PER LA CREAZIONE DINAMICA ---
    private void CreateNewPoolForWeapon(WeaponData weaponData)
    {
        // Controlla se l'arma ha un proiettile da poter usare per creare la pool
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogError($"L'arma '{weaponData.weaponName}' non ha un prefab di proiettile assegnato nel suo WeaponData!");
            return;
        }

        Debug.Log($"Pool per l'arma '{weaponData.weaponName}' non trovata. Creazione dinamica in corso...");

        // Crea una nuova definizione di pool in memoria
        WeaponProjectilePool newPool = new WeaponProjectilePool
        {
            weaponName = weaponData.weaponName,
            projectilePrefab = weaponData.projectilePrefab,
            poolSize = 20 // Usiamo una dimensione di default per le pool create al volo
        };
        
        // Inizializza la nuova pool e aggiungila al nostro dizionario
        newPool.Initialize(transform);
        poolDictionary.Add(weaponData.weaponName, newPool);
    }

    public void ReturnProjectile(string weaponName, GameObject projectile)
    {
        if (poolDictionary.TryGetValue(weaponName, out WeaponProjectilePool pool))
        {
            pool.ReturnProjectile(projectile);
        }
        else
        {
            // Se per qualche motivo la pool non esiste più, distruggiamo il proiettile
            Destroy(projectile); 
        }
    }
}

// Classe di supporto per organizzare le pool nell'Inspector
[System.Serializable]
public class WeaponProjectilePool
{
    public string weaponName;
    public GameObject projectilePrefab;
    public int poolSize = 30;
    
    private Queue<GameObject> projectileQueue = new Queue<GameObject>();
    private Transform parentTransform;

    public void Initialize(Transform parent)
    {
        parentTransform = parent;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = GameObject.Instantiate(projectilePrefab, parentTransform);
            obj.SetActive(false);
            projectileQueue.Enqueue(obj);
        }
    }

    public GameObject GetProjectile()
    {
        if (projectileQueue.Count == 0)
        {
            GameObject newObj = GameObject.Instantiate(projectilePrefab, parentTransform);
            newObj.SetActive(false);
            projectileQueue.Enqueue(newObj);
        }
        
        GameObject proj = projectileQueue.Dequeue();
        proj.SetActive(true);
        return proj;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.SetParent(parentTransform);
        projectileQueue.Enqueue(projectile);
    }
}