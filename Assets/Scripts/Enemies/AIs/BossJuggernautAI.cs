using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossJuggernautAI : MonoBehaviour
{
    // Definiamo i possibili stati del boss
    private enum BossState
    {
        Entering,       // Ingresso in scena
        Repositioning,  // Movimento lento tra un attacco e l'altro
        SpreadShot,     // Attacco con proiettili a ventaglio
        MissileBarrage, // Lancio di missili a ricerca
        DashAttack,      // Scatto contro il giocatore
        DashEndPause, // Pausa post-scatto
        Retreating // Stato per il ritorno rapido dopo lo scatto
    }

    [Header("Riferimenti Generali")]
    private Transform playerTransform;
    private EnemyStats stats;
    private BossState currentState;

    [Header("Movimento")]
    public Vector2 arrivalPosition = new Vector2(0, 3.5f);
    public float arrivalSpeed = 3f;
    public float patrolSpeed = 1.5f;
    private float minX, maxX;
    private int patrolDirection = 1;

    [Header("Ciclo di Attacco")]
    [Tooltip("Secondi di pausa tra un attacco e l'altro.")]
    public float repositionDuration = 3f;
    private float stateTimer;
    // --- NUOVE VARIABILI PER LA LOGICA DI ATTACCO ---
    private BossState lastAttackState = BossState.Entering; // Memorizza l'ultimo attacco usato
    private int consecutiveAttackCount = 0; // Conta quante volte di fila è stato usato lo stesso attacco
    // --- FINE NUOVE VARIABILI ---

    [Header("Attacco 1: Raffica a Ventaglio")]
    public GameObject spreadShotProjectile;
    public Transform spreadFirePoint;
    public int spreadProjectileCount = 8;
    public float spreadAngle = 90f;
    public float spreadFireRate = 0.5f;
    public float spreadShotDuration = 5f;
    private float attackTimer;

    [Header("Attacco 2: Sbarramento di Missili")]
    public GameObject missileProjectile;
    public List<Transform> missileLaunchPoints;
    public int missileCountPerVolley = 3;
    public float missileFireRate = 1.5f;
    public float missileBarrageDuration = 6f;

    [Header("Attacco 3: Scatto Fulmineo")]
    public float dashTelegraphDuration = 1f;
    public float dashSpeed = 15f;
    [Tooltip("La velocità con cui il boss torna alla sua posizione dopo lo scatto.")]
    public float retreatSpeed = 18f; 
    [Tooltip("Secondi di pausa del boss dopo aver completato uno scatto.")]
    public float dashEndPauseDuration = 1f; // NUOVA VARIABILE
    public GameObject dashTelegraphIndicator;
    private Vector2 dashTargetPosition;
    private bool isDashing = false;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Calcola i confini orizzontali per il movimento
        Camera cam = Camera.main;
        float padding = GetComponent<SpriteRenderer>().bounds.extents.x;
        minX = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + padding;
        maxX = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding;

        if (dashTelegraphIndicator != null)
        {
            dashTelegraphIndicator.SetActive(false);
        }

        currentState = BossState.Entering;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Gestisce il timer di stato principale
        stateTimer -= Time.deltaTime;

        // Macchina a stati
        switch (currentState)
        {
            case BossState.Entering:
                HandleEntering();
                break;
            case BossState.Repositioning:
                HandleRepositioning();
                break;
            case BossState.SpreadShot:
                HandleSpreadShot();
                break;
            case BossState.MissileBarrage:
                HandleMissileBarrage();
                break;
            case BossState.DashAttack:
                HandleDashAttack();
                break;
            case BossState.DashEndPause:
                HandleDashEndPause();
                break;
            case BossState.Retreating:
                HandleRetreating();
                break;
        }
    }

    private void TransitionToState(BossState newState)
    {
        currentState = newState;
        
        // Logica di setup per ogni stato
        switch (currentState)
        {
            case BossState.Repositioning:
                stateTimer = repositionDuration;
                break;

            case BossState.SpreadShot:
                stateTimer = spreadShotDuration;
                attackTimer = 0; // Spara subito
                break;

            case BossState.MissileBarrage:
                stateTimer = missileBarrageDuration;
                attackTimer = 0; // Lancia subito
                break;

            case BossState.DashAttack:
                isDashing = false;
                StartCoroutine(DashTelegraphCoroutine());
                break;
            case BossState.DashEndPause:
                stateTimer = dashEndPauseDuration;
                break;
        }
    }

    private void ChooseNextAttack()
    {
        List<BossState> possibleAttacks = new List<BossState>
        {
            BossState.SpreadShot,
            BossState.MissileBarrage,
            BossState.DashAttack
        };

        BossState nextAttack;

        // --- CORREZIONE BUG 2: Limite alla ripetizione degli attacchi ---
        do
        {
            nextAttack = possibleAttacks[Random.Range(0, possibleAttacks.Count)];
        } 
        // Continua a scegliere un nuovo attacco se quello scelto è uguale al precedente E lo abbiamo già fatto 2 volte di fila.
        while (nextAttack == lastAttackState && consecutiveAttackCount >= 2);
        
        // Aggiorna i contatori
        if (nextAttack == lastAttackState)
        {
            consecutiveAttackCount++;
        }
        else
        {
            consecutiveAttackCount = 1; // È il primo di una (potenziale) nuova serie
        }
        
        lastAttackState = nextAttack;
        // --- FINE CORREZIONE ---

        TransitionToState(nextAttack);
    }

    // --- Gestione dei singoli stati ---

    void HandleEntering()
    {
        transform.position = Vector2.MoveTowards(transform.position, arrivalPosition, arrivalSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, arrivalPosition) < 0.1f)
        {
            TransitionToState(BossState.Repositioning);
        }
    }
    
    void HandleRepositioning()
    {
        // --- CORREZIONE BUG 1: Ritorno alla posizione di pattugliamento ---
        // Priorità 1: Se il boss è più in basso della sua posizione di arrivo, risale.
        if (transform.position.y < arrivalPosition.y)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, arrivalPosition.y), patrolSpeed * Time.deltaTime);
        }
        // Priorità 2: Se è all'altezza giusta, esegue il pattugliamento laterale.
        else
        {
        transform.position += new Vector3(patrolDirection * patrolSpeed * Time.deltaTime, 0, 0);
        if (transform.position.x > maxX) patrolDirection = -1;
        else if (transform.position.x < minX) patrolDirection = 1;
        }
        // --- FINE CORREZIONE ---

        if (stateTimer <= 0)
        {
            ChooseNextAttack();
        }
    }

    void HandleSpreadShot()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            // --- CORREZIONE BUG 2: Mira dinamica verso il giocatore ---
            // Calcoliamo la direzione verso il giocatore ADESSO, non con un valore fisso.
            Vector2 directionToPlayer = (playerTransform.position - spreadFirePoint.position).normalized;
            float baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg - 90f;
            // --- FINE CORREZIONE ---

            float startAngle = baseAngle - (spreadAngle / 2f);
            float angleStep = spreadAngle / (spreadProjectileCount - 1);

            for (int i = 0; i < spreadProjectileCount; i++)
            {
                float currentAngle = startAngle + i * angleStep;
                Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
                Instantiate(spreadShotProjectile, spreadFirePoint.position, rotation);
            }
            attackTimer = spreadFireRate;
        }

        if (stateTimer <= 0)
        {
            TransitionToState(BossState.Repositioning);
        }
    }

    void HandleMissileBarrage()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            StartCoroutine(LaunchMissileVolley());
            attackTimer = missileFireRate;
        }

        if (stateTimer <= 0)
        {
            TransitionToState(BossState.Repositioning);
        }
    }

    IEnumerator LaunchMissileVolley()
    {
        for (int i = 0; i < missileCountPerVolley; i++)
        {
            if (missileLaunchPoints == null || missileLaunchPoints.Count == 0) continue;
            Transform launchPoint = missileLaunchPoints[i % missileLaunchPoints.Count];
            Instantiate(missileProjectile, launchPoint.position, launchPoint.rotation);
            yield return new WaitForSeconds(0.2f); // Piccolo ritardo tra un missile e l'altro della salva
        }
    }
    
    void HandleDashAttack()
    {
        if (isDashing)
        {
            transform.position = Vector2.MoveTowards(transform.position, dashTargetPosition, dashSpeed * Time.deltaTime);
            // Se ha raggiunto la destinazione, torna a riposizionarsi
            if (Vector2.Distance(transform.position, dashTargetPosition) < 0.1f)
            {
                isDashing = false;
                TransitionToState(BossState.DashEndPause);
            }
        }
    }
    
    // --- NUOVO METODO PER GESTIRE LA PAUSA ---
    void HandleDashEndPause()
    {
        // Non fa nulla, aspetta solo che il timer scada
        if(stateTimer <= 0)
        {
            // E poi passa alla ritirata
            TransitionToState(BossState.Retreating);
        }
    }
    // --- FINE NUOVO METODO ---

    void HandleRetreating()
    {
        // Torna rapidamente alla sua posizione di pattugliamento
        transform.position = Vector2.MoveTowards(transform.position, arrivalPosition, retreatSpeed * Time.deltaTime);
        // Una volta tornato, passa allo stato di riposizionamento per scegliere il prossimo attacco
        if (Vector2.Distance(transform.position, arrivalPosition) < 0.1f)
        {
            TransitionToState(BossState.Repositioning);
        }
    }

    IEnumerator DashTelegraphCoroutine()
    {
        // Scatta verso il basso lungo la sua X attuale, fino all'altezza del giocatore
        dashTargetPosition = new Vector2(transform.position.x, playerTransform.position.y - 1.0f); 

        // Mostra l'indicatore
        if (dashTelegraphIndicator != null) dashTelegraphIndicator.SetActive(true);
        
        yield return new WaitForSeconds(dashTelegraphDuration);
        
        // Nascondi l'indicatore e inizia lo scatto
        if (dashTelegraphIndicator != null) dashTelegraphIndicator.SetActive(false);
        isDashing = true;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(stats.contactDamage);
            }
        }
    }
}