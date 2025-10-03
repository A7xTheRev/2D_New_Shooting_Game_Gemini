using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BestiaryEntryUI : MonoBehaviour
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
        bool isUnlocked = killCount >= enemyData.bestiaryKillRequirement;
        bool isClaimed = ProgressionManager.Instance.IsBestiaryRewardClaimed(enemyData.name);

        if (isUnlocked)
        {
            // Sbloccato
            lockOverlay.SetActive(false);
            enemyIcon.color = Color.white;
            // --- CORREZIONE 1: Rimosso ".enemyData" extra ---
            enemyNameText.text = associatedEnemyData.name.Replace("ED_", "").Replace("_", " ");
            descriptionText.text = associatedEnemyData.bestiaryDescription;
            killCountText.text = $"{killCount} / {associatedEnemyData.bestiaryKillRequirement}";

            if (isClaimed)
            {
                // Già riscosso
                claimButton.gameObject.SetActive(false);
                rewardText.text = "RISCOSSO";
            }
            else
            {
                // Da riscuotere
                claimButton.gameObject.SetActive(true);
                claimButton.onClick.AddListener(OnClaimButtonPressed);
                rewardText.text = $"{associatedEnemyData.bestiaryCoinReward} Monete\n{associatedEnemyData.bestiaryGemReward} Gemme";
            }
        }
        else
        {
            // Bloccato
            lockOverlay.SetActive(true);
            enemyIcon.color = Color.black; // Effetto silhouette
            enemyNameText.text = "???";
            descriptionText.text = "Sconfiggi questo nemico per sbloccare le informazioni.";
            killCountText.text = $"{killCount} / {associatedEnemyData.bestiaryKillRequirement}";
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
        ProgressionManager.Instance.ClaimBestiaryReward(associatedEnemyData);
        // Aggiorna la UI di questa voce per mostrare che è stata riscossa
        Setup(associatedEnemyData);
    }
}