using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(PowerUpManager))]
public class PowerUpManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PowerUpManager powerUpManager = (PowerUpManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Questo pulsante cercherà tutti gli asset di tipo PowerUpEffect nella cartella specificata e nelle sue sottocartelle.", MessageType.Info);

        if (GUILayout.Button("Find All PowerUps In Specified Folder"))
        {
            if (powerUpManager.powerUpsFolder == null)
            {
                Debug.LogError("ERRORE: Assegna una cartella al campo 'Power Ups Folder' prima di cercare!");
            }
            else
            {
                FindAndAssignPowerUps(powerUpManager);
            }
        }
    }

    private void FindAndAssignPowerUps(PowerUpManager manager)
    {
        manager.allPowerUps.Clear();
        
        string folderPath = AssetDatabase.GetAssetPath(manager.powerUpsFolder);
        
        // --- LOGICA DI RICERCA DEFINITIVA ---
        // 1. Cerca TUTTI gli ScriptableObject nella cartella, senza filtri di tipo.
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath });
        Debug.Log($"[Ricerca Robusta] Trovati {guids.Length} asset di tipo ScriptableObject nella cartella '{folderPath}'. Ora li filtro...");

        int foundCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // 2. Carica ogni asset e controlla manualmente se è un PowerUpEffect.
            Object loadedAsset = AssetDatabase.LoadAssetAtPath<Object>(path);
            
            if (loadedAsset is PowerUpEffect powerUp)
            {
                // 3. Se lo è, e se il suo nome inizia con "PUD_", lo aggiunge alla lista.
                if (powerUp.name.StartsWith("PUD_"))
                {
                    manager.allPowerUps.Add(powerUp);
                    foundCount++;
                }
            }
        }
        // --- FINE LOGICA ---

        manager.allPowerUps = manager.allPowerUps.OrderBy(p => p.name).ToList();

        EditorUtility.SetDirty(manager);
        
        if (foundCount > 0)
        {
            Debug.Log($"SUCCESSO: Trovati e assegnati {foundCount} potenziamenti al PowerUpManager!");
        }
        else
        {
            Debug.LogError("FALLIMENTO: Nessun potenziamento trovato. Controlla che i tuoi asset siano nella cartella giusta e che i loro nomi inizino con 'PUD_'.");
        }
    }
}