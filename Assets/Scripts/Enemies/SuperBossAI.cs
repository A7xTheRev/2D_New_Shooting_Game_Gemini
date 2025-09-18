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
    private EnemyStats mainBodyStats;

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
        mainBodyStats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        currentPhase = BossPhase.Entering;
        
        if (coreObject != null) coreObject.SetActive(false);
        if (mainHealthBarObject != null) mainHealthBarObject.SetActive(false);
        if (mainBodyStats != null) mainBodyStats.enabled = false;

        foreach (BossTurret turret in turrets)
        {
            if (turret != null) turret.canBeDamaged = false;
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
                turret.ScaleHealth(healthMultiplier);
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
                turret.canBeDamaged = true;
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
        
        if (coreObject != null) coreObject.SetActive(true);
        if (mainHealthBarObject != null) mainHealthBarObject.SetActive(true);
        if (mainBodyStats != null) mainBodyStats.enabled = true;

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
                // --- MODIFICA APPLICATA QUI ---
                // Aggiungiamo 'transform' come ultimo argomento.
                // Questo dice a Unity di creare il laser come figlio di questo oggetto (il boss).
                Instantiate(laserAttackPrefab, laserFirePoint.position, laserFirePoint.rotation, transform);
                // --- FINE MODIFICA ---
            }
            
            yield return new WaitForSeconds(laserCooldown);
        }
    }
}