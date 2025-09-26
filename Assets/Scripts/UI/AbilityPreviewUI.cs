using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityPreviewUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Image abilityIcon;
    public TextMeshProUGUI abilityNameText;
    public TextMeshProUGUI abilityDescriptionText;
    [Tooltip("La singola immagine usata per l'highlight.")]
    public Image highlightImage; // <-- RIPRISTINATO A IMAGE

    private SpecialAbility currentAbility;

    public void Setup(SpecialAbility abilityData)
    {
        currentAbility = abilityData;

        if (abilityIcon != null)
        {
            abilityIcon.sprite = abilityData.icon;
            abilityIcon.enabled = (abilityData.icon != null);
        }
        if (abilityNameText != null)
        {
            abilityNameText.text = abilityData.abilityName;
        }
        if (abilityDescriptionText != null)
        {
            abilityDescriptionText.text = abilityData.description;
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (highlightImage != null)
        {
            // Ora controlliamo la visibilità con ".enabled"
            highlightImage.enabled = isSelected;
        }
    }
}