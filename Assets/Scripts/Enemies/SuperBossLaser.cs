using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Aggiunto per poter usare le Liste

public class SuperBossLaser : MonoBehaviour
{
    [Header("Tempistiche")]
    [Tooltip("Secondi di preavviso prima che il raggio spari.")]
    public float chargeTime = 1.5f;
    [Tooltip("Secondi in cui il raggio rimane attivo e fa danno.")]
    public float fireTime = 2f;

    [Header("Danno")]
    // --- VARIABILE MODIFICATA ---
    [Tooltip("Il danno secco inflitto al primo contatto.")]
    public int flatDamage = 25;
    // --- FINE MODIFICA ---

    [Header("Riferimenti Visuali")]
    [Tooltip("L'oggetto visivo che mostra la linea di preavviso/carica.")]
    public GameObject chargeIndicator;
    [Tooltip("L'oggetto visivo del raggio laser vero e proprio.")]
    public GameObject laserBeam;

    private BoxCollider2D laserCollider;
    private bool isDealingDamage = false;

    // --- NUOVA LISTA PER TRACCIARE I BERSAGLI COLPITI ---
    private List<PlayerStats> playersDamaged;

    void Awake()
    {
        // Trova il collider sul raggio principale
        if (laserBeam != null)
        {
            laserCollider = laserBeam.GetComponent<BoxCollider2D>();
        }
        // Inizializza la lista
        playersDamaged = new List<PlayerStats>();
    }

    void Start()
    {
        // Assicurati che all'inizio solo l'indicatore di carica sia visibile
        if (chargeIndicator != null) chargeIndicator.SetActive(true);
        if (laserBeam != null) laserBeam.SetActive(false);
        if (laserCollider != null) laserCollider.enabled = false;

        // Avvia la sequenza di attacco
        StartCoroutine(FireSequence());
    }

    private IEnumerator FireSequence()
    {
        // Fase 1: Carica (mostra solo l'indicatore)
        yield return new WaitForSeconds(chargeTime);

        // Fase 2: Fuoco!
        if (chargeIndicator != null) chargeIndicator.SetActive(false);
        if (laserBeam != null) laserBeam.SetActive(true);
        if (laserCollider != null) laserCollider.enabled = true;
        isDealingDamage = true;

        yield return new WaitForSeconds(fireTime);

        // Fase 3: Scomparsa
        // Potremmo aggiungere un effetto di dissolvenza, ma per ora lo distruggiamo
        Destroy(gameObject);
    }

    // Questo metodo viene chiamato continuamente finché il giocatore è nel raggio
    void OnTriggerStay2D(Collider2D other)
    {
        if (isDealingDamage && other.CompareTag("Player"))
        {
            PlayerStats player = other.GetComponent<PlayerStats>();
            if (player != null)
            {
                // --- LOGICA DI DANNO MODIFICATA ---
                // Controlla se abbiamo GIA' danneggiato questo giocatore con questo laser
                if (!playersDamaged.Contains(player))
                {
                    // --- DANNO MODIFICATO ---
                    // Ora infliggiamo il danno secco, senza calcoli sul tempo.
                    player.TakeDamage(flatDamage);
                    // --- FINE MODIFICA ---
                    
                    playersDamaged.Add(player);
                }
                // --- FINE MODIFICA ---
            }
        }
    }
}