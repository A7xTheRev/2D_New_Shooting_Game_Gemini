using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    private PlayerStats stats;
    private Camera cam;
    private Vector2 touchPosition;

    [Header("Proiettile")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 0.5f;

    [Header("Proiettili extra")]
    public float projectileAngleStep = 15f;

    [Header("Moltiplicatori Arma")]
    public float currentWeaponDamageMultiplier = 1f;
    public float currentWeaponSpeedMultiplier = 1f;
    public float currentWeaponAreaDamage = 0f;

    private float fireTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        cam = Camera.main;

        // Applica subito l'arma salvata
        ApplyWeaponStats(MenuManager.GetSelectedWeapon());
    }

    void OnEnable()
    {
        // Si iscrive all'evento per aggiornare arma dinamicamente
        MenuManager.OnWeaponChanged += ApplyWeaponStats;
    }

    void OnDisable()
    {
        // Rimuove iscrizione per evitare memory leak
        MenuManager.OnWeaponChanged -= ApplyWeaponStats;
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        // 🔹 Blocca movimento se il puntatore è sopra un bottone/elemento UI
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
                return; // ignora il touch sopra UI

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
        if (projectilePrefab == null || firePoint == null) return;

        int count = stats.projectileCount;

        if (count == 1)
        {
            SpawnProjectile(Vector2.up);
        }
        else
        {
            float startAngle = -projectileAngleStep * (count - 1) / 2f;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * projectileAngleStep;
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                SpawnProjectile(dir);
            }
        }
    }

    void SpawnProjectile(Vector2 direction)
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();
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
        // Reset valori default
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
                currentWeaponDamageMultiplier = 0.7f;  // danno ridotto ma rateo veloce
                currentWeaponSpeedMultiplier = 1.5f;
                break;

            case "Mitra":
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
                currentWeaponAreaDamage = 1f; // danno ad area
                break;
        }

        Debug.Log($"Arma selezionata: {weapon} | Danno: {stats.damage}, AtkSpeed: {stats.attackSpeed}, Area: {currentWeaponAreaDamage}");
    }
}
