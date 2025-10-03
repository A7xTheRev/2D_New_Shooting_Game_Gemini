using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodexEntryUI : MonoBehaviour
{
    [Header("Riferimenti Voce UI")]
    public Image enemyIcon;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI descriptionText;
    public GameObject lockOverlay;
    public Button claimButton;
    public TextMeshProUGUI rewardText;

    private EnemyData associatedEnemyData;

    public void Setup(EnemyData enemyData)
    {
        associatedEnemyData = enemyData;
        int killCount = ProgressionManager.Instance.GetMissionProgress(enemyData.name);
        bool isUnlocked = killCount >= enemyData.codexKillRequirement;
        bool isClaimed = ProgressionManager.Instance.IsCodexRewardClaimed(enemyData.name);

        if (isUnlocked)
        {
            // Sbloccato
            lockOverlay.SetActive(false);
            enemyIcon.color = Color.white;
            // --- CORREZIONE 1: Rimosso ".enemyData" extra ---
            enemyNameText.text = associatedEnemyData.name.Replace("ED_", "").Replace("_", " ");
            descriptionText.text = associatedEnemyData.codexDescription;
            killCountText.text = $"{killCount} / {associatedEnemyData.codexKillRequirement}";

            if (isClaimed)
            {
                // Già riscosso
                claimButton.gameObject.SetActive(false);
                rewardText.text = "CLAIMED";
            }
            else
            {
                // Da riscuotere
                claimButton.gameObject.SetActive(true);
                claimButton.onClick.AddListener(OnClaimButtonPressed);
                rewardText.text = $"{associatedEnemyData.codexCoinReward} Coins\n{associatedEnemyData.codexGemReward} Gems";
            }
        }
        else
        {
            // Bloccato
            lockOverlay.SetActive(true);
            enemyIcon.color = Color.black; // Effetto silhouette
            enemyNameText.text = "???";
            descriptionText.text = "Defeat this enemy to unlock its information.";
            killCountText.text = $"{killCount} / {associatedEnemyData.codexKillRequirement}";
            claimButton.gameObject.SetActive(false);
            rewardText.text = "";
        }
        
        // Assegna lo sprite (lo facciamo sempre, ma sarà nero se bloccato)
        if (enemyData.enemySprite != null)
        {
           enemyIcon.sprite = enemyData.enemySprite;
        }
    }

    void OnClaimButtonPressed()
    {
        ProgressionManager.Instance.ClaimCodexReward(associatedEnemyData);
        Setup(associatedEnemyData);
    }
}