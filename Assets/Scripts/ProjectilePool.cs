using UnityEngine;
using System.Collections.Generic;

// Classe interna per raggruppare il prefab e la sua coda
[System.Serializable]
public class WeaponProjectilePool
{
    public string weaponName; // Nome dell'arma a cui è associato questo tipo di proiettile
    public GameObject projectilePrefab;
    public int poolSize = 20;
    public Queue<GameObject> projectileQueue = new Queue<GameObject>();

    // Metodo per inizializzare la coda
    public void Initialize(Transform parentTransform)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = GameObject.Instantiate(projectilePrefab, parentTransform);
            obj.SetActive(false);
            projectileQueue.Enqueue(obj);
        }
    }

    // Metodo per ottenere un proiettile
    public GameObject GetProjectile()
    {
        if (projectileQueue.Count == 0)
        {
            // Crea un nuovo proiettile al volo se il pool è vuoto
            GameObject newObj = GameObject.Instantiate(projectilePrefab, parentTransform);
            newObj.SetActive(false);
            projectileQueue.Enqueue(newObj); // Lo aggiunge e poi lo dequeua
            Debug.LogWarning($"ProjectilePool for {weaponName} was empty. Increased pool size dynamically. Consider increasing initial poolSize.");
        }
        
        GameObject proj = projectileQueue.Dequeue();
        proj.SetActive(true);
        return proj;
    }

    // Metodo per restituire un proiettile
    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.SetParent(parentTransform);
        projectileQueue.Enqueue(projectile);
    }

    private Transform parentTransform; // Per mantenere il riferimento al genitore del pool
    public void SetParentTransform(Transform parent)
    {
        parentTransform = parent;
    }
}


// Gestione di un pool di proiettili per ottimizzare le prestazioni
public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [Header("Pools per Tipo di Arma")]
    public List<WeaponProjectilePool> weaponPools = new List<WeaponProjectilePool>();

    private Dictionary<string, WeaponProjectilePool> poolDictionary = new Dictionary<string, WeaponProjectilePool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inizializza tutte le pool definite nell'editor
        foreach (var pool in weaponPools)
        {
            pool.SetParentTransform(transform); // Passa il riferimento al genitore
            pool.Initialize(transform);
            poolDictionary.Add(pool.weaponName, pool);
        }
    }

    // Metodo pubblico per ottenere un proiettile in base al nome dell'arma
    public GameObject GetProjectileForWeapon(string weaponName)
    {
        if (poolDictionary.TryGetValue(weaponName, out WeaponProjectilePool pool))
        {
            return pool.GetProjectile();
        }
        Debug.LogError($"No projectile pool found for weapon: {weaponName}");
        return null;
    }

    // Metodo pubblico per restituire un proiettile (il proiettile stesso conosce il suo pool)
    public void ReturnProjectile(string weaponName, GameObject projectile)
    {
        if (poolDictionary.TryGetValue(weaponName, out WeaponProjectilePool pool))
        {
            pool.ReturnProjectile(projectile);
        }
        else
        {
            Debug.LogError($"Cannot return projectile for weapon {weaponName}. Pool not found.");
            // Fallback: distrugge l'oggetto se non può essere restituito
            Destroy(projectile); 
        }
    }
}