using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButtonUI : MonoBehaviour
{
    [Header("Riferimenti")]
    public TextMeshProUGUI weaponNameText;
    public Image highlightImage; // Immagine usata per l'evidenziazione

    [HideInInspector]
    public WeaponData weaponData;

    public void Setup(WeaponData data)
    {
        weaponData = data;
        weaponNameText.text = data.weaponName;
    }

    public void SetHighlight(bool isSelected)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = isSelected;
        }
    }
}