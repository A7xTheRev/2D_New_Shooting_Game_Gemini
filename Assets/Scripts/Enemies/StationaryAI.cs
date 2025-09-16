using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class StationaryAI : MonoBehaviour
{
    [Header("Impostazioni di Posizionamento")]
    [Tooltip("La posizione Y a cui questo nemico si fermerà per attaccare.")]
    public float targetYPosition = 4f;

    private EnemyStats stats;
    private bool hasReachedPosition = false;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        // Disattiva lo sparo all'inizio, lo riattiveremo quando il nemico è in posizione
        EnemyShooter shooter = GetComponent<EnemyShooter>();
        if (shooter != null)
        {
            shooter.enabled = false;
        }
    }

    void Update()
    {
        // Se non ha ancora raggiunto la sua posizione finale, continua a scendere
        if (!hasReachedPosition)
        {
            transform.position += Vector3.down * stats.moveSpeed * Time.deltaTime;

            if (transform.position.y <= targetYPosition)
            {
                transform.position = new Vector3(transform.position.x, targetYPosition, transform.position.z);
                hasReachedPosition = true;
                
                // Attiva lo sparo (se presente) solo ora che è in posizione
                EnemyShooter shooter = GetComponent<EnemyShooter>();
                if (shooter != null)
                {
                    shooter.enabled = true;
                }
            }
        }
    }
}