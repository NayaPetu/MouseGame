using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    public Image onImage;
    public Image offImage;
    public AudioSource menuMusic;

    private bool soundOn = true;

    void Start()
    {
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        UpdateImages();

        if (menuMusic != null)
            menuMusic.mute = !soundOn;
    }

    public void OnSoundButtonClicked()
    {
        soundOn = !soundOn;
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();

        if (menuMusic != null)
            menuMusic.mute = !soundOn;

        UpdateImages();
    }

    private void UpdateImages()
    {
        if (onImage != null) onImage.gameObject.SetActive(soundOn);
        if (offImage != null) offImage.gameObject.SetActive(!soundOn);
    }
}
