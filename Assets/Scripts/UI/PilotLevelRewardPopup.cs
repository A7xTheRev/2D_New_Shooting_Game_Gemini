using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestisce il popup che appare quando il pilota sale di livello, mostrando la ricompensa.
/// </summary>
public class PilotLevelRewardPopup : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private Button continueButton;

    private PilotLevelReward currentReward;

    void Awake()
    {
        // Aggiunge un listener al pulsante per chiudere il popup quando premuto.
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }

    /// <summary>
    /// Configura e mostra il popup con i dettagli di una specifica ricompensa di livello.
    /// </summary>
    /// <param name="reward">La ricompensa da visualizzare.</param>
    public void Show(PilotLevelReward reward)
    {
        this.currentReward = reward; // Memorizza la ricompensa

        titleText.text = $"LEVEL {reward.level} REACHED!";
        rewardIconImage.sprite = reward.rewardIcon;

        switch (reward.rewardType)
        {
            case PilotRewardType.Coins:
                descriptionText.text = $"You earned <color=#FFD700>{reward.amount} Coins</color>!";
                break;
            case PilotRewardType.Gems:
                descriptionText.text = $"You earned <color=#4DDBE8>{reward.amount} Gems</color>!";
                break;
            case PilotRewardType.ModuleSlot:
                descriptionText.text = $"New <color=#9370DB>{reward.moduleSlotType}</color> Module Slot Unlocked!";
                break;
        }
        gameObject.SetActive(true);
    }

    private void OnContinuePressed()
    {
        // Applica la ricompensa immediatamente
        if (currentReward != null && ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.ApplyPilotLevelReward(currentReward);
        }
        
        // Chiude e distrugge il popup
        Destroy(gameObject);
    }
}