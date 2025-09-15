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
            b.GetComponentInChildren<TextMeshProUGUI>().text = pu.displayName;
            b.onClick.AddListener(() => ApplyPowerUp(pu));
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
            List<PowerUp> newOptions = manager.GetRandomPowerUps(3);
            PopulateChoices(newOptions);
        }
    }
    
    private void ApplyPowerUp(PowerUp pu) 
    {
        if (currentPlayer == null) return;
        pu.Apply(currentPlayer);
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}