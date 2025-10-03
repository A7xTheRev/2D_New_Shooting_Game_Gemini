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
    public Button rerollButton;

    private PlayerStats currentPlayer;
    private bool rerollUsedThisLevel;

    // --- NUOVA VARIABILE PUBBLICA PER LA COMUNICAZIONE ---
    [HideInInspector]
    public bool hasMadeChoice = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (panel != null) panel.SetActive(false);
    }

    // Accetta una lista del nuovo tipo PowerUpEffect
    public void ShowPowerUpChoices(List<PowerUpEffect> options, PlayerStats player)
    {
        if (options == null || options.Count == 0) 
        {
            // Se non ci sono opzioni, segnala subito di aver "scelto" per non bloccare il gioco.
            hasMadeChoice = true;
            return;
        }

        hasMadeChoice = false; // Resetta il flag all'inizio
        currentPlayer = player;
        panel.SetActive(true);

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
                Debug.LogError($"Il potenziamento '{pu.displayName}' non ha un prefab di pulsante assegnato!");
                continue;
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
        AudioManager.Instance.PlaySound(AudioManager.Instance.powerUpSelectSound);

        // --- MODIFICA CHIAVE: NON RIPRENDERE IL GIOCO, SEGNALA SOLO LA SCELTA ---
        hasMadeChoice = true;
    }

    public void ShowEvolutionChoice(WeaponEvolutionData evolution, PlayerStats player, PlayerController controller)
    {
        hasMadeChoice = false; // Resetta il flag
        currentPlayer = player;
        panel.SetActive(true);

        if (rerollButton != null) rerollButton.gameObject.SetActive(false);

        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        GameObject buttonObj = Instantiate(evolution.evolutionButtonPrefab, buttonsContainer);
        
        PowerUpButtonUI buttonUI = buttonObj.GetComponent<PowerUpButtonUI>();
        if (buttonUI != null)
        {
            // Usiamo il Setup del pulsante per mostrare i dati dell'arma EVOLUTA
            buttonUI.powerUpNameText.text = evolution.evolvedWeapon.weaponName;
            buttonUI.powerUpDescriptionText.text = evolution.evolvedWeapon.description;
            buttonUI.powerUpIcon.sprite = evolution.evolvedWeapon.weaponIcon;
        }

        Button b = buttonObj.GetComponent<Button>();
        if (b != null)
        {
            b.onClick.AddListener(() => OnEvolutionSelected(evolution, controller));
        }
    }

    private void OnEvolutionSelected(WeaponEvolutionData evolution, PlayerController controller)
    {
        if (currentPlayer == null) return;
        
        EvolutionManager.Instance.EvolveWeapon(currentPlayer, controller, evolution);
        AudioManager.Instance.PlaySound(AudioManager.Instance.powerUpSelectSound);

        // --- MODIFICA CHIAVE: NON RIPRENDERE IL GIOCO, SEGNALA SOLO LA SCELTA ---
        hasMadeChoice = true;
    }
        
    // --- NUOVO METODO PER NASCONDERE IL PANNELLO ---
    public void HidePanel()
    {
        if(panel != null)
        {
        panel.SetActive(false);
        }
    }
}