using UnityEngine;
using System.IO;
using System.Collections.Generic;

// La nostra "scatola" per i dati. Ora usa le liste.
[System.Serializable]
public class SaveData
{
    public int coins;
    public List<PermanentUpgradeType> savedUpgradeTypes = new List<PermanentUpgradeType>();
    public List<int> savedUpgradeLevels = new List<int>();
}

public static class SaveSystem
{
    private static string saveFile = Application.persistentDataPath + "/savedata.json";

    // Ora il metodo SaveGame è più semplice: riceve l'oggetto SaveData già pronto.
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