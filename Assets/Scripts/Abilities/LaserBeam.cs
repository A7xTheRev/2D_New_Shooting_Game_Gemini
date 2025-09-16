using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    private int calculatedDamagePerSecond;
    private PlayerStats owner;

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
                float damage = calculatedDamagePerSecond * Time.deltaTime;

                // --- MODIFICA APPLICATA QUI ---
                bool isCrit = false; // Partiamo dal presupposto che non sia un critico

                if (Random.value < owner.critChance)
                {
                    isCrit = true; // Se lo è, lo registriamo
                    damage *= owner.critDamageMultiplier;
                }
                
                // Ora chiamiamo il metodo TakeDamage con entrambi i parametri
                enemy.TakeDamage(Mathf.RoundToInt(damage), isCrit);
                // --- FINE MODIFICA ---
            }
        }
    }
}