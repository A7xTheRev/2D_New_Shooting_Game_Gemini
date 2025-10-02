using UnityEngine;

public class PhalanxShield : MonoBehaviour
{
    // Usa OnCollisionEnter2D perché lo scudo non è più un trigger
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Controlla se l'oggetto entrato in collisione è un proiettile del giocatore
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Non serve più il metodo BlockAndDeactivate, basta disattivarlo
                projectile.Deactivate();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }
}