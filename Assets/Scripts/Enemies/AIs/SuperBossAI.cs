using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuperBossAI : MonoBehaviour
{
    private enum BossPhase
    {
        Entering,
        Phase1_Turrets,
        Phase2_Core
    }

    [Header("Riferimenti Componenti")]
    public List<BossTurret> turrets;
    public GameObject coreObject;
    public GameObject mainHealthBarObject;
    private EnemyStats coreStats; // Ora si riferisce direttamente alle statistiche del nucleo

    [Header("Movimento Boss")]
    public float patrolSpeed = 1.5f;
    public float patrolAreaPadding = 1f;
    public float arrivalSpeed = 4f;
    public Vector2 arrivalPosition = new Vector2(0, 3.5f);
    private float minX, maxX;
    private int patrolDirection = 1;

    // --- NUOVA SEZIONE PER L'ATTACCO LASER ---
    [Header("Attacco Fase 2 (Laser)")]
    [Tooltip("Il prefab dell'attacco laser che abbiamo creato.")]
    public GameObject laserAttackPrefab;
    [Tooltip("Il punto da cui partirà il raggio laser.")]
    public Transform laserFirePoint;
    [Tooltip("Secondi di attesa tra un attacco laser e l'altro.")]
    public float laserCooldown = 5f;
    // --- FINE NUOVA SEZIONE ---

    private BossPhase currentPhase;
    private int activeTurrets;

    void Awake()
    {
        // Ottiene il riferimento a EnemyStats direttamente dal figlio Core
        if (coreObject != null)
        {
            coreStats = coreObject.GetComponent<EnemyStats>();
        }
    }

    void Start()
    {
        currentPhase = BossPhase.Entering;
        
        // Disattiva gli elementi della fase 2 all'inizio
        if (coreObject != null) coreObject.SetActive(false);
        if (mainHealthBarObject != null) mainHealthBarObject.SetActive(false);
        if (coreStats != null) coreStats.enabled = false; // Disabilita le stats del nucleo

        foreach (BossTurret turret in turrets)
        {
            if (turret != null) turret.canShoot = false; // Usa la variabile corretta 'canShoot'
        }

        Camera cam = Camera.main;
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
        minX = bottomLeft.x + patrolAreaPadding;
        maxX = topRight.x - patrolAreaPadding;
    }

    void Update()
    {
        switch (currentPhase)
        {
            case BossPhase.Entering:
                HandleArrival();
                break;
            case BossPhase.Phase1_Turrets:
            case BossPhase.Phase2_Core:
        HandlePatrolMovement();
                break;
        }
    }

    public void InitializeBoss(float healthMultiplier)
    {
        // Applica il moltiplicatore a tutte le torrette
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
            {
                // Trova il componente EnemyStats sulla torretta e scala la sua vita
                EnemyStats turretStats = turret.GetComponent<EnemyStats>();
                if (turretStats != null)
                {
                    turretStats.maxHealth = Mathf.RoundToInt(turretStats.maxHealth * healthMultiplier);
                    turretStats.currentHealth = turretStats.maxHealth;
                }
            }
        }
    }

    // Nuovo metodo per gestire l'entrata in scena
    private void HandleArrival()
    {
        transform.position = Vector2.MoveTowards(transform.position, arrivalPosition, arrivalSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, arrivalPosition) < 0.01f)
        {
            StartPhase1();
        }
    }
    
    private void StartPhase1()
    {
        currentPhase = BossPhase.Phase1_Turrets;
        
        activeTurrets = turrets.Count;
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
            {
                turret.canShoot = true; // Usa la variabile corretta 'canShoot'
            }
        }
    }

    private void HandlePatrolMovement()
    {
        transform.position += new Vector3(patrolDirection * patrolSpeed * Time.deltaTime, 0, 0);
        if (transform.position.x > maxX) patrolDirection = -1;
        else if (transform.position.x < minX) patrolDirection = 1;
    }

    public void TurretDestroyed()
    {
        activeTurrets--;
        if (activeTurrets <= 0)
        {
            StartPhase2();
        }
    }

    private void StartPhase2()
    {
        if (currentPhase == BossPhase.Phase2_Core) return;
        currentPhase = BossPhase.Phase2_Core;
        
        // --- LOGICA DI ATTIVAZIONE MODIFICATA ---
        // Attiva il nucleo, la sua barra della vita e il suo script di statistiche
        if (coreObject != null) coreObject.SetActive(true);
        if (mainHealthBarObject != null) mainHealthBarObject.SetActive(true);
        if (coreStats != null) coreStats.enabled = true; // Ora il nucleo può essere danneggiato

        // --- ATTIVA IL CICLO DI ATTACCO LASER ---
        StartCoroutine(LaserAttackPattern());
    }

    // --- NUOVO COROUTINE PER IL CICLO D'ATTACCO ---
    private IEnumerator LaserAttackPattern()
    {
        // Aspetta un paio di secondi prima del primo attacco
        yield return new WaitForSeconds(2f);

        // Ciclo infinito finché siamo in Fase 2
        while (currentPhase == BossPhase.Phase2_Core)
        {
            // Lancia l'attacco
            if (laserAttackPrefab != null && laserFirePoint != null)
            {
                Instantiate(laserAttackPrefab, laserFirePoint.position, laserFirePoint.rotation, transform);
            }
            yield return new WaitForSeconds(laserCooldown);
        }
    }
}