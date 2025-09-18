using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool isQuitting = false;

    public static GameManager Instance
    {
        get
        {
            if (isQuitting)
            {
                Debug.LogWarning("GameManager Instance richiesto durante la chiusura, restituisco null.");
                return null;
            }

            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("GameManager (Auto-Generated)");
                    _instance = singletonObject.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
    
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void EnemyDefeated(int coins, int xp, int specialCurrency)
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            player.CollectCoin(coins);
            player.AddXP(xp);
            if (specialCurrency > 0)
            {
                player.CollectSpecialCurrency(specialCurrency);
            }
        }
    }
}