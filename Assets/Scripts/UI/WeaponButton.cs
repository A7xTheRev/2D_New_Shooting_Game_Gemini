using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    [Header("UI References")]
    public Image weaponIconImage; // L'utente trascina qui l'oggetto Image per l'icona
    public TextMeshProUGUI weaponNameText; // L'utente trascina qui l'oggetto TextMeshPro per il nome
    public Button equipButton;
    public GameObject equippedIndicator;

    // Data
    public WeaponData WeaponData { get; private set; }
    private Action<WeaponData> onEquipClicked;

    public void Setup(WeaponData data, Action<WeaponData> equipCallback)
    {
        WeaponData = data;
        onEquipClicked = equipCallback;

        if (weaponIconImage != null && WeaponData.weaponIcon != null)
        {
            weaponIconImage.sprite = WeaponData.weaponIcon;
        }

        if (weaponNameText != null)
        {
            weaponNameText.text = WeaponData.weaponName;
        }

        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipButtonPressed);
        }
    }

    private void OnEquipButtonPressed()
    {
        onEquipClicked?.Invoke(WeaponData);
    }

    public void UpdateUI(WeaponData currentlyEquippedWeapon)
    {
        if (WeaponData == null) return;

        bool isEquipped = (currentlyEquippedWeapon != null && currentlyEquippedWeapon.weaponName == WeaponData.weaponName);

        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(isEquipped);
        }

        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(!isEquipped);
        }
    }

    void OnDestroy()
    {
        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
        }
    }
}
