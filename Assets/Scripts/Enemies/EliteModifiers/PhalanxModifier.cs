using UnityEngine;
using System.Collections;

public class PhalanxModifier : EliteModifier
{
    [Header("Impostazioni Scudo Falange")]
    [Tooltip("Il prefab dello scudo da attivare.")]
    public GameObject shieldPrefab;
    [Tooltip("Per quanti secondi lo scudo rimane attivo.")]
    public float shieldActiveDuration = 3f;
    [Tooltip("Secondi di attesa tra una disattivazione e la successiva riattivazione.")]
    public float shieldCooldown = 5f;
    [Tooltip("La posizione dello scudo rispetto al centro del nemico (es. X=0, Y=0.5 per metterlo un po' più in alto/davanti).")]
    public Vector2 shieldOffset = new Vector2(0, 0.5f);

    private GameObject shieldInstance;
    private float cooldownTimer;

    public override void CopyProperties(EliteModifier source)
    {
        if (source is PhalanxModifier sourcePhalanx)
        {
            this.shieldPrefab = sourcePhalanx.shieldPrefab;
            this.shieldActiveDuration = sourcePhalanx.shieldActiveDuration;
            this.shieldCooldown = sourcePhalanx.shieldCooldown;
            this.shieldOffset = sourcePhalanx.shieldOffset;
        }
    }

    public override void Activate(EnemyStats stats)
    {
        // Inizia con il cooldown, così non attiva lo scudo appena spawna
        cooldownTimer = shieldCooldown; 
    }

    void Update()
    {
        // Se lo scudo non è attivo, conta il tempo per la prossima attivazione
        if (shieldInstance == null)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                ActivateShield();
            }
        }
    }

    private void ActivateShield()
    {
        if (shieldPrefab == null) return;

        // Crea un'istanza dello scudo come figlio del nemico
        shieldInstance = Instantiate(shieldPrefab, transform);
        
        // Impostiamo la rotazione locale a zero (Quaternion.identity),
        // così erediterà correttamente solo la rotazione di 180° del genitore.
        shieldInstance.transform.localPosition = shieldOffset;
        shieldInstance.transform.localRotation = Quaternion.identity;

        // Avvia la coroutine per disattivarlo dopo un tot di tempo
        StartCoroutine(ShieldLifecycleCoroutine());
    }

    private IEnumerator ShieldLifecycleCoroutine()
    {
        yield return new WaitForSeconds(shieldActiveDuration);

        if (shieldInstance != null)
        {
            Destroy(shieldInstance);
        }
        
        // Fa ripartire il cooldown
        cooldownTimer = shieldCooldown;
    }
}