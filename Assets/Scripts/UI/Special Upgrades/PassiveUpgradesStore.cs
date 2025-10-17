using UnityEngine;
using System.Collections.Generic;

// Questo script gestisce la visualizzazione dei potenziamenti passivi nello store
public class PassiveUpgradesStore : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Il prefab per il pulsante di un potenziamento speciale.")]
    public GameObject specialUpgradePrefab;
    [Tooltip("Il contenitore dove verranno creati i pulsanti.")]
    public Transform container;

    private List<SpecialAbilityButton> spawnedButtons = new List<SpecialAbilityButton>();

    void OnEnable()
    {
        DrawUI();
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged += UpdateAllButtonsUI;
        }
    }

    void OnDisable()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.OnValuesChanged -= UpdateAllButtonsUI;
        }
    }

    private void DrawUI()
    {
        if (specialUpgradePrefab == null || container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        if (ProgressionManager.Instance == null) return;

        List<SpecialAbility> passiveUpgrades = ProgressionManager.Instance.GetSpecialAbilities(AbilityBehaviorType.Passive);

        foreach (SpecialAbility ability in passiveUpgrades)
        {
            GameObject buttonGO = Instantiate(specialUpgradePrefab, container);
            SpecialAbilityButton buttonScript = buttonGO.GetComponent<SpecialAbilityButton>();
            if (buttonScript != null)
            {
                buttonScript.Setup(ability);
                spawnedButtons.Add(buttonScript);
            }
        }
    }

    private void UpdateAllButtonsUI()
    {
        foreach (var button in spawnedButtons)
        {
            button.UpdateUI();
        }
    }
}
