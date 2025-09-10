using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PowerUpUI : MonoBehaviour
{
    public static PowerUpUI Instance;

    [Header("UI Panel")]
    public GameObject panel;             // Contenitore principale (intestazione + buttonsContainer)
    public Transform buttonsContainer;   // Contenitore solo per i bottoni
    public Button buttonPrefab;          // Prefab del bottone powerup

    private PlayerStats currentPlayer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowPowerUpChoices(List<PowerUp> options, PlayerStats player)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("Nessun powerup disponibile da mostrare!");
            return;
        }

        currentPlayer = player;
        panel.SetActive(true);

        // Blocca il gioco
        Time.timeScale = 0f;

        // Rimuove eventuali bottoni vecchi senza toccare l'intestazione
        foreach (Transform child in buttonsContainer)
            Destroy(child.gameObject);

        // Crea un bottone per ogni opzione
        foreach (PowerUp pu in options)
        {
            Button b = Instantiate(buttonPrefab, buttonsContainer);
            b.GetComponentInChildren<TextMeshProUGUI>().text = pu.displayName;

            PowerUp capturedPU = pu; // cattura variabile locale
            b.onClick.AddListener(() => ApplyPowerUp(capturedPU));
        }
    }

    private void ApplyPowerUp(PowerUp pu)
    {
        if (currentPlayer == null)
        {
            Debug.LogWarning("Player non assegnato!");
            return;
        }

        Debug.Log("CLICK su: " + pu.displayName + " | Player = " + currentPlayer.name);
        pu.Apply(currentPlayer);

        // Chiudi pannello e sblocca il gioco
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}
