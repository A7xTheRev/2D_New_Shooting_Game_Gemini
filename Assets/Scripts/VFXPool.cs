using UnityEngine;
using System.Collections.Generic;

// Questa piccola classe ci aiuta a organizzare i dati nell'Inspector di Unity
[System.Serializable]
public class VFXPoolItem
{
    public string tag;
    public GameObject prefab;
    public int size = 10;
}

public class VFXPool : MonoBehaviour
{
    public static VFXPool Instance;

    public List<VFXPoolItem> pools; // La lista delle pool che definiremo nell'Inspector
    private Dictionary<string, Queue<GameObject>> poolDictionary;

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
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Cicla attraverso tutte le pool definite nell'Inspector
        foreach (VFXPoolItem pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            // Pre-crea ("warm up") gli oggetti per la pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            
            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject GetVFX(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("La Pool con il tag '" + tag + "' non esiste.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];

        // Se la pool è vuota, per sicurezza creiamo un nuovo oggetto al volo
        if (queue.Count == 0)
        {
            VFXPoolItem item = pools.Find(p => p.tag == tag);
            if (item != null)
            {
                GameObject newObj = Instantiate(item.prefab, transform);
                return newObj;
            }
            return null;
        }

        GameObject objectToSpawn = queue.Dequeue();
        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnVFX(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("La Pool con il tag '" + tag + "' non esiste.");
            Destroy(objectToReturn); // Se la pool non esiste, lo distruggiamo per evitare problemi
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}