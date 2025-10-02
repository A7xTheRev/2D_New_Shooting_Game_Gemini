using UnityEngine;

public class AvengerModifier : EliteModifier
{
    [Header("Impostazioni Vendicatore")]
    public GameObject projectilePrefab;
    public int projectileCount = 8;

    private EnemyStats ownerStats;

    // Metodo per copiare le impostazioni da un altro componente AvengerModifier
    public override void CopyProperties(EliteModifier source)
    {
        if (source is AvengerModifier sourceAvenger)
        {
            this.projectilePrefab = sourceAvenger.projectilePrefab;
            this.projectileCount = sourceAvenger.projectileCount;
        }
    }

    public override void Activate(EnemyStats stats)
    {
        ownerStats = stats;
    }

    public override void OnDeath()
    {
        if (projectilePrefab == null || ownerStats == null) return;

        Debug.Log("Modificatore Vendicatore attivato!");
        float angleStep = 360f / projectileCount;
        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            
            // Usiamo la posizione del nemico al momento della morte come punto di sparo
            GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
            EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
            if(projScript != null)
            {
                // I proiettili fanno un danno basato sul danno da proiettile dell'Elite
                projScript.SetDamage(ownerStats.projectileDamage);
            }
        }
    }
}