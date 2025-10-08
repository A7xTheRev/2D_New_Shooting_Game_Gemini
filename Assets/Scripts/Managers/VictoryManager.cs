using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // Aggiunto per poter usare le Image
using System.Collections.Generic; // Aggiunto per poter usare le List

public class VictoryManager : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI sectorNameText;
    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI gemsEarnedText;
    public Button continueButton;

    [Header("Stelle Ottenute")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite starFilledSprite; // Lo sprite per una stella guadagnata
    public Sprite starEmptySprite;  // Lo sprite per una stella non guadagnata

    // Variabili statiche per passare i dati tra le scene
    private static string lastCompletedSectorID;
    private static string lastCompletedSectorName;
    private static int lastSessionCoins;
    private static int lastSessionGems;
    private static SectorObjective lastObjectivesAchieved;
    private static SectorData lastCompletedSectorData; // --- NUOVO: Memorizziamo l'intero SectorData ---

    // Metodo statico che lo StageManager chiamerà prima di cambiare scena
    public static void SetVictoryStats(SectorData sector, int coins, int gems, SectorObjective objectives)
    {
        lastCompletedSectorID = sector.name;
        lastCompletedSectorName = sector.sectorName;
        lastSessionCoins = coins;
        lastSessionGems = gems;
        lastObjectivesAchieved = objectives;
        lastCompletedSectorData = sector; // --- NUOVO: Salviamo il riferimento al SectorData ---
    }

    void Start()
    {
        // 1. Salva il progresso e assegna le valute
        if (ProgressionManager.Instance != null && !string.IsNullOrEmpty(lastCompletedSectorID))
        {
            ProgressionManager.Instance.SetSectorProgress(lastCompletedSectorID, lastObjectivesAchieved);
            ProgressionManager.Instance.NotifySectorCompleted(lastCompletedSectorID); // Per le missioni
            
            // Aggiungi le valute della sessione al totale
            ProgressionManager.Instance.AddCoins(lastSessionCoins);
            ProgressionManager.Instance.AddSpecialCurrency(lastSessionGems);

            // --- NUOVA LOGICA PER ASSEGNARE I MODULI ---
            GrantModuleRewards();
            // --- FINE NUOVA LOGICA ---
        }

        // 2. Mostra le informazioni nella UI
        if (sectorNameText != null) sectorNameText.text = lastCompletedSectorName + "";
        if (coinsEarnedText != null) coinsEarnedText.text = "Obtained: " + lastSessionCoins;
        if (gemsEarnedText != null) gemsEarnedText.text = "Obtained: " + lastSessionGems;

        // 3. Mostra le stelle
        UpdateStars();

        // Collega il pulsante per tornare al menu
        continueButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }

    // --- NUOVO METODO PER GESTIRE LE RICOMPENSE IN MODULI ---
    private void GrantModuleRewards()
    {
    if (lastCompletedSectorData == null || ProgressionManager.Instance == null)
        {
            return;
        }

    // --- MODIFICA: Logica di ricompensa aggiornata ---
    var chances = lastCompletedSectorData.victoryRarityDropChances;
    if (chances == null || chances.Count == 0) return;

        List<ModuleData> awardedModules = new List<ModuleData>();

        // Tira il dado per ogni modulo che dobbiamo assegnare
        for (int i = 0; i < lastCompletedSectorData.moduleRewardsCount; i++)
        {
        // Chiama il nuovo metodo del ProgressionManager per ottenere un modulo
        ModuleData droppedModule = ProgressionManager.Instance.GetRandomModuleDrop(chances);
            if (droppedModule != null)
            {
                ProgressionManager.Instance.AddModule(droppedModule.moduleID, 1);
                awardedModules.Add(droppedModule);
            }
        }

        if (awardedModules.Count > 0)
        {
            Debug.Log($"Ricompense settore '{lastCompletedSectorName}' completato:");
            foreach (var module in awardedModules)
            {
                Debug.Log($"- Ricevuto 1x {module.moduleName} ({module.rarity})");
            }

            // TODO in Fase 4: Aggiornare la UI della schermata di vittoria per mostrare i moduli ottenuti.
        }
    }
    // --- FINE NUOVO METODO ---

    private void UpdateStars()
    {
        // Stella 1: Completamento
        bool star1Earned = (lastObjectivesAchieved & SectorObjective.SECTOR_COMPLETED) != 0;
        star1.sprite = star1Earned ? starFilledSprite : starEmptySprite;

        // Stella 2: Vita > 70%
        bool star2Earned = (lastObjectivesAchieved & SectorObjective.HEALTH_OVER_70_PERCENT) != 0;
        star2.sprite = star2Earned ? starFilledSprite : starEmptySprite;

        // Stella 3: Nessun danno
        bool star3Earned = (lastObjectivesAchieved & SectorObjective.NO_DAMAGE_TAKEN) != 0;
        star3.sprite = star3Earned ? starFilledSprite : starEmptySprite;
    }
}