using UnityEngine; 
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    [Header("Pulsanti Arma")]
    public Button laserButton;
    public Button standardButton; // Puoi rinominare questa variabile in "standardButton" se vuoi, ma non è obbligatorio
    public Button missileButton;

    [Header("FPS Settings")]
    public TMP_Dropdown fpsDropdown; 

    public static event Action<string> OnWeaponChanged;
    private static string selectedWeapon = "Laser";

    void Start()
    {
        UpdateCoinsUI();

        string savedWeapon = PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
        HighlightSelectedWeapon(savedWeapon);
        
        SetupFPSDropdown();
    }

    void OnEnable()
    {
        UpdateCoinsUI();
    }

    public void UpdateCoinsUI()
    {
        if (coinsText != null && ProgressionManager.Instance != null)
        {
            coinsText.text = "Coins: " + ProgressionManager.Instance.GetCoins();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void SelectWeapon(string weaponName)
    {
        selectedWeapon = weaponName;
        PlayerPrefs.SetString("SelectedWeapon", weaponName);
        PlayerPrefs.Save();

        Debug.Log("Arma selezionata: " + weaponName);

        HighlightSelectedWeapon(weaponName);
        OnWeaponChanged?.Invoke(weaponName);
    }

    private void HighlightSelectedWeapon(string weaponName)
    {
        Color selectedColor = Color.green;
        Color normalColor = Color.white;

        if (laserButton != null)
            laserButton.image.color = (weaponName == "Laser") ? selectedColor : normalColor;
        
        // --- MODIFICATO QUI ---
        if (standardButton != null)
            standardButton.image.color = (weaponName == "Standard") ? selectedColor : normalColor;
        
        if (missileButton != null)
            missileButton.image.color = (weaponName == "Missile") ? selectedColor : normalColor;
    }

    public static string GetSelectedWeapon()
    {
        return PlayerPrefs.GetString("SelectedWeapon", selectedWeapon);
    }
    
    // ... (Il resto dello script per gli FPS e il reset rimane invariato) ...
    private void SetupFPSDropdown()
    {
        if (fpsDropdown == null) return;

        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new System.Collections.Generic.List<string> { "Unlimited", "60 FPS", "90 FPS", "120 FPS" });

        int savedFPS = PlayerPrefs.GetInt("TargetFPS", -1);
        int index = FPSValueToIndex(savedFPS);
        fpsDropdown.value = index;
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);

        ApplyFPS(savedFPS);
    }

    private void OnFPSChanged(int index)
    {
        int fps = IndexToFPSValue(index);
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();

        ApplyFPS(fps);
    }

    private void ApplyFPS(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log("FPS limit impostato a: " + (fps == -1 ? "Unlimited" : fps.ToString()));
    }

    private int FPSValueToIndex(int fps)
    {
        switch (fps)
        {
            case 60: return 1;
            case 90: return 2;
            case 120: return 3;
            default: return 0;
        }
    }

    private int IndexToFPSValue(int index)
    {
        switch (index)
        {
            case 1: return 60;
            case 2: return 90;
            case 3: return 120;
            default: return -1;
        }
    }
    
    public void ResetCoinsButton()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.ResetCoins();
            UpdateCoinsUI();
        }
    }
}