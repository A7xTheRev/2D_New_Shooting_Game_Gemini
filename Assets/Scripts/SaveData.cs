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

    public int maxWaveReached = 0;
    public int maxCoinsInSession = 0;

    public string equippedShipName;
    public List<string> unlockedShipNames = new List<string>();
    
    // --- NUOVI CAMPI PER LE MISSIONI ---
    // Salviamo i progressi attuali (es. "kill_kamikaze", 45)
    public List<string> missionProgressID = new List<string>();
    public List<int> missionProgressValue = new List<int>();
    
    // Salviamo la lista delle missioni già completate e riscosse
    public List<string> claimedMissionsID = new List<string>();

    // --- NUOVI CAMPI PER LA PROGRESSIONE DEI SETTORI ---
    public List<string> sectorProgressID = new List<string>();
    public List<int> sectorProgressValue = new List<int>(); // Salverà il "numero magico"
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