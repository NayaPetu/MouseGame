using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image onImage;
    public Image offImage;

    [Header("Audio")]
    public AudioSource menuMusic;

    private bool soundOn = true;

    void Start()
    {
        // Загружаем состояние звука
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // Обновляем отображение
        UpdateImages();

        // Применяем к музыке
        if (menuMusic != null)
            menuMusic.mute = !soundOn;
    }

    public void OnSoundButtonClicked()
    {
        soundOn = !soundOn;

        // Сохраняем состояние
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();

        // Включаем/выключаем музыку
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
