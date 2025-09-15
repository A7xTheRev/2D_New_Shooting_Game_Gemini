using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    private int calculatedDamagePerSecond;
    private PlayerStats owner;

    // MODIFICATO QUI: Ora accetta 2 argomenti
    public void Activate(PlayerStats player, int dps)
    {
        owner = player;
        calculatedDamagePerSecond = dps;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (owner != null && other.CompareTag("Enemy"))
        {
            EnemyStats enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                // Usa il danno calcolato che gli è stato passato
                float damage = calculatedDamagePerSecond * Time.deltaTime;

                if (Random.value < owner.critChance)
                {
                    damage *= owner.critDamageMultiplier;
                }
                
                enemy.TakeDamage(Mathf.RoundToInt(damage));
            }
        }
    }
}