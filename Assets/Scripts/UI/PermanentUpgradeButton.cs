using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermanentUpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Image iconImage;
    public Button buyButton;

    private PermanentUpgradeData upgradeData;

    void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
        }
    }

    public void Setup(PermanentUpgradeData data)
    {
        upgradeData = data;
        if (upgradeData == null) return;

        if (nameText != null) nameText.text = upgradeData.upgradeName;
        if (descriptionText != null) descriptionText.text = upgradeData.description;
        if (iconImage != null) iconImage.sprite = upgradeData.icon;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyButtonPressed);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (upgradeData == null || ProgressionManager.Instance == null) return;

        int currentLevel = ProgressionManager.Instance.GetUpgradeLevel(upgradeData.upgradeType);

        if (levelText != null) levelText.text = $"Liv. {currentLevel}/{upgradeData.maxLevel}";

        if (currentLevel >= upgradeData.maxLevel)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            int cost = upgradeData.GetCostForLevel(currentLevel);
            costText.text = cost.ToString();
            buyButton.interactable = ProgressionManager.Instance.CanAfford(upgradeData.upgradeType);
        }
    }

    private void OnBuyButtonPressed()
    {
        if (upgradeData == null || ProgressionManager.Instance == null) return;

        ProgressionManager.Instance.BuyUpgrade(upgradeData.upgradeType);
        // L'UI si aggiornerà automaticamente grazie all'evento OnValuesChanged
    }
}
