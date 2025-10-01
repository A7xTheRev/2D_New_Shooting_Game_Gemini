using UnityEngine;

public class DisruptorProjectile : MonoBehaviour
{
    [Header("Impostazioni Debuff")]
    [Tooltip("Per quanti secondi l'arma del giocatore verrà disabilitata.")]
    public float disableDuration = 2f;

    // Le altre variabili del proiettile (velocità, durata, etc.)
    // sono gestite dal prefab stesso o da altri componenti se necessario.
    // Per ora, lo manteniamo semplice.
    public float speed = 7f;
    public float lifeTime = 5f;

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
                // Chiama il nuovo metodo sul PlayerStats per applicare il debuff
                playerStats.ApplyWeaponDisable(disableDuration);
            }
            // Il proiettile si distrugge all'impatto
            Deactivate();
        }
    }

    void Deactivate()
    {
        // In futuro, potremmo voler usare un pool anche per i proiettili nemici.
        // Per ora, lo distruggiamo.
        Destroy(gameObject);
    }
}