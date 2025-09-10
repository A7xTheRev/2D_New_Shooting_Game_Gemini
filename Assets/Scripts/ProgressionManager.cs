using UnityEngine;

// Gestione monete e upgrade permanenti
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;
    private int coins;

    // Evento per notificare la UI dei cambiamenti di monete
    public static event System.Action<int> OnCoinsChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // Carica monete salvate
        coins = SaveSystem.LoadCoins();
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddCoins(int value)
    {
        coins += value;
        SaveSystem.SaveCoins(coins);
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int value)
    {
        if (coins >= value)
        {
            coins -= value;
            SaveSystem.SaveCoins(coins);
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    public int GetCoins() { return coins; }

    public void ResetCoins()
    {
        coins = 0;
        SaveSystem.SaveCoins(coins);
        OnCoinsChanged?.Invoke(coins);
        Debug.Log("Coins resettati!");
    }
}