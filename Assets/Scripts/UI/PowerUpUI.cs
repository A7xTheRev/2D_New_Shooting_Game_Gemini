using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PowerUpUI : MonoBehaviour
{
    public static PowerUpUI Instance;

    [Header("UI Panel")]
    public GameObject panel;
    public Transform buttonsContainer;
    public GameObject buttonPrefab; // Lasciato come GameObject per flessibilità
    public Button rerollButton;

    private PlayerStats currentPlayer;
    private bool rerollUsedThisLevel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if(panel != null) panel.SetActive(false);
    }

    public void ShowPowerUpChoices(List<PowerUp> options, PlayerStats player)
    {
        if (options == null || options.Count == 0) return;
        currentPlayer = player;
        panel.SetActive(true);
        Time.timeScale = 0f;

        if (rerollButton != null)
        {
            bool canReroll = ProgressionManager.Instance.IsSpecialUpgradeUnlocked(AbilityID.PowerUpReroll);
            rerollButton.gameObject.SetActive(canReroll);
            if (canReroll)
            {
                rerollUsedThisLevel = false;
                rerollButton.interactable = true;
            }
        }
        PopulateChoices(options);
    }

    // --- LOGICA DI POPOLAMENTO CORRETTA ---
    private void PopulateChoices(List<PowerUp> options) 
    {
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        foreach (PowerUp pu in options)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsContainer);
            
            // 1. Troviamo lo script PowerUpButtonUI sul nostro prefab
            PowerUpButtonUI buttonUI = buttonObj.GetComponent<PowerUpButtonUI>();
            if (buttonUI != null)
            {
                // 2. Usiamo il suo metodo Setup per popolare icona e testi nei posti giusti
                buttonUI.Setup(pu);
            }
            else
            {
                // Fallback di sicurezza se lo script manca, per evitare errori
                Debug.LogError("Il prefab del pulsante non ha lo script PowerUpButtonUI!");
            }

            // 3. Aggiungiamo l'azione al click del pulsante
            Button b = buttonObj.GetComponent<Button>();
            if (b != null)
            {
            b.onClick.AddListener(() => OnPowerUpSelected(pu));
            }
        }
    }
    
    public void OnRerollButtonPressed() 
    {
        if (rerollUsedThisLevel || currentPlayer == null) return;
        rerollUsedThisLevel = true;
        rerollButton.interactable = false;
        PowerUpManager manager = FindFirstObjectByType<PowerUpManager>();
        if (manager != null)
        {
            List<PowerUp> newOptions = manager.GetRandomPowerUps(3, currentPlayer);
            PopulateChoices(newOptions);
        }
    }
    
    private void OnPowerUpSelected(PowerUp pu) 
    {
        if (currentPlayer == null) return;

        pu.Apply(currentPlayer);
        currentPlayer.acquiredPowerUps.Add(pu.type);

        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}