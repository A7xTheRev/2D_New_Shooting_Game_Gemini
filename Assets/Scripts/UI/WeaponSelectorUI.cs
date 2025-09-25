using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class WeaponSelectorUI : MonoBehaviour
{
    [Header("Riferimenti UI")]
    [Tooltip("Il pannello 'Content' dello Scroll View delle armi.")]
    public Transform contentPanel;
    [Tooltip("Il prefab del pulsante/icona dell'arma.")]
    public GameObject weaponButtonPrefab;
    [Tooltip("Il campo di testo per mostrare la descrizione dell'arma selezionata.")]
    public TextMeshProUGUI descriptionText;
    [Tooltip("Il riferimento allo script SnapController sullo Scroll View.")]
    public SnapController snapController;

    // Riferimenti interni
    private List<WeaponData> allWeapons;
    private List<WeaponButtonUI> weaponButtons = new List<WeaponButtonUI>();
    private List<RectTransform> weaponButtonRects = new List<RectTransform>();

    void Start()
    {
        // Prendiamo le armi disponibili dal MenuManager
        if (MenuManager.Instance != null)
        {
            allWeapons = new List<WeaponData>();
            foreach (var buttonData in MenuManager.Instance.weaponButtons)
            {
                allWeapons.Add(buttonData.weaponData);
            }
        }

        PopulateScrollView();

        if (snapController != null)
        {
            snapController.Initialize(weaponButtonRects, this.OnWeaponChanged);
        }
    }

    void PopulateScrollView()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        weaponButtons.Clear();
        weaponButtonRects.Clear();

        foreach (WeaponData weapon in allWeapons)
        {
            GameObject itemObj = Instantiate(weaponButtonPrefab, contentPanel);
            WeaponButtonUI buttonUI = itemObj.GetComponent<WeaponButtonUI>();
            buttonUI.Setup(weapon);
            weaponButtons.Add(buttonUI);
            weaponButtonRects.Add(itemObj.GetComponent<RectTransform>());
        }
    }

    // Questo metodo viene chiamato dallo SnapController quando l'arma al centro cambia
    public void OnWeaponChanged(int index)
    {
        if (index < 0 || index >= allWeapons.Count) return;

        WeaponData selectedWeapon = allWeapons[index];

        // Aggiorna la descrizione
        // (Qui dovrai aggiungere le descrizioni ai tuoi WeaponData)
        if (descriptionText != null)
        {
            descriptionText.text = selectedWeapon.description; 
        }

        // Evidenzia il pulsante selezionato e deseleziona gli altri
        for (int i = 0; i < weaponButtons.Count; i++)
        {
            weaponButtons[i].SetHighlight(i == index);
        }

        // Salva la scelta
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.SelectWeapon(selectedWeapon);
        }
    }
}