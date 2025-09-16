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
        fpsDropdown.AddOptions(new System.Collections.Generic.List<string> { "Unlimited", "60 FPS", "90 FPS", "120 FPS" });
        UpdateVisuals();
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);
    }

    void OnEnable()
    {
        OnFPSSettingChanged += UpdateVisuals;
        UpdateVisuals(); // Aggiorna subito quando il pannello diventa visibile
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
        int savedFPS = PlayerPrefs.GetInt("TargetFPS", -1);
        int index = FPSValueToIndex(savedFPS);
        fpsDropdown.SetValueWithoutNotify(index);
    }

    private void ApplyFPS(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log("Limite FPS impostato a: " + (fps == -1 ? "Unlimited" : fps.ToString()));
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
}