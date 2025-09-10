using UnityEngine;

// Orchestratore generale, collega UI, nemici e progressione
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

            // 🔥 Applica il limite FPS scelto
            QualitySettings.vSyncCount = 0; // Disabilita vSync per rispettare targetFrameRate
            Application.targetFrameRate = (int)targetFPS;
        }
        else Destroy(gameObject);
    }

    // Metodo da chiamare quando un nemico viene sconfitto
    public void EnemyDefeated(int coins, int xp)
    {
        PlayerStats player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.CollectCoin(coins); // Aggiunge coins alla sessione
            player.AddXP(xp);
        }
    }
}
