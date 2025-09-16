using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Dropdown))]
public class FPSSelector : MonoBehaviour
{
    private TMP_Dropdown fpsDropdown;
    public static event Action OnFPSSettingChanged;

    void Awake()
    {
        fpsDropdown = GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        fpsDropdown.ClearOptions();
        // MODIFICATO QUI: Rimossa l'opzione "Unlimited"
        fpsDropdown.AddOptions(new System.Collections.Generic.List<string> { "60 FPS", "90 FPS", "120 FPS" });
        
        UpdateVisuals();
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
    }

    void OnEnable()
    {
        OnFPSSettingChanged += UpdateVisuals;
        UpdateVisuals();
    }

    void OnDisable()
    {
        OnFPSSettingChanged -= UpdateVisuals;
    }

    private void OnFPSChanged(int index)
    {
        int fps = IndexToFPSValue(index);
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
        ApplyFPS(fps);
        OnFPSSettingChanged?.Invoke();
    }

    private void UpdateVisuals()
    {
        if (fpsDropdown == null) return;
        // MODIFICATO QUI: Il valore di default ora è 60
        int savedFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        int index = FPSValueToIndex(savedFPS);
        fpsDropdown.SetValueWithoutNotify(index);
    }

    private void ApplyFPS(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log("Limite FPS impostato a: " + fps.ToString());
    }

    // MODIFICATO QUI: Logica degli indici aggiornata
    private int FPSValueToIndex(int fps)
    {
        switch (fps)
        {
            case 90: return 1;
            case 120: return 2;
            default: return 0; // 60 FPS è ora l'indice 0
        }
    }

    // MODIFICATO QUI: Logica dei valori aggiornata
    private int IndexToFPSValue(int index)
    {
        switch (index)
        {
            case 1: return 90;
            case 2: return 120;
            default: return 60; // 60 FPS è ora il valore di default
        }
    }
}