using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    private PlayerStats stats;
    private Camera cam;
    private Vector2 touchPosition;
    private ProjectilePool projectilePool;

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
        MenuManager.OnWeaponChanged -= ApplyWeaponStats;
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
            
        #if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Vector2.Lerp(transform.position, mousePos, stats.moveSpeed * Time.deltaTime);
        }
        #else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return;

            Vector2 touchPos = cam.ScreenToWorldPoint(touch.position);
            transform.position = Vector2.Lerp(transform.position, touchPos, stats.moveSpeed * Time.deltaTime);
        }
        #endif
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
        projGameObject.transform.rotation = firePoint.rotation; // Usa la rotazione del punto di sparo

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
        stats.damage = 10;
        stats.attackSpeed = 1f;
        currentWeaponDamageMultiplier = 1f;
        currentWeaponSpeedMultiplier = 1f;
        currentWeaponAreaDamage = 0f;

        switch (weapon)
        {
            case "Laser":
                stats.damage = 10;
                stats.attackSpeed = 2f;
                currentWeaponDamageMultiplier = 0.7f;
                currentWeaponSpeedMultiplier = 1.5f;
                break;
            case "Standard":
                stats.damage = 10;
                stats.attackSpeed = 1f;
                currentWeaponDamageMultiplier = 1f;
                currentWeaponSpeedMultiplier = 1f;
                break;
            case "Missile":
                stats.damage = 20;
                stats.attackSpeed = 0.5f;
                currentWeaponDamageMultiplier = 1f;
                currentWeaponSpeedMultiplier = 0.8f;
                currentWeaponAreaDamage = 1f;
                break;
        }
    }
}