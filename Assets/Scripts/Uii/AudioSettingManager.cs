using UnityEngine;

public class AudioSettingsManager : MonoBehaviour
{
    public AudioSource backgroundMusic;

    void Start()
    {
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
            backgroundMusic.mute = !soundOn;
        }
    }
}
