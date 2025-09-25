using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AbilitySelectorUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Il pannello 'Content' dello Scroll View delle abilità.")]
    public Transform contentPanel;
    [Tooltip("Il prefab della 'scheda' dell'abilità.")]
    public GameObject abilityPreviewPrefab;
    [Tooltip("Il riferimento allo script SnapController sullo Scroll View.")]
    public SnapController snapController;

    private List<SpecialAbility> unlockedAbilities;
    private List<AbilityPreviewUI> abilityPreviews = new List<AbilityPreviewUI>();
    private List<RectTransform> abilityPreviewRects = new List<RectTransform>();

    void OnEnable()
    {
        // Usiamo OnEnable invece di Start per far sì che si aggiorni ogni volta che il pannello Hangar viene aperto
        PopulateScrollView();

        if (snapController != null && abilityPreviewRects.Count > 0)
        {
            snapController.Initialize(abilityPreviewRects, OnAbilityChanged);
        }
    }

    void PopulateScrollView()
    {
        if (ProgressionManager.Instance == null) return;

        // Pulisci la lista precedente
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        abilityPreviews.Clear();
        abilityPreviewRects.Clear();

        // Ottieni solo le abilità ATTIVE sbloccate
        unlockedAbilities = ProgressionManager.Instance.allSpecialAbilities
            .Where(a => ProgressionManager.Instance.IsSpecialUpgradeUnlocked(a.abilityID) && a.behaviorType == AbilityBehaviorType.Active)
            .ToList();

        // Crea una "scheda" per ogni abilità
        foreach (SpecialAbility ability in unlockedAbilities)
        {
            GameObject itemObj = Instantiate(abilityPreviewPrefab, contentPanel);
            AbilityPreviewUI previewUI = itemObj.GetComponent<AbilityPreviewUI>();
            previewUI.Setup(ability);
            abilityPreviews.Add(previewUI);
            abilityPreviewRects.Add(itemObj.GetComponent<RectTransform>());
        }
    }

    // Chiamato dallo SnapController quando la selezione cambia
    public void OnAbilityChanged(int index)
    {
        if (index < 0 || index >= unlockedAbilities.Count) return;

        SpecialAbility selectedAbility = unlockedAbilities[index];

        // Evidenzia l'elemento selezionato
        for (int i = 0; i < abilityPreviews.Count; i++)
        {
            abilityPreviews[i].SetHighlight(i == index);
        }

        // Salva l'abilità equipaggiata
        ProgressionManager.Instance.SetEquippedAbility(selectedAbility);
    }
}