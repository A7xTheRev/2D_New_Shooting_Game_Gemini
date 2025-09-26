using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldButtonUI : MonoBehaviour
{
    [Header("Riferimenti")]
    public Button button;
    public TextMeshProUGUI worldNameText;
    public GameObject lockIndicator; // L'immagine del lucchetto

    public void Setup(string name, bool isUnlocked)
    {
        worldNameText.text = name;
        button.interactable = isUnlocked;

        // Mostra o nascondi il lucchetto in base allo stato di sblocco
        if (lockIndicator != null)
        {
            lockIndicator.SetActive(!isUnlocked);
        }
    }
}