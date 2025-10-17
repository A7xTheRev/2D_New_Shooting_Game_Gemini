using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class WeaponSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentPanel;
    public GameObject weaponButtonPrefab;
    public TextMeshProUGUI descriptionText;

    // Data
    private List<WeaponData> allWeapons;
    private List<WeaponButton> weaponButtons = new List<WeaponButton>();

    void Start()
    {
        // Get weapon data from ProgressionManager
        if (ProgressionManager.Instance != null)
        {
            allWeapons = ProgressionManager.Instance.allWeapons;
        }

        PopulateScrollView();

        // Select the initially equipped weapon
        if (ProgressionManager.Instance != null)
        {
            WeaponData equippedWeapon = ProgressionManager.Instance.GetEquippedWeapon();
            if (equippedWeapon != null)
            {
                OnWeaponEquipped(equippedWeapon);
            }
            else if (allWeapons.Count > 0)
            {
                // If no weapon is equipped, equip the first one by default
                OnWeaponEquipped(allWeapons[0]);
            }
        }
    }

    void PopulateScrollView()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        weaponButtons.Clear();

        if (allWeapons == null) return;

        foreach (WeaponData weapon in allWeapons)
        {
            GameObject itemObj = Instantiate(weaponButtonPrefab, contentPanel);
            WeaponButton buttonScript = itemObj.GetComponent<WeaponButton>();
            if (buttonScript != null)
            {
                buttonScript.Setup(weapon, OnWeaponEquipped);
                weaponButtons.Add(buttonScript);
            }
        }
    }

    void OnWeaponEquipped(WeaponData selectedWeapon)
    {
        if (selectedWeapon == null) return;

        // Update description panel
        if (descriptionText != null)
        {
            descriptionText.text = selectedWeapon.description;
        }

        // Save and select the weapon in the backend using the new system
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.SetEquippedWeapon(selectedWeapon);
        }

        // Update all buttons to reflect the new selection
        UpdateAllButtonsUI(selectedWeapon);
    }

    private void UpdateAllButtonsUI(WeaponData equippedWeapon)
    {
        foreach (var button in weaponButtons)
        {
            button.UpdateUI(equippedWeapon);
        }
    }
}
