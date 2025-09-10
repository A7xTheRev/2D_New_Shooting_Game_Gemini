using UnityEngine;
using System.IO;

// Sistema di salvataggio semplice dei dati (monete)
public static class SaveSystem
{
    private static string coinsFile = Application.persistentDataPath + "/coins.json";

    public static void SaveCoins(int coins)
    {
        File.WriteAllText(coinsFile, coins.ToString());
    }

    public static int LoadCoins()
    {
        if (File.Exists(coinsFile))
        {
            string data = File.ReadAllText(coinsFile);
            int c;
            if (int.TryParse(data, out c)) return c;
        }
        return 0;
    }

    // 🔥 Metodo per resettare le monete
    public static void ResetCoins()
    {
        SaveCoins(0);
        Debug.Log("Coins resettati! Percorso file: " + coinsFile);
    }
}
