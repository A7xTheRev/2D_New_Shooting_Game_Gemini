using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Punto di sparo")]
    public Transform firePoint;
    public float fireCooldown = 0.5f;

    [Header("Proiettili extra")]
    public float projectileAngleStep = 15f;
    
    [Header("Impostazioni Rallentatore")]
    [Range(0.1f, 1f)]
    public float slowMotionFactor = 0.2f;
    public float slowMotionDelay = 1f;
    public UnityEngine.UI.Image slowMotionVignette;

    private PlayerStats stats;
    private Camera cam;
    private ProjectilePool projectilePool;
    private AbilityController abilityController;
    
    private float timeSinceLastTouch = 0f;
    private Color vignetteColor;

    [Header("Statistiche Arma Attiva")]
    public float currentWeaponDamageMultiplier = 1f;
    public float currentWeaponSpeedMultiplier = 1f;
    public float currentWeaponAreaDamage = 0f;
    private string currentWeaponImpactTag; 

    private string currentWeaponName;
    private float fireTimer;
    
    private bool ignoreInputThisFrame = false;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        cam = Camera.main;
        abilityController = GetComponent<AbilityController>();
        ApplyWeaponStats(MenuManager.GetSelectedWeapon());
        if (slowMotionVignette != null) { vignetteColor = slowMotionVignette.color; }
    }

    void Start()
    {
        projectilePool = ProjectilePool.Instance;
        ApplyWeaponStats(MenuManager.GetSelectedWeapon());
    }

    void OnEnable() 
    { 
        MenuManager.OnWeaponChanged += ApplyWeaponStats; 
        Time.timeScale = 1f;
    }
    void OnDisable() 
    { 
        Time.timeScale = 1f; 
        MenuManager.OnWeaponChanged -= ApplyWeaponStats; 
    }

    void Update()
    {
        if (Time.timeScale > 0f && ignoreInputThisFrame)
        {
            ignoreInputThisFrame = false;
            return;
        }
        
        if (Time.timeScale == 0f)
        {
            ignoreInputThisFrame = true;
        }

        HandleMovementAndAbility();
        HandleShooting();
        // LA CHIAMATA A HandleHomingMissiles() È STATA RIMOSSA
    }
    
    void HandleMovementAndAbility()
    {
        if (Time.timeScale == 0f) return;
        bool isInputDown = false;
        Vector2 screenPoint = Vector2.zero;
        #if UNITY_EDITOR
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) { isInputDown = true; screenPoint = Input.mousePosition; }
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject()) { if (abilityController != null) { abilityController.ActivateAbility(); } }
        #else
        if (Input.touchCount > 0) { Touch touch = Input.GetTouch(0); if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) { if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) { isInputDown = true; screenPoint = touch.position; } if (touch.phase == TouchPhase.Ended) { if (abilityController != null) { abilityController.ActivateAbility(); } } } }
        #endif
        if (isInputDown) { transform.position = Vector2.Lerp(transform.position, cam.ScreenToWorldPoint(screenPoint), stats.moveSpeed * Time.deltaTime); timeSinceLastTouch = 0f; Time.timeScale = 1f; if (slowMotionVignette != null) { vignetteColor.a = Mathf.Lerp(vignetteColor.a, 0f, Time.deltaTime * 10f); slowMotionVignette.color = vignetteColor; } }
        else { timeSinceLastTouch += Time.unscaledDeltaTime; if (timeSinceLastTouch >= slowMotionDelay) { Time.timeScale = slowMotionFactor; if (slowMotionVignette != null) { vignetteColor.a = Mathf.Lerp(vignetteColor.a, 0.5f, Time.deltaTime * 10f); slowMotionVignette.color = vignetteColor; } } }
    }

    void HandleShooting()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireCooldown / stats.attackSpeed;
        }
    }

    void Shoot()
    {
        if (projectilePool == null || firePoint == null || string.IsNullOrEmpty(currentWeaponName)) return;
        
        AudioManager.Instance.PlaySound(AudioManager.Instance.playerShootSound);

        int count = stats.projectileCount;
        if (count == 1) { SpawnProjectile(Vector2.up, firePoint.position); }
        else { float startAngle = -projectileAngleStep * (count - 1) / 2f; for (int i = 0; i < count; i++) { float angle = startAngle + i * projectileAngleStep; Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up; SpawnProjectile(dir, firePoint.position); } }
    }

    void SpawnProjectile(Vector2 direction, Vector3 position)
    {
        GameObject projGameObject = projectilePool.GetProjectileForWeapon(currentWeaponName);
        if (projGameObject == null) return;
        projGameObject.transform.position = position;
        projGameObject.transform.rotation = firePoint.rotation;
        projGameObject.transform.localScale = Vector3.one * stats.projectileSizeMultiplier;
        
        Projectile p = projGameObject.GetComponent<Projectile>();
        if (p != null) 
        { 
            p.baseDamage = stats.damage; 
            p.damageMultiplier = currentWeaponDamageMultiplier; 
            p.speed *= currentWeaponSpeedMultiplier; 
            p.areaDamageRadius = currentWeaponAreaDamage;
            p.impactVFXTag = currentWeaponImpactTag; 
            p.SetOwner(stats); 
            p.Launch(direction); 
        }
    }

    private void ApplyWeaponStats(string weapon)
    {
        currentWeaponName = weapon;
        switch (weapon)
        {
            case "Laser": 
                currentWeaponDamageMultiplier = 0.7f; 
                currentWeaponSpeedMultiplier = 1.5f; 
                currentWeaponAreaDamage = 0f;
                currentWeaponImpactTag = "LaserImpact";
                break;

            case "Missile": 
                currentWeaponDamageMultiplier = 1f; 
                currentWeaponSpeedMultiplier = 0.8f; 
                currentWeaponAreaDamage = 1f; 
                currentWeaponImpactTag = "MissileImpact";
                break;

            default:
            case "Standard": 
                currentWeaponDamageMultiplier = 1f; 
                currentWeaponSpeedMultiplier = 1f; 
                currentWeaponAreaDamage = 0f; 
                currentWeaponImpactTag = "StandardImpact";
                break;
        }
    }
}