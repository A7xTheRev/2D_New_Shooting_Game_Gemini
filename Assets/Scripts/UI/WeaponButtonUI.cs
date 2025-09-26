using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButtonUI : MonoBehaviour
{
    [Header("Riferimenti")]
    public TextMeshProUGUI weaponNameText;
    public Image weaponIconImage;
    [Tooltip("La singola immagine usata per l'highlight.")]
    public Image highlightImage; // <-- RIPRISTINATO A IMAGE

    [HideInInspector]
    public WeaponData weaponData;

    public void Setup(WeaponData data)
    {
        weaponData = data;
        weaponNameText.text = data.weaponName;

        if (weaponIconImage != null)
        {
            if (data.weaponIcon != null)
            {
                weaponIconImage.sprite = data.weaponIcon;
                weaponIconImage.enabled = true;
            }
            else
            {
                weaponIconImage.enabled = false;
            }
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