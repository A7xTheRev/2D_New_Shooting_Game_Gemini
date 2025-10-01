using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Punto di sparo")]
    public Transform firePoint;

    // --- NUOVA VARIABILE ---
    [Header("Impostazioni PowerUp")]
    [Tooltip("L'angolo di dispersione da usare quando un'arma ottiene proiettili extra da un power-up.")]
    public float powerupSpreadAngle = 15f;
    // --- FINE NUOVA VARIABILE ---

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
    public WeaponData GetCurrentWeaponData() { return currentWeaponData; }

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
            Debug.LogError("Nessuna arma selezionata trovata nel GameDataManager!");
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
        // --- NUOVO CONTROLLO AGGIUNTO QUI ---
        // Se l'arma è disabilitata dal debuff, interrompi la funzione e non sparare.
        if (stats.isWeaponDisabled)
        {
            return;
        }
        // --- FINE NUOVO CONTROLLO ---

        if (currentWeaponData == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            // --- MODIFICA QUI: Calcola il cooldown basandosi sul fireRate dell'arma e sulla statistica del giocatore ---
            if (currentWeaponData.fireRate > 0)
            {
                fireTimer = 1f / (currentWeaponData.fireRate * stats.attackSpeed);
            }
        }
    }

    void Shoot()
    {
        if (projectilePool == null || firePoint == null || currentWeaponData == null) return;
        
        AudioManager.Instance.PlaySound(AudioManager.Instance.playerShootSound);

        // --- MODIFICA QUI: Il numero di proiettili ora parte da quello dell'arma e viene aumentato dai power-up ---
        int totalProjectiles = currentWeaponData.projectileCount + (stats.projectileCount - 1);

        if (totalProjectiles <= 1) 
        { 
            SpawnProjectile(Vector2.up, firePoint.position); 
        }
        else 
        { 
            // --- LOGICA DI SPREAD AGGIORNATA ---
            float finalSpreadAngle = currentWeaponData.spreadAngle;
            // Se l'arma non ha uno spread nativo (es. NON è uno shotgun), ma abbiamo proiettili extra da un power-up...
            if (finalSpreadAngle == 0 && totalProjectiles > 1)
            {
                // ...usiamo l'angolo di dispersione di default per i power-up.
                finalSpreadAngle = powerupSpreadAngle;
            }
            // --- FINE LOGICA AGGIORNATA ---

            float startAngle = -finalSpreadAngle * (totalProjectiles - 1) / 2f; 
            for (int i = 0; i < totalProjectiles; i++) 
            { 
                float angle = startAngle + i * finalSpreadAngle; 
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up; 
                SpawnProjectile(dir, firePoint.position); 
            } 
        }
    }

    void SpawnProjectile(Vector2 direction, Vector3 position)
    {
        // Ora passiamo l'intero ScriptableObject dell'arma, non solo il suo nome.
        GameObject projGameObject = projectilePool.GetProjectileForWeapon(currentWeaponData);

        if (projGameObject == null) return;
        
        projGameObject.transform.position = position;
        projGameObject.transform.rotation = firePoint.rotation;
        projGameObject.transform.localScale = Vector3.one * stats.projectileSizeMultiplier;
        
        Projectile p = projGameObject.GetComponent<Projectile>();
        if (p != null)
        {
            // p.baseDamage = stats.damage; // <-- QUESTA ERA LA RIGA CHE CAUSAVA L'ERRORE. RIMOSSA.
            p.damageMultiplier = currentWeaponData.damageMultiplier;
            p.speed *= currentWeaponData.projectileSpeedMultiplier;
            p.areaDamageRadius = currentWeaponData.areaDamageRadius;
            p.impactVFXTag = currentWeaponData.impactVFXTag;
            p.lifeTime = currentWeaponData.projectileLifetime;
            p.pierceCount = currentWeaponData.pierceCount;
            p.weaponType = currentWeaponData.weaponName;

            p.SetOwner(stats);
            p.Launch(direction); 
            
            // 2. SOLO ALLA FINE, attiviamo il timer di distruzione
            p.Activate(); 
        }
    }
}