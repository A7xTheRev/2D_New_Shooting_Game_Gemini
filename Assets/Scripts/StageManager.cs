using UnityEngine;
using System.Collections;

// Gestisce lo spawn dei nemici e lo scaling degli stage
public class StageManager : MonoBehaviour
{
    [Header("Prefab Nemico")]
    public GameObject enemyPrefab;

    [Header("Gestione Stage")]
    public int stageNumber = 1;          // Numero corrente dello stage
    public float growthRate = 0.15f;     // Moltiplicatore per scalare statistiche nemici
    public int enemiesPerStage = 5;      // Numero di nemici da spawnare per stage
    public float spawnY = 6f;            // Altezza sopra lo schermo dove spawnare i nemici

    [Header("Limiti orizzontali spawn")]
    public float spawnXMin = -8f;
    public float spawnXMax = 8f;

    [Header("Limiti ritardo spawn")]
    public float spawnDelayMin = 0.3f;
    public float spawnDelayMax = 1f;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        StartCoroutine(SpawnStageCoroutine());
    }

    void Update()
    {
        // Controlla se non ci sono più nemici in scena
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            NextStage();
        }
    }

    // Spawn singolo nemico
    public void SpawnEnemy(Vector3 position)
    {
        if (enemyPrefab == null) return;

        // Usa la rotazione del prefab invece che Quaternion.identity
        GameObject e = Instantiate(enemyPrefab, position, enemyPrefab.transform.rotation);

        EnemyStats es = e.GetComponent<EnemyStats>();
        if (es != null)
        {
            float multiplier = 1f + (stageNumber - 1) * growthRate;
            es.maxHealth = Mathf.RoundToInt(es.maxHealth * multiplier);
            es.currentHealth = es.maxHealth;
            es.moveSpeed *= multiplier; // Mantiene la velocità già presente nello script nemico
            //es.coinReward = Mathf.RoundToInt(es.coinReward * multiplier);
            //es.xpReward = Mathf.RoundToInt(es.xpReward * multiplier);
        }

        e.tag = "Enemy"; // Tagga per il rilevamento automatico
    }

    // Coroutine per spawn multiplo con ritardo variabile
    private IEnumerator SpawnStageCoroutine()
    {
        for (int i = 0; i < enemiesPerStage; i++)
        {
            float xPos = Random.Range(spawnXMin, spawnXMax);
            Vector3 pos = new Vector3(xPos, spawnY, 0f);
            SpawnEnemy(pos);

            // Ritardo random tra spawn
            float delay = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(delay);
        }
    }

    // Passa al prossimo stage
    public void NextStage()
    {
        stageNumber++;
        StartCoroutine(SpawnStageCoroutine());
    }
}
