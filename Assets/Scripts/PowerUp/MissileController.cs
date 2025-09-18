using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStats))]
public class MissileController : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject homingMissilePrefab;
    public List<Transform> missileLaunchPoints;
    private PlayerStats stats;

    [Header("Statistiche di Lancio")]
    public float homingMissileBaseCooldown = 4f;
    private float homingMissileTimer;
    private int missileLauncherIndex = 0;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        homingMissileTimer = homingMissileBaseCooldown;
    }

    void Update()
    {
        HandleHomingMissiles();
    }

    void HandleHomingMissiles()
    {
        if (stats.homingMissileLevel <= 0 || missileLaunchPoints == null || missileLaunchPoints.Count == 0) return;

        homingMissileTimer -= Time.deltaTime;
        if (homingMissileTimer <= 0f)
        {
            if (homingMissilePrefab != null)
            {
                for (int i = 0; i < stats.homingMissileCount; i++)
                {
                    Transform launchPoint = missileLaunchPoints[missileLauncherIndex];
                    
                    GameObject missileObj = Instantiate(homingMissilePrefab, launchPoint.position, launchPoint.rotation);
                    missileObj.GetComponent<HomingMissile>()?.SetOwner(stats);

                    missileLauncherIndex = (missileLauncherIndex + 1) % missileLaunchPoints.Count;
                }
            }
            
            float currentCooldown = homingMissileBaseCooldown * stats.homingMissileCooldownMultiplier;
            homingMissileTimer = currentCooldown;
        }
    }
}