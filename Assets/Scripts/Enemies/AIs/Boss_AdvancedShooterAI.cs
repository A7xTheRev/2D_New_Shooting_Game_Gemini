using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class Boss_AdvancedShooterAI : MonoBehaviour
{
    private enum BossState
    {
        Repositioning,
        FocusedBarrage,
        SpiralShot,
        MissileRain
    }

    [Header("Riferimenti Generali")]
    private Transform playerTransform;
    private EnemyStats stats;
    private BossState currentState;

    [Header("Movimento")]
    public float patrolSpeed = 2f;
    private float minX, maxX, minY, maxY;
    private Vector2 targetPosition;

    [Header("Ciclo di Attacco")]
    [Tooltip("Secondi di pausa mentre si sposta tra un attacco e l'altro.")]
    public float repositionDuration = 2f;
    private float stateTimer;
    private BossState lastAttackState = BossState.Repositioning;
    private int consecutiveAttackCount = 0;

    [Header("Attacco 1: Raffica Concentrata")]
    public GameObject barrageProjectile;
    public Transform barrageFirePoint;
    public float barrageFireRate = 0.1f;
    public float barrageDuration = 4f;
    private float attackTimer;

    [Header("Attacco 2: Spirale di Proiettili")]
    public GameObject spiralProjectile;
    public Transform spiralPivot;
    public float spiralRotationSpeed = 90f;
    public float spiralFireRate = 0.05f;
    public float spiralDuration = 5f;
    private List<Transform> spiralFirePoints;

    [Header("Attacco 3: Pioggia di Missili")]
    public GameObject rainIndicatorPrefab;
    public GameObject rainProjectilePrefab;
    public int rainAttackCount = 5;
    public float rainWarningTime = 1f;
    public float rainDelayBetweenShots = 0.5f;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Calcola l'area di pattugliamento (metà superiore dello schermo)
        Camera cam = Camera.main;
        float padding = GetComponent<SpriteRenderer>().bounds.extents.x;
        minX = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x + padding;
        maxX = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x - padding;
        minY = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)).y;
        maxY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1, 0)).y - padding;

        // Raccogli i punti di fuoco per la spirale
        if (spiralPivot != null)
        {
            spiralFirePoints = new List<Transform>();
            foreach (Transform child in spiralPivot)
            {
                spiralFirePoints.Add(child);
            }
        }

        TransitionToState(BossState.Repositioning);
    }

    void Update()
    {
        if (playerTransform == null) return;

        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Repositioning:
                HandleRepositioning();
                break;
            case BossState.FocusedBarrage:
                HandleFocusedBarrage();
                break;
            case BossState.SpiralShot:
                HandleSpiralShot();
                break;
            case BossState.MissileRain:
                // Questo stato è gestito da una coroutine, quindi l'Update non fa nulla
                break;
        }
    }

    private void TransitionToState(BossState newState)
    {
        currentState = newState;
        
        switch (currentState)
        {
            case BossState.Repositioning:
                stateTimer = repositionDuration;
                SetNewRandomTarget();
                break;
            case BossState.FocusedBarrage:
                stateTimer = barrageDuration;
                attackTimer = 0;
                break;
            case BossState.SpiralShot:
                stateTimer = spiralDuration;
                attackTimer = 0;
                break;
            case BossState.MissileRain:
                StartCoroutine(MissileRainCoroutine());
                break;
        }
    }

    private void ChooseNextAttack()
    {
        List<BossState> possibleAttacks = new List<BossState>
        {
            BossState.FocusedBarrage,
            BossState.SpiralShot,
            BossState.MissileRain
        };

        BossState nextAttack;
        do
        {
            nextAttack = possibleAttacks[Random.Range(0, possibleAttacks.Count)];
        } 
        while (nextAttack == lastAttackState && consecutiveAttackCount >= 2);
        
        if (nextAttack == lastAttackState)
        {
            consecutiveAttackCount++;
        }
        else
        {
            consecutiveAttackCount = 1;
        }
        
        lastAttackState = nextAttack;

        TransitionToState(nextAttack);
    }
    
    void SetNewRandomTarget()
    {
        targetPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }

    void HandleRepositioning()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, patrolSpeed * Time.deltaTime);

        if (stateTimer <= 0)
        {
            ChooseNextAttack();
        }
    }

    void HandleFocusedBarrage()
    {
        // Mira costantemente al giocatore
        if (barrageFirePoint != null)
        {
            Vector2 direction = (playerTransform.position - barrageFirePoint.position).normalized;
            // --- CORREZIONE BUG 1: MIRA INVERTITA ---
            // Il -90f è l'offset standard per gli sprite che puntano in alto, come i nostri.
            // Aggiungiamo 180 gradi per compensare la rotazione del prefab nemico.
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + 180f;
            // --- FINE CORREZIONE ---
            barrageFirePoint.rotation = Quaternion.Euler(0, 0, angle);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            // --- CORREZIONE MIRA ---
            // Calcoliamo la direzione e la rotazione necessaria per il proiettile...
            Vector2 direction = (playerTransform.position - barrageFirePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion projectileRotation = Quaternion.Euler(0, 0, angle);

            // ...e la applichiamo al proiettile al momento della creazione,
            // ignorando la rotazione del FirePoint.
            Instantiate(barrageProjectile, barrageFirePoint.position, projectileRotation);
            // --- FINE CORREZIONE ---

            attackTimer = barrageFireRate;
        }

        if (stateTimer <= 0)
        {
            TransitionToState(BossState.Repositioning);
        }
    }

    void HandleSpiralShot()
    {
        // Fa ruotare il pivot
        spiralPivot.Rotate(0, 0, spiralRotationSpeed * Time.deltaTime);
        
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            foreach (Transform firePoint in spiralFirePoints)
            {
                Instantiate(spiralProjectile, firePoint.position, firePoint.rotation);
            }
            attackTimer = spiralFireRate;
        }

        if (stateTimer <= 0)
        {
            TransitionToState(BossState.Repositioning);
        }
    }

    IEnumerator MissileRainCoroutine()
    {
        for (int i = 0; i < rainAttackCount; i++)
        {
            // Scegli la posizione del giocatore in questo momento
            Vector2 targetPos = playerTransform.position;
            
            // Fai apparire l'indicatore
            GameObject indicator = Instantiate(rainIndicatorPrefab, targetPos, Quaternion.identity);
            
            // Aspetta il tempo di preavviso
            yield return new WaitForSeconds(rainWarningTime);
            
            if(indicator != null) Destroy(indicator);

            // Fai "piovere" il proiettile dall'alto
            Vector2 spawnPos = new Vector2(targetPos.x, maxY + 1f);
            Instantiate(rainProjectilePrefab, spawnPos, Quaternion.Euler(0, 0, 180)); // Ruotato per andare verso il basso

            yield return new WaitForSeconds(rainDelayBetweenShots);
        }

        // Finito l'attacco, torna a riposizionarsi
        TransitionToState(BossState.Repositioning);
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