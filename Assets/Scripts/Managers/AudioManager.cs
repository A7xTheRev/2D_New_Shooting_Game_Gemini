using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sorgente Audio (Musica)")]
    public AudioSource bgmSource;

    [Header("Impostazioni Pool Effetti Sonori")]
    [Tooltip("Quante 'casse' audio creare per gli effetti sonori. Aumenta se i suoni si perdono nelle scene più caotiche.")]
    public int sfxPoolSize = 30;

    [Header("Controllo Volumi")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;

    [Header("Libreria Musicale")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;

    [Header("Effetti Sonori (SFX)")]
    public AudioClip playerShootSound;
    public AudioClip enemyHitSound;
    public AudioClip enemyDeathSound;
    public AudioClip playerHitSound;
    public AudioClip levelUpSound;
    public AudioClip abilityActivateSound;
    public AudioClip powerUpSelectSound;

    private List<AudioSource> sfxPool;
    private int sfxPoolIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxPool = new List<AudioSource>();
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject sfxPlayer = new GameObject("SFX_Player_" + i);
                sfxPlayer.transform.SetParent(this.transform);
                AudioSource source = sfxPlayer.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(source);
            }
            
            bgmVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            SetBGMVolume(bgmVolume);
            SetSFXVolume(sfxVolume);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") { PlayMusic(mainMenuMusic); }
        else if (scene.name == "GameScene") { PlayMusic(gameplayMusic); }
    }
    
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (bgmSource == null || musicClip == null || (bgmSource.clip == musicClip && bgmSource.isPlaying)) return;
        StopAllCoroutines();
        StartCoroutine(FadeMusic(musicClip, loop));
    }

    private System.Collections.IEnumerator FadeMusic(AudioClip newClip, bool loop)
    {
        float fadeDuration = 1.0f;
        float startVolume = bgmSource.volume;
        while (bgmSource.volume > 0) { bgmSource.volume -= startVolume * Time.deltaTime / fadeDuration; yield return null; }
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.loop = loop;
        bgmSource.Play();
        while (bgmSource.volume < bgmVolume) { bgmSource.volume += bgmVolume * Time.deltaTime / fadeDuration; yield return null; }
        bgmSource.volume = bgmVolume;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null || sfxPool == null || sfxPool.Count == 0) return;
        sfxPool[sfxPoolIndex].PlayOneShot(clip);
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Count;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) { bgmSource.volume = bgmVolume; }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxPool != null)
        {
            foreach (AudioSource source in sfxPool) { source.volume = sfxVolume; }
        }
    }
}