using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(TMP_Dropdown))]
public class FPSSelector : MonoBehaviour
{
    private TMP_Dropdown fpsDropdown;
    
    // Rimosso l'evento 'OnFPSSettingChanged', non è più necessario per questo sistema.

    void Awake()
    {
        fpsDropdown = GetComponent<TMP_Dropdown>();
    }

    void Start()
    {
        // Pulisce e popola le opzioni del menu a tendina
        fpsDropdown.ClearOptions();
        fpsDropdown.AddOptions(new System.Collections.Generic.List<string> { "60 FPS", "90 FPS", "120 FPS" });
        
        // Collega la funzione OnFPSChanged all'evento di cambio valore
        fpsDropdown.onValueChanged.AddListener(OnFPSChanged);

        // Aggiorna la visuale per mostrare l'impostazione salvata
        UpdateVisuals();
    }

    // OnEnable/OnDisable sono stati rimossi perché non più necessari.

    private void OnFPSChanged(int index)
    {
        // Converte l'indice del menu (0, 1, 2) in un valore di FPS (60, 90, 120)
        int fps = IndexToFPSValue(index);
        
        // Salva la nuova preferenza del giocatore
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.Save();
        
        // Applica immediatamente la nuova impostazione
        ApplyFPS(fps);
    }

    // Questo metodo aggiorna solo l'interfaccia grafica del menu a tendina
    private void UpdateVisuals()
    {
        if (fpsDropdown == null) return;
        
        // Legge il valore salvato (o 60 di default)
        int savedFPS = PlayerPrefs.GetInt("TargetFPS", 60);
        
        // Converte il valore FPS nell'indice corrispondente del menu
        int index = FPSValueToIndex(savedFPS);
        
        // Imposta il menu a tendina su quell'indice senza attivare l'evento OnFPSChanged
        fpsDropdown.SetValueWithoutNotify(index);
    }

    // Questo metodo applica fisicamente il limite di FPS
    private void ApplyFPS(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log($"[FPSSelector] Limite FPS modificato a: {fps}");
    }

    // Funzioni di supporto per convertire indice <-> valore
    private int FPSValueToIndex(int fps)
    {
        switch (fps)
        {
            case 90: return 1;
            case 120: return 2;
            default: return 0; // 60 FPS è l'indice 0
        }
    }

    private int IndexToFPSValue(int index)
    {
        switch (index)
        {
            case 1: return 90;
            case 2: return 120;
            default: return 60; // 60 FPS è il valore di default
        }
    }
}