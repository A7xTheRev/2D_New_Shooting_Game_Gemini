using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Impostazioni Rallentatore")]
    [Range(0.1f, 1f)]
    public float slowMotionFactor = 0.2f;
    public float slowMotionDelay = 1f;

    private PlayerStats stats;
    private Camera cam;
    private ProjectilePool projectilePool;
    private AbilityController abilityController;
    
    private float timeSinceLastTouch = 0f;

    [Header("Punto di sparo")]
    public Transform firePoint;
    public float fireCooldown = 0.5f;

    [Header("Proiettili extra")]
    public float projectileAngleStep = 15f;

    [Header("Moltiplicatori Arma")]
    public float currentWeaponDamageMultiplier = 1f;
    public float currentWeaponSpeedMultiplier = 1f;
    public float currentWeaponAreaDamage = 0f;

    private string currentWeaponName;
    private float fireTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        cam = Camera.main;
        abilityController = GetComponent<AbilityController>();
        ApplyWeaponStats(MenuManager.GetSelectedWeapon());
    }

    void Start()
    {
        projectilePool = ProjectilePool.Instance;
        ApplyWeaponStats(MenuManager.GetSelectedWeapon());
    }

    void OnEnable()
    {
        MenuManager.OnWeaponChanged += ApplyWeaponStats;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        HandleMovementAndAbility();
        HandleShooting();
    }
    
    // --- METODO COMPLETAMENTE RISCRITTO ---
    void HandleMovementAndAbility()
    {
        if (Time.timeScale == 0f) return;

        bool isInputDown = false;

        #if UNITY_EDITOR
        // --- GESTIONE INPUT SU PC ---
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // MOVIMENTO: Se il mouse è premuto e non sulla UI
            isInputDown = true;
            Vector2 screenPoint = Input.mousePosition;
            transform.position = Vector2.Lerp(transform.position, cam.ScreenToWorldPoint(screenPoint), stats.moveSpeed * Time.deltaTime);
        }

        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // ABILITÀ: Se il mouse viene RILASCIATO e non sulla UI
            if (abilityController != null)
            {
                abilityController.ActivateAbility();
            }
        }
        #else
        // --- GESTIONE INPUT SU MOBILE ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                // MOVIMENTO: Se c'è un tocco e non è sulla UI
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    isInputDown = true;
                    Vector2 screenPoint = touch.position;
                    transform.position = Vector2.Lerp(transform.position, cam.ScreenToWorldPoint(screenPoint), stats.moveSpeed * Time.deltaTime);
                }

                // ABILITÀ: Se il tocco viene RILASCIATO e non sulla UI
                if (touch.phase == TouchPhase.Ended)
                {
                    if (abilityController != null)
                    {
                        abilityController.ActivateAbility();
                    }
                }
            }
        }
        #endif

        // --- GESTIONE SLOW-MOTION ---
        if (isInputDown)
        {
            // Se c'è un input valido, il tempo è normale e il timer si resetta
            timeSinceLastTouch = 0f;
            Time.timeScale = 1f;
        }
        else
        {
            // Se non c'è input, parte il timer per lo slow-motion
            timeSinceLastTouch += Time.unscaledDeltaTime;
            if (timeSinceLastTouch >= slowMotionDelay)
            {
                Time.timeScale = slowMotionFactor;
            }
        }
    }

    void HandleShooting()
    {
        if (Time.timeScale < 0.5f) return;
        
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
        
        int count = stats.projectileCount;
        if (count == 1)
        {
            SpawnProjectile(Vector2.up, firePoint.position);
        }
        else
        {
            float startAngle = -projectileAngleStep * (count - 1) / 2f;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * projectileAngleStep;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                SpawnProjectile(dir, firePoint.position);
            }
        }
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
                break;
            case "Standard":
                currentWeaponDamageMultiplier = 1f;
                currentWeaponSpeedMultiplier = 1f;
                currentWeaponAreaDamage = 0f;
                break;
            case "Missile":
                currentWeaponDamageMultiplier = 1f;
                currentWeaponSpeedMultiplier = 0.8f;
                currentWeaponAreaDamage = 1f;
                break;
        }
    }
}