using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AbilitySelectorUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The container where the ability buttons will be created.")]
    public Transform contentPanel;
    [Tooltip("The prefab for the special ability button.")]
    public GameObject specialAbilityButtonPrefab;

    private List<SpecialAbilityButton> spawnedButtons = new List<SpecialAbilityButton>();

    void OnEnable()
    {
        PopulateScrollView();
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

    void PopulateScrollView()
    {
        if (ProgressionManager.Instance == null || specialAbilityButtonPrefab == null || contentPanel == null) return;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        List<SpecialAbility> activeAbilities = ProgressionManager.Instance.allSpecialAbilities
            .Where(a => a.behaviorType == AbilityBehaviorType.Active)
            .ToList();

        foreach (SpecialAbility ability in activeAbilities)
        {
            GameObject buttonGO = Instantiate(specialAbilityButtonPrefab, contentPanel);
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
