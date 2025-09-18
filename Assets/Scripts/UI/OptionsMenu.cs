using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        // Carica i valori salvati all'avvio e imposta gli slider
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        musicVolumeSlider.value = musicVolume;
        AudioManager.Instance.SetBGMVolume(musicVolume);

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        sfxVolumeSlider.value = sfxVolume;
        AudioManager.Instance.SetSFXVolume(sfxVolume);

        // Aggiunge i "listener" per rilevare quando l'utente muove lo slider
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    public void OnMusicVolumeChanged(float value)
    {
        // Comunica il nuovo valore all'AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
        // Salva la preferenza del giocatore
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // Metodo da collegare al pulsante "Chiudi"
    public void CloseOptionsMenu()
    {
        // Salva le preferenze prima di chiudere
        PlayerPrefs.Save();
        gameObject.SetActive(false);

        // Se siamo in gioco, toglie la pausa
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
}