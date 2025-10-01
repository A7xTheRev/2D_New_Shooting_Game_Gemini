using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class SwarmAI : MonoBehaviour
{
    [Header("Impostazioni Movimento Ondeggiante")]
    public float waveAmplitude = 2f;
    public float waveFrequency = 3f;

    private EnemyStats stats;
    private float initialXPosition;
    private Camera cam;
    private float camLeftEdge;
    private float camRightEdge;
    private float spriteWidth;
    private float cleanupYThreshold;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        cam = Camera.main;
        camLeftEdge = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        camRightEdge = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x / 2;
    }

    void Start()
    {
        initialXPosition = transform.position.x;
        cleanupYThreshold = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 2f;
    }

    void Update()
    {
        if (transform.position.y < cleanupYThreshold)
        {
            Destroy(gameObject);
            return;
        }

        float verticalMovement = -stats.moveSpeed * Time.deltaTime;
        float horizontalPosition = initialXPosition + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        
        float clampedX = Mathf.Clamp(horizontalPosition, camLeftEdge + spriteWidth, camRightEdge - spriteWidth);
        transform.position = new Vector3(clampedX, transform.position.y + verticalMovement, transform.position.z);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats ps = collision.gameObject.GetComponent<PlayerStats>();
            if (ps != null) ps.TakeDamage(stats.contactDamage);
            stats.Die();
        }
    }
}