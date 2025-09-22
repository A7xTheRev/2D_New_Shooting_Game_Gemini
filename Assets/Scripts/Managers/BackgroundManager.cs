using UnityEngine;
using System.Collections.Generic;
using System;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;

    [Header("Sfondi Disponibili")]
    public List<Texture2D> availableBackgrounds;

    public static event Action OnBackgroundChanged;

    private int currentBackgroundIndex = 0;
    private const string PrefsKey = "SelectedBackgroundIndex";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSelection();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Carica la selezione salvata
    private void LoadSelection()
    {
        currentBackgroundIndex = PlayerPrefs.GetInt(PrefsKey, 0);
        // Assicura che l'indice sia valido
        if (currentBackgroundIndex >= availableBackgrounds.Count)
        {
            currentBackgroundIndex = 0;
        }
    }

    // Salva la selezione
    private void SaveSelection()
    {
        PlayerPrefs.SetInt(PrefsKey, currentBackgroundIndex);
    }

    // Metodo per cambiare lo sfondo (es. dal menu)
    public void CycleNextBackground()
    {
        currentBackgroundIndex++;
        if (currentBackgroundIndex >= availableBackgrounds.Count)
        {
            currentBackgroundIndex = 0;
        }
        SaveSelection();
        OnBackgroundChanged?.Invoke(); // Lancia il segnale!
    }
    
    public void CyclePreviousBackground()
    {
        currentBackgroundIndex--;
        if (currentBackgroundIndex < 0)
        {
            currentBackgroundIndex = availableBackgrounds.Count - 1;
        }
        SaveSelection();
        OnBackgroundChanged?.Invoke(); // Lancia il segnale!
    }
    
    // Metodo per impostare uno sfondo casuale
    public void SetRandomBackground()
    {
        if (availableBackgrounds.Count > 0)
        {
            // --- CORREZIONE APPLICATA QUI ---
            // Specifichiamo di usare UnityEngine.Random
            currentBackgroundIndex = UnityEngine.Random.Range(0, availableBackgrounds.Count);
        }
        // Non salviamo la selezione casuale, ma lanciamo comunque il segnale
        // per aggiornare la UI per la partita corrente.
        OnBackgroundChanged?.Invoke();
    }

    // Metodo che gli altri script useranno per sapere quale sfondo mostrare
    public Texture2D GetCurrentBackgroundTexture()
    {
        if (availableBackgrounds.Count == 0 || currentBackgroundIndex >= availableBackgrounds.Count)
        {
            Debug.LogError("Nessuno sfondo disponibile o indice non valido!");
            return null;
        }
        return availableBackgrounds[currentBackgroundIndex];
    }
}