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
    // La variabile 'buttonPrefab' è stata rimossa da qui.
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

    // Accetta una lista del nuovo tipo PowerUpEffect
    public void ShowPowerUpChoices(List<PowerUpEffect> options, PlayerStats player)
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

    // Accetta una lista del nuovo tipo PowerUpEffect
    private void PopulateChoices(List<PowerUpEffect> options) 
    {
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        foreach (PowerUpEffect pu in options)
        {
            // --- LOGICA DI CREAZIONE DINAMICA AGGIORNATA ---
            
            // 1. Controlla se il potenziamento ha un prefab di pulsante assegnato.
            if (pu.buttonPrefab == null)
            {
                Debug.LogError($"Il potenziamento '{pu.displayName}' non ha un prefab di pulsante assegnato nel suo ScriptableObject!");
                continue; // Salta questo potenziamento per evitare errori.
            }

            // 2. Crea un'istanza del prefab specifico del potenziamento.
            GameObject buttonObj = Instantiate(pu.buttonPrefab, buttonsContainer);
            // --- FINE LOGICA AGGIORNATA ---
            
            PowerUpButtonUI buttonUI = buttonObj.GetComponent<PowerUpButtonUI>();
            if (buttonUI != null)
            {
                // 2. Usiamo il suo metodo Setup per popolare icona e testi nei posti giusti
                buttonUI.Setup(pu);
            }
            else
            {
                Debug.LogError($"Il prefab del pulsante per '{pu.displayName}' non ha lo script PowerUpButtonUI!");
            }

            // 3. Aggiungiamo l'azione al click del pulsante
            Button b = buttonObj.GetComponent<Button>();
            if (b != null)
            {
                // Passa il PowerUpEffect al metodo OnPowerUpSelected
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
            // GetRandomPowerUps ora restituisce List<PowerUpEffect>
            List<PowerUpEffect> newOptions = manager.GetRandomPowerUps(3, currentPlayer);
            PopulateChoices(newOptions);
        }
    }
    
    // Accetta il nuovo tipo PowerUpEffect
    private void OnPowerUpSelected(PowerUpEffect pu) 
    {
        if (currentPlayer == null) return;

        // --- MODIFICA CHIAVE QUI ---
        // Ora la UI si limita a dire al giocatore "acquisisci questo potenziamento".
        // Tutta la logica di applicazione e conteggio è centralizzata nel PlayerStats.
        currentPlayer.AcquirePowerUp(pu);
        // --- FINE MODIFICA ---

        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}