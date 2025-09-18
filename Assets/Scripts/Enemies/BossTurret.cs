using UnityEngine;

// Ora richiede anche un EnemyStats, che sarà il nostro gestore della salute
[RequireComponent(typeof(EnemyStats))]
public class BossTurret : MonoBehaviour
{
    // NON CI SONO PIU' VARIABILI PER LA SALUTE QUI

    [Header("Sistema di Fuoco")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float fireTimer;
    public bool canShoot = false;

    [Header("Riferimenti")]
    private Transform playerTransform;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Spara solo se il boss glielo permette
        if (!canShoot) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Shoot();
            fireTimer = fireRate;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null) return;
        Vector2 direction = (playerTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Instantiate(projectilePrefab, firePoint.position, rotation);
    }
}