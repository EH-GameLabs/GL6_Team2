using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    [Header("SOURCES")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SOUNDS")]
    [SerializeField] private AudioClip webThrow;
    [SerializeField] private AudioClip waterDrop;
    [SerializeField] private AudioClip lifeLost;
    [SerializeField] private AudioClip candlestickOn;
    [SerializeField] private AudioClip candlestickOff;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip win;
    [SerializeField] private AudioClip candyGrabbed;
    //[SerializeField] private AudioClip button;

    [Header("MUSICS")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip mainMenuMusic;

    private void Start()
    {
        //SetButtonSound();
        PlayMusic(mainMenuMusic);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void PlayMusic(AudioClip music)
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        musicSource.clip = music;
        musicSource.Play();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
        sfxSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
        sfxSource.UnPause();
    }

    public void PLaySFXSound(AudioClip clip)
    {
        if (clip != null)
        {
            //sfxSource.Stop();
            sfxSource.PlayOneShot(clip);
        }
    }

    /*private void SetButtonSound()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(() => PLaySFXSound(button));
        }
    }*/

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }
    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
}
