using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum FPSLimit
    {
        Unlimited = -1,
        FPS_60 = 60,
        FPS_90 = 90,
        FPS_120 = 120
    }

    [Header("Impostazioni FPS")]
    public FPSLimit targetFPS = FPSLimit.FPS_60;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)targetFPS;
        }
        else Destroy(gameObject);
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