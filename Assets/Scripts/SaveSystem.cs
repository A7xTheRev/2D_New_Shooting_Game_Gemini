using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int coins;
    public int specialCurrency;
    public AbilityID equippedAbilityID;
    public List<PermanentUpgradeType> savedUpgradeTypes = new List<PermanentUpgradeType>();
    public List<int> savedUpgradeLevels = new List<int>();
    public List<AbilityID> unlockedSpecialAbilities = new List<AbilityID>();

    // --- NUOVI CAMPI PER I RECORD ---
    public int maxWaveReached = 0;
    public int maxCoinsInSession = 0;

    // --- NUOVI CAMPI PER LE NAVICELLE ---
    public string equippedShipName; // Salviamo il nome della navicella equipaggiata
    public List<string> unlockedShipNames = new List<string>(); // Lista dei nomi delle navicelle sbloccate
    // --- FINE NUOVI CAMPI ---
}

public static class SaveSystem
{
    private static string saveFile = Application.persistentDataPath + "/savedata.json";

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFile, json);
        Debug.Log("Dati di gioco salvati in: " + saveFile);
    }

    public static SaveData LoadGame()
    {
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
        }
        else
        {
            Debug.LogWarning("File di salvataggio non trovato. Creazione di nuovi dati.");
            return new SaveData();
        }
    }
    
    public static void ResetSave()
    {
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            Debug.Log("Dati di salvataggio resettati!");
        }
    }
}