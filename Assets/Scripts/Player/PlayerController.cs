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

    // La nuova variabile che contiene i dati dell'arma equipaggiata
    private WeaponData currentWeaponData;

    private PlayerStats stats;
    private Camera cam;
    private ProjectilePool projectilePool;
    private AbilityController abilityController;
    private float timeSinceLastTouch = 0f;
    private Color vignetteColor;
    private float fireTimer;
    private bool ignoreInputThisFrame = false;
    // --- NUOVA VARIABILE DI CONTROLLO ---
    public bool controlsEnabled = false;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        cam = Camera.main;
        abilityController = GetComponent<AbilityController>();
        if (slowMotionVignette != null) { vignetteColor = slowMotionVignette.color; }
    }

    void Start()
    {
        projectilePool = ProjectilePool.Instance;

        // Legge l'arma selezionata dal GameDataManager e la equipaggia
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedWeapon != null)
        {
            EquipWeapon(GameDataManager.Instance.selectedWeapon);
        }
        else
        {
            Debug.LogError("Nessuna arma selezionata trovata nel GameDataManager! Assicurati che l'oggetto GameDataManager esista nella scena MainMenu.");
        }
    }

    void Update()
    {
        // --- NUOVO CONTROLLO INIZIALE ---
        // Se i controlli non sono attivi, non fare nulla in Update.
        if (!controlsEnabled) return;
        // --- FINE NUOVO CONTROLLO ---

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
    }

    // --- NUOVO METODO PUBBLICO PER EQUIPAGGIARE UN'ARMA ---
    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("Si è tentato di equipaggiare un'arma nulla!");
            return;
        }
        currentWeaponData = weaponData;
        Debug.Log("Arma equipaggiata: " + currentWeaponData.weaponName);
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
        // Aggiungiamo un controllo per assicurarci di avere un'arma equipaggiata
        if (currentWeaponData == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireCooldown / stats.attackSpeed;
        }
    }

    void Shoot()
    {
        if (projectilePool == null || firePoint == null) return;
        if (currentWeaponData == null) 
        {
            Debug.LogError("ERRORE: Tento di sparare ma non ho un'arma equipaggiata (currentWeaponData è nullo)!");
            return;
        }
        
        AudioManager.Instance.PlaySound(AudioManager.Instance.playerShootSound);

        int count = stats.projectileCount;
        if (count == 1) { SpawnProjectile(Vector2.up, firePoint.position); }
        else { float startAngle = -projectileAngleStep * (count - 1) / 2f; for (int i = 0; i < count; i++) { float angle = startAngle + i * projectileAngleStep; Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up; SpawnProjectile(dir, firePoint.position); } }
    }

    void SpawnProjectile(Vector2 direction, Vector3 position)
    {
        GameObject projGameObject = projectilePool.GetProjectileForWeapon(currentWeaponData.weaponName);
        if (projGameObject == null) return;
        
        projGameObject.transform.position = position;
        projGameObject.transform.rotation = firePoint.rotation;
        projGameObject.transform.localScale = Vector3.one * stats.projectileSizeMultiplier;
        
        Projectile p = projGameObject.GetComponent<Projectile>();
        if (p != null) 
        { 
            p.baseDamage = stats.damage; 
            p.damageMultiplier = currentWeaponData.damageMultiplier; 
            p.speed *= currentWeaponData.projectileSpeedMultiplier; 
            p.areaDamageRadius = currentWeaponData.areaDamageRadius;
            p.impactVFXTag = currentWeaponData.impactVFXTag; 
            p.SetOwner(stats); 
            p.Launch(direction); 
        }
    }
}