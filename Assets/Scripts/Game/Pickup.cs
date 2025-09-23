using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Coin, Gem, Health }

    [Header("Impostazioni Raccoglibile")]
    public PickupType type = PickupType.Coin;
    public int value = 1;

    [Header("Comportamento Magnetico")]
    public float magnetSpeed = 8f;
    public float timeBeforeMagnet = 0.5f; // Un breve ritardo prima che l'oggetto voli verso il player

    private Transform playerTransform;
    private bool isFollowingPlayer = false;

    void Start()
    {
        // Avvia una coroutine per attivare il magnetismo dopo un breve ritardo
        StartCoroutine(ActivateMagnetCoroutine());
    }

    void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            // Muove l'oggetto verso il giocatore
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, magnetSpeed * Time.deltaTime);
        }
    }

    private IEnumerator ActivateMagnetCoroutine()
    {
        yield return new WaitForSeconds(timeBeforeMagnet);
        // Trova il giocatore solo una volta, per efficienza
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            isFollowingPlayer = true;
        }
    }

    // Questo metodo verrà chiamato dal giocatore quando lo raccoglie
    public void Collect()
    {
        // Per ora, distruggiamo semplicemente l'oggetto.
        // La logica di aggiunta monete/vita sarà gestita dal giocatore.
        Destroy(gameObject);
    }
}