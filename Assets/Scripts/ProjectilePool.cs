using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    // --- LOGICA SINGLETON AGGIUNTA ---
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
            pool.SetParentTransform(transform);
            pool.Initialize(transform);
            poolDictionary.Add(pool.weaponName, pool);
        }
    }

    public GameObject GetProjectileForWeapon(string weaponName)
    {
        if (poolDictionary.TryGetValue(weaponName, out WeaponProjectilePool pool))
        {
            return pool.GetProjectile();
        }
        Debug.LogError($"Nessun pool di proiettili trovato per l'arma: {weaponName}");
        return null;
    }

    public void ReturnProjectile(string weaponName, GameObject projectile)
    {
        if (poolDictionary.TryGetValue(weaponName, out WeaponProjectilePool pool))
        {
            pool.ReturnProjectile(projectile);
        }
        else
        {
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

    public void SetParentTransform(Transform parent)
    {
        parentTransform = parent;
    }
}