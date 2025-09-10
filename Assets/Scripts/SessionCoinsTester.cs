using UnityEngine;

// Script di test per incrementare i session coins ogni secondo
public class SessionCoinsTester : MonoBehaviour
{
    public PlayerStats player;
    public int coinsPerSecond = 1;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        }

        // Avvia coroutine per test incrementi
        StartCoroutine(AddCoinsOverTime());
    }

    private System.Collections.IEnumerator AddCoinsOverTime()
    {
        while (true)
        {
            if (player != null)
                player.CollectCoin(coinsPerSecond);

            yield return new WaitForSeconds(1f); // ogni secondo
        }
    }
}