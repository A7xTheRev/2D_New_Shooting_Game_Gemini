using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpecialAbilityButton : MonoBehaviour
{
    [Header("Data")]
    private SpecialAbility abilityData;

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public TextMeshProUGUI costText;

    [Header("Buttons and Indicators")]
    public Button buyButton;
    public Button equipButton;
    public GameObject equippedIndicator;
    public GameObject unlockedIndicator; // Per passive
    public GameObject lockedOverlay; // Overlay per quando è bloccato e non acquistabile

    public void Setup(SpecialAbility data)
    {
        abilityData = data;
        if (abilityData == null) 
        {
            gameObject.SetActive(false);
            return;
        }

        if (nameText != null) nameText.text = abilityData.abilityName;
        if (descriptionText != null) descriptionText.text = abilityData.description;
        if (iconImage != null) iconImage.sprite = abilityData.icon;

        if (buyButton != null) buyButton.onClick.AddListener(OnBuyButtonPressed);
        if (equipButton != null) equipButton.onClick.AddListener(OnEquipButtonPressed);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (abilityData == null || ProgressionManager.Instance == null) return;

        bool isUnlocked = ProgressionManager.Instance.IsSpecialUpgradeUnlocked(abilityData.abilityID);

        // Disattiva tutto di default, poi attiva solo ciò che serve
        if (buyButton != null) buyButton.gameObject.SetActive(false);
        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (equippedIndicator != null) equippedIndicator.SetActive(false);
        if (unlockedIndicator != null) unlockedIndicator.SetActive(false);
        if (lockedOverlay != null) lockedOverlay.SetActive(false);
        if (costText != null) costText.gameObject.SetActive(false);

        if (isUnlocked)
        {
            if (abilityData.behaviorType == AbilityBehaviorType.Passive)
            {
                if (unlockedIndicator != null) unlockedIndicator.SetActive(true);
            }
            else // Active
            {
                bool isEquipped = (ProgressionManager.Instance.GetEquippedAbility()?.abilityID == abilityData.abilityID);
                if (isEquipped)
                {
                    if (equippedIndicator != null) equippedIndicator.SetActive(true);
                }
                else
                {
                    if (equipButton != null) equipButton.gameObject.SetActive(true);
                }
            }
        }
        else // Locked
        {
            // Mostra il pulsante di acquisto solo se è un'abilità passiva con un costo
            if (abilityData.behaviorType == AbilityBehaviorType.Passive && abilityData.cost > 0)
            {
                if (buyButton != null) 
                {
                    buyButton.gameObject.SetActive(true);
                    buyButton.interactable = ProgressionManager.Instance.GetSpecialCurrency() >= abilityData.cost;
                }
                if (costText != null) 
                {
                    costText.gameObject.SetActive(true);
                    costText.text = abilityData.cost.ToString();
                }
            }
            else // Altrimenti (es. abilità attiva non ancora sbloccata) mostra come bloccato
            {
                if (lockedOverlay != null) lockedOverlay.SetActive(true);
            }
        }
    }

    private void OnBuyButtonPressed()
    {
        if (abilityData == null || ProgressionManager.Instance == null) return;
        ProgressionManager.Instance.BuySpecialUpgrade(abilityData.abilityID);
    }

    private void OnEquipButtonPressed()
    {
        if (abilityData == null || ProgressionManager.Instance == null) return;
        ProgressionManager.Instance.SetEquippedAbility(abilityData);
    }

    void OnDestroy()
    {
        if (buyButton != null) buyButton.onClick.RemoveAllListeners();
        if (equipButton != null) equipButton.onClick.RemoveAllListeners();
    }
}
