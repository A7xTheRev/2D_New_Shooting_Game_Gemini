using UnityEngine;
using System.Collections.Generic;

// Gestione di un pool di proiettili per ottimizzare le prestazioni
public class ProjectilePool : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject projectilePrefab;
    public int poolSize = 20;

    private Queue<GameObject> projectileQueue = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab);
            obj.SetActive(false);
            projectileQueue.Enqueue(obj);
        }
    }

    public GameObject GetProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject proj;
        if (projectileQueue.Count > 0) proj = projectileQueue.Dequeue();
        else proj = Instantiate(projectilePrefab);

        proj.transform.position = position;
        proj.transform.rotation = rotation;
        proj.SetActive(true);
        return proj;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectileQueue.Enqueue(projectile);
    }
}