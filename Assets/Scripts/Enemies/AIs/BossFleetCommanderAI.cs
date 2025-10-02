using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
public class BossFleetCommanderAI : MonoBehaviour
{
    private enum BossState
    {
        Repositioning,
        ExecutingManeuver
    }

    private enum Maneuver
    {
        ShieldWall,
        PincerAttack,
        OverwhelmSwarm
    }

    [Header("Riferimenti Generali")]
    private Transform playerTransform;
    private StageManager stageManager;
    private BossState currentState;

    [Header("Movimento")]
    public float patrolSpeed = 2f;
    private float minX, maxX, minY, maxY;
    private Vector2 targetPosition;

    [Header("Ciclo Tattico")]
    [Tooltip("Secondi di pausa mentre si sposta tra una manovra e l'altra.")]
    public float repositionDuration = 4f;
    private float stateTimer;
    private Maneuver lastManeuver = Maneuver.OverwhelmSwarm;

    [Header("Abilità: Urlo di Battaglia")]
    public float buffInterval = 12f;
    public float buffRadius = 8f;
    public float buffSpeedMultiplier = 1.5f; // Aumenta la velocità del 50%
    public float buffDuration = 5f;
    public Color buffColor = Color.red;
    public string buffVFXTag = "HealPulse"; // Riutilizziamo il VFX dell'Healer per ora
    private float buffTimer;

    [Header("Manovra 1: Muro di Scudi")]
    public GameObject shieldWallEnemyPrefab;
    public int shieldWallCount = 4;

    [Header("Manovra 2: Attacco a Tenaglia")]
    public GameObject pincerAttackEnemyPrefab;
    public int pincerPairCount = 2; // Quante coppie di nemici spawnare

    [Header("Manovra 3: Sciame Soverchiante")]
    public GameObject overwhelmSwarmEnemyPrefab;
    public int overwhelmSwarmCount = 12;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        stageManager = FindFirstObjectByType<StageManager>();
        buffTimer = buffInterval;

        Camera cam = Camera.main;
        float padding = GetComponent<SpriteRenderer>().bounds.extents.x;
        minX = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x + padding;
        maxX = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x - padding;
        minY = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, 0)).y;
        maxY = cam.ViewportToWorldPoint(new Vector3(0.5f, 1, 0)).y - padding;

        TransitionToState(BossState.Repositioning);
    }

    void Update()
    {
        if (playerTransform == null) return;

        stateTimer -= Time.deltaTime;
        buffTimer -= Time.deltaTime;

        // L'Urlo di Battaglia è indipendente dagli altri stati
        if (buffTimer <= 0)
        {
            ExecuteBattleCry();
            buffTimer = buffInterval;
        }

        switch (currentState)
        {
            case BossState.Repositioning:
                HandleRepositioning();
                break;
            case BossState.ExecutingManeuver:
                // La logica è gestita dalla coroutine, quindi qui non facciamo nulla
                break;
        }
    }

    private void TransitionToState(BossState newState)
    {
        currentState = newState;
        
        if (currentState == BossState.Repositioning)
        {
            stateTimer = repositionDuration;
            SetNewRandomTarget();
        }
    }

    private void ChooseNextManeuver()
    {
        List<Maneuver> possibleManeuvers = new List<Maneuver>
        {
            Maneuver.ShieldWall,
            Maneuver.PincerAttack,
            Maneuver.OverwhelmSwarm
        };
        
        // Rimuovi l'ultima manovra usata per evitare ripetizioni immediate
        possibleManeuvers.Remove(lastManeuver);
        
        Maneuver nextManeuver = possibleManeuvers[Random.Range(0, possibleManeuvers.Count)];
        lastManeuver = nextManeuver;

        StartCoroutine(ExecuteManeuver(nextManeuver));
    }
    
    IEnumerator ExecuteManeuver(Maneuver maneuver)
    {
        TransitionToState(BossState.ExecutingManeuver);
        float multiplier = stageManager.GetCurrentStatMultiplier();

        switch (maneuver)
        {
            case Maneuver.ShieldWall:
                for (int i = 0; i < shieldWallCount; i++)
                {
                    float xPos = Mathf.Lerp(minX + 1, maxX - 1, (float)i / (shieldWallCount - 1));
                    stageManager.SpawnEnemy(new Vector3(xPos, maxY + 1f, 0), shieldWallEnemyPrefab);
                    yield return new WaitForSeconds(0.3f);
                }
                break;

            case Maneuver.PincerAttack:
                for (int i = 0; i < pincerPairCount; i++)
                {
                    stageManager.SpawnEnemy(new Vector3(minX, maxY + 1f, 0), pincerAttackEnemyPrefab);
                    stageManager.SpawnEnemy(new Vector3(maxX, maxY + 1f, 0), pincerAttackEnemyPrefab);
                    yield return new WaitForSeconds(1f);
                }
                break;

            case Maneuver.OverwhelmSwarm:
                for (int i = 0; i < overwhelmSwarmCount; i++)
                {
                    float xPos = Random.Range(minX, maxX);
                    stageManager.SpawnEnemy(new Vector3(xPos, maxY + 1f, 0), overwhelmSwarmEnemyPrefab);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
        }

        // Finita la manovra, torna a riposizionarsi
        TransitionToState(BossState.Repositioning);
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
            ChooseNextManeuver();
        }
    }

    void ExecuteBattleCry()
    {
        if (VFXPool.Instance != null && !string.IsNullOrEmpty(buffVFXTag))
        {
            GameObject vfx = VFXPool.Instance.GetVFX(buffVFXTag);
            if (vfx != null) vfx.transform.position = transform.position;
        }

        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, buffRadius);
        foreach (Collider2D ally in allies)
        {
            // Controlla che non sia il boss stesso
            if (ally.gameObject == this.gameObject) continue;

            EnemyStats allyStats = ally.GetComponent<EnemyStats>();
            if (allyStats != null)
            {
                allyStats.ApplySpeedBuff(buffSpeedMultiplier, buffDuration, buffColor);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ... (il codice per il danno da contatto rimane invariato)
    }
}