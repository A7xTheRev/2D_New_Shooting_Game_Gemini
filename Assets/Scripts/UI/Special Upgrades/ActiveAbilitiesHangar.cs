using UnityEngine;
using System.Collections.Generic;

// Questo script gestisce la visualizzazione delle abilità attive nell'hangar
public class ActiveAbilitiesHangar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Il prefab per il pulsante di un'abilità speciale.")]
    public GameObject specialAbilityPrefab;
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
        if (specialAbilityPrefab == null || container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        if (ProgressionManager.Instance == null) return;

        List<SpecialAbility> activeAbilities = ProgressionManager.Instance.GetSpecialAbilities(AbilityBehaviorType.Active);

        foreach (SpecialAbility ability in activeAbilities)
        {
            GameObject buttonGO = Instantiate(specialAbilityPrefab, container);
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
