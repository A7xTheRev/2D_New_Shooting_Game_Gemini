using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // Aggiunto per poter usare le Image

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

    // Metodo statico che lo StageManager chiamerà prima di cambiare scena
    public static void SetVictoryStats(SectorData sector, int coins, int gems, SectorObjective objectives)
    {
        lastCompletedSectorID = sector.name;
        lastCompletedSectorName = sector.sectorName;
        lastSessionCoins = coins;
        lastSessionGems = gems;
        lastObjectivesAchieved = objectives;
    }

    void Start()
    {
        // 1. Salva il progresso
        if (ProgressionManager.Instance != null && !string.IsNullOrEmpty(lastCompletedSectorID))
        {
            ProgressionManager.Instance.SetSectorProgress(lastCompletedSectorID, lastObjectivesAchieved);
            ProgressionManager.Instance.NotifySectorCompleted(lastCompletedSectorID); // Per le missioni
            
            // Aggiungi le valute della sessione al totale
            ProgressionManager.Instance.AddCoins(lastSessionCoins);
            ProgressionManager.Instance.AddSpecialCurrency(lastSessionGems);
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