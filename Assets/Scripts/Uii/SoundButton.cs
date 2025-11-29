using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image onImage;   // картинка для включённого звука
    public Image offImage;  // картинка для выключенного звука

    void Start()
    {
        UpdateImages();
    }

    // Метод вызывается при нажатии на кнопку
    public void OnSoundButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            // переключаем состояние звука
            bool newState = AudioManager.Instance.musicSource.mute;
            AudioManager.Instance.SetSound(newState); // true = включить звук
        }

        UpdateImages();
    }

    // Обновляем отображение картинок
    private void UpdateImages()
    {
        if (AudioManager.Instance != null)
        {
            bool soundOn = !AudioManager.Instance.musicSource.mute;

            if (onImage != null) onImage.gameObject.SetActive(soundOn);
            if (offImage != null) offImage.gameObject.SetActive(!soundOn);
        }
    }
}
