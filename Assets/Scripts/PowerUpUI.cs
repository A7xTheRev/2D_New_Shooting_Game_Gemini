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
    public Button buttonPrefab;
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

    private void PopulateChoices(List<PowerUp> options) 
    {
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        foreach (PowerUp pu in options)
        {
            Button b = Instantiate(buttonPrefab, buttonsContainer);
            // Mostra sia il nome che la descrizione
            b.GetComponentInChildren<TextMeshProUGUI>().text = $"{pu.displayName}\n<size=18>{pu.description}</size>";
            b.onClick.AddListener(() => OnPowerUpSelected(pu));
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
            // --- MODIFICA APPLICATA QUI ---
            // Ora passiamo anche il 'currentPlayer' al metodo, come richiesto
            List<PowerUp> newOptions = manager.GetRandomPowerUps(3, currentPlayer);
            PopulateChoices(newOptions);
        }
    }
    
    private void OnPowerUpSelected(PowerUp pu) 
    {
        if (currentPlayer == null) return;

        // Applica il potenziamento e registralo nella lista del giocatore
        pu.Apply(currentPlayer);
        currentPlayer.acquiredPowerUps.Add(pu.type);

        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}