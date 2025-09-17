using UnityEngine;
using System.Collections.Generic;

public class SuperBossAI : MonoBehaviour
{
    // Aggiungiamo il nuovo stato di Entrata
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
    // --- NUOVE VARIABILI PER L'ENTRATA IN SCENA ---
    [Tooltip("La velocità con cui il boss entra in scena.")]
    public float arrivalSpeed = 4f;
    [Tooltip("La posizione finale che il boss raggiungerà prima di iniziare a combattere.")]
    public Vector2 arrivalPosition = new Vector2(0, 3.5f);
    // --- FINE NUOVE VARIABILI ---
    private float minX, maxX;
    private int patrolDirection = 1;

    private BossPhase currentPhase;
    private int activeTurrets;

    void Awake()
    {
        mainBodyStats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        // All'inizio, il boss è sempre nello stato di "Entrata"
        currentPhase = BossPhase.Entering;
        
        // Assicuriamoci che tutti gli elementi di combattimento siano spenti
        if (coreObject != null) coreObject.SetActive(false);
        if (mainHealthBarObject != null) mainHealthBarObject.SetActive(false);
        if (mainBodyStats != null) mainBodyStats.enabled = false;

        // Disattiva le torrette (non possono sparare né essere danneggiate durante l'entrata)
        foreach (BossTurret turret in turrets)
        {
            if (turret != null) turret.canBeDamaged = false;
        }

        // Calcola i limiti di pattugliamento per dopo
        Camera cam = Camera.main;
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector3(1, 0, 0));
        minX = bottomLeft.x + patrolAreaPadding;
        maxX = topRight.x - patrolAreaPadding;
    }

    void Update()
    {
        // Gestisci il comportamento in base alla fase corrente
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

    // Nuovo metodo per gestire l'entrata in scena
    private void HandleArrival()
    {
        // Muovi il boss verso la sua posizione di arrivo
        transform.position = Vector2.MoveTowards(transform.position, arrivalPosition, arrivalSpeed * Time.deltaTime);

        // Quando ha raggiunto la destinazione...
        if (Vector2.Distance(transform.position, arrivalPosition) < 0.01f)
        {
            // ...inizia la Fase 1
            StartPhase1();
        }
    }
    
    // Contiene la logica che prima era in Start()
    private void StartPhase1()
    {
        currentPhase = BossPhase.Phase1_Turrets;
        
        activeTurrets = turrets.Count;
        foreach (BossTurret turret in turrets)
        {
            if (turret != null)
            {
                // Ora le torrette si attivano!
                turret.canBeDamaged = true;
            }
        }
        Debug.Log("Super Boss: Inizio Fase 1. Distruggi le " + activeTurrets + " torrette!");
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
    }
}