using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 10;
    public float lifeTime = 5f;

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void OnEnable()
    {
        Invoke(nameof(Deactivate), lifeTime);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Controlla se ha colpito una zona di morte
        if (other.CompareTag("DeathZone"))
        {
            Deactivate();
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Deactivate();
        }
    }

    void Deactivate()
    {
        Destroy(gameObject);
    }
}