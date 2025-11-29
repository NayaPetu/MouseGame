using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip introAnimationMusic;
    public AudioClip endAnimationMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Загружаем сохраненные настройки звука
        float volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        if (musicSource != null)
        {
            musicSource.volume = volume;
            musicSource.mute = !soundOn;
        }
    }

    // ---------------- Методы для проигрывания музыки ----------------

    public void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;
        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameMusic()
    {
        if (musicSource == null || gameMusic == null) return;
        musicSource.clip = gameMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayIntroAnimationMusic()
    {
        if (musicSource == null || introAnimationMusic == null) return;
        musicSource.clip = introAnimationMusic;
        musicSource.loop = false; // обычно для анимаций без зацикливания
        musicSource.Play();
    }

    public void PlayEndAnimationMusic()
    {
        if (musicSource == null || endAnimationMusic == null) return;
        musicSource.clip = endAnimationMusic;
        musicSource.loop = false;
        musicSource.Play();
    }

    // ---------------- Изменение громкости и звука ----------------

    public void SetVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;

        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }

    public void SetSound(bool on)
    {
        if (musicSource != null)
            musicSource.mute = !on;

        PlayerPrefs.SetInt("SoundOn", on ? 1 : 0);
        PlayerPrefs.Save();
    }
}
