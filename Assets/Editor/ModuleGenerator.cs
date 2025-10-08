using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class ModuleGenerator : EditorWindow
{
    // Variabili per la nostra finestra del tool
    private string csvPath = "Assets/Data/Modules.csv";
    private string outputPath = "Assets/Data/ModuleData/";
    private string specificIconsFolderPath = "Assets/Resources/Icons/Modules";
    private string genericIconsSheetPath = "Assets/Resources/Icons/Modules/MOD_GenericIcons.png"; // Percorso al file dello sheet

    private Sprite[] genericSpritesCache; // Cache per non ricaricare lo sheet ogni volta

    // Aggiunge una voce di menu in alto nell'editor di Unity
    [MenuItem("Tools/Astro Survivor/Module Generator")]
    public static void ShowWindow()
    {
        // Apre la nostra finestra personalizzata
        GetWindow<ModuleGenerator>("Module Generator");
    }

    // Disegna l'interfaccia della nostra finestra
    void OnGUI()
    {
        GUILayout.Label("Module Generation Settings", EditorStyles.boldLabel);
        
        csvPath = EditorGUILayout.TextField("Path to CSV File", csvPath);
        outputPath = EditorGUILayout.TextField("Output Path for Assets", outputPath);
        
        EditorGUILayout.Space();
        GUILayout.Label("Icon Settings", EditorStyles.boldLabel);
        specificIconsFolderPath = EditorGUILayout.TextField("Specific Icons Folder", specificIconsFolderPath);
        genericIconsSheetPath = EditorGUILayout.TextField("Generic Icons Sheet File", genericIconsSheetPath);

        if (GUILayout.Button("Generate / Update Modules"))
        {
            GenerateModules();
        }
    }

    private void GenerateModules()
    {
        // --- MODIFICA: Carica lo sprite sheet una sola volta all'inizio ---
        PreloadGenericSprites();
        // Legge tutte le righe dal file CSV
        string[] lines = File.ReadAllLines(Application.dataPath + csvPath.Replace("Assets", ""));

        // Salta la prima riga (l'intestazione con i nomi delle colonne)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            // Mappiamo i valori letti in variabili più leggibili
            string moduleID = values[0];
            string moduleName = values[1];
            string description = values[2];
            ModuleRarity rarity = (ModuleRarity)System.Enum.Parse(typeof(ModuleRarity), values[3]);
            ModuleSlotType slotType = (ModuleSlotType)System.Enum.Parse(typeof(ModuleSlotType), values[4]);
            ModuleStatType statToModify = (ModuleStatType)System.Enum.Parse(typeof(ModuleStatType), values[5]);
            // Usiamo CultureInfo.InvariantCulture per forzare l'uso del '.' come separatore decimale
            float bonusValue = float.Parse(values[6], CultureInfo.InvariantCulture);
            // fusionResult (values[7]) lo useremo dopo

            // --- Logica per il nome del file ---
            string fileName = $"MOD_{statToModify}_{rarity}.asset";
            string fullPath = Path.Combine(outputPath, fileName);

            // Cerca se l'asset esiste già, altrimenti lo crea
            ModuleData moduleData = AssetDatabase.LoadAssetAtPath<ModuleData>(fullPath);
            if (moduleData == null)
            {
                moduleData = ScriptableObject.CreateInstance<ModuleData>();
                // Assicurati che la cartella di output esista
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
                AssetDatabase.CreateAsset(moduleData, fullPath);
            }

            // Popola o aggiorna i dati dell'asset
            moduleData.moduleID = moduleID;
            moduleData.moduleName = moduleName;
            moduleData.description = description;
            moduleData.rarity = rarity;
            moduleData.slotType = slotType;
            moduleData.statToModify = statToModify;
            moduleData.bonusValue = bonusValue;

            // --- NUOVA LOGICA IBRIDA PER L'ICONA ---
            // 1. Prova a cercare l'icona specifica
            string specificIconName = $"MOD_{statToModify}_Icon";
            Sprite icon = LoadSpriteFromResources(specificIconsFolderPath, specificIconName);
            
            // 2. Se non la trova, cerca l'icona generica nello sheet
            if (icon == null)
            {
                string genericIconName = $"MOD_{slotType}_Icon";
                icon = FindSpriteInCache(genericIconName);
            }
            moduleData.icon = icon;

            // Marca l'asset come "modificato" per forzare il salvataggio
            EditorUtility.SetDirty(moduleData);
        }

        // --- SECONDA PASSATA: Collega i Fusion Results ---
        // Lo facciamo dopo aver creato tutti gli asset, così siamo sicuri che esistano
        for (int i = 1; i < lines.Length; i++)
        {
             string[] values = lines[i].Split(',');
             ModuleStatType statToModify = (ModuleStatType)System.Enum.Parse(typeof(ModuleStatType), values[5]);
             ModuleRarity rarity = (ModuleRarity)System.Enum.Parse(typeof(ModuleRarity), values[3]);
             string fusionResultID = values[7];

             if (!string.IsNullOrEmpty(fusionResultID) && fusionResultID != "None")
             {
                 // Trova l'asset sorgente
                 string sourceFileName = $"MOD_{statToModify}_{rarity}.asset";
                 string sourcePath = Path.Combine(outputPath, sourceFileName);
                 ModuleData sourceModule = AssetDatabase.LoadAssetAtPath<ModuleData>(sourcePath);

                 // Trova l'asset risultato della fusione (cercandolo tra tutti)
                 ModuleData resultModule = FindModuleByID(fusionResultID);
                 
                 if (sourceModule != null && resultModule != null)
                 {
                     sourceModule.fusionResult = resultModule;
                     EditorUtility.SetDirty(sourceModule);
                 }
             }
        }

        // Salva tutte le modifiche fatte agli asset
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Generazione/Aggiornamento Moduli Completato con logica ibrida per le icone!");
    }

    // Carica un singolo sprite da una cartella in Resources
    private Sprite LoadSpriteFromResources(string folderPath, string spriteName)
    {
        string relativePath = folderPath.Replace("Assets/Resources/", "");
        return Resources.Load<Sprite>(Path.Combine(relativePath, spriteName));
    }

    // Carica tutti i sotto-sprite da uno sheet e li mette in cache
    private void PreloadGenericSprites()
    {
        string sheetRelativePath = genericIconsSheetPath
            .Replace("Assets/Resources/", "")
            .Replace(".png", "") // Rimuove l'estensione per il caricamento
            .Replace(".jpg", ""); 
            
        genericSpritesCache = Resources.LoadAll<Sprite>(sheetRelativePath);
        if (genericSpritesCache == null || genericSpritesCache.Length == 0)
        {
            Debug.LogWarning($"Nessun sotto-sprite trovato nel file sheet: {genericIconsSheetPath}. Assicurati che sia in una cartella 'Resources' e che sia configurato come 'Multiple'.");
        }
    }

    // Cerca uno sprite nella cache pre-caricata
    private Sprite FindSpriteInCache(string spriteName)
    {
        if (genericSpritesCache == null) return null;
        // Usa LINQ per trovare il primo sprite il cui nome corrisponde
        return genericSpritesCache.FirstOrDefault(s => s.name == spriteName);
    }

    private ModuleData FindModuleByID(string id)
    {
        // Trova tutti gli asset di tipo ModuleData nel progetto
        string[] guids = AssetDatabase.FindAssets("t:ModuleData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModuleData data = AssetDatabase.LoadAssetAtPath<ModuleData>(path);
            if (data.moduleID == id)
            {
                return data;
            }
        }
        return null;
    }
}