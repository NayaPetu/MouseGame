using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button startButton;
    public Button optionsButton;
    public Button aboutButton;
    public Button exitButton;

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject aboutPanel;

    [Header("Options UI")]
    public Toggle soundToggle;
    public Slider volumeSlider;
    public Button backFromOptionsButton;

    [Header("Audio")]
    public AudioSource menuMusic;

    private void Start()
    {
        // Подключаем кнопки
        startButton.onClick.AddListener(OnStartGame);
        optionsButton.onClick.AddListener(OnOptions);
        aboutButton.onClick.AddListener(OnAbout);
        exitButton.onClick.AddListener(OnExit);

        if (backFromOptionsButton != null)
            backFromOptionsButton.onClick.AddListener(CloseOptions);

        // Панели
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);

        // Настройки звука
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        if (menuMusic != null)
        {
            menuMusic.volume = savedVolume;
            menuMusic.mute = !soundOn;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (soundToggle != null)
        {
            soundToggle.isOn = soundOn;
            soundToggle.onValueChanged.AddListener(OnSoundToggle);
        }
    }

    public void OnStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void OnAbout()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OnExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ======= Настройки звука =======

    public void OnSoundToggle(bool value)
    {
        if (menuMusic != null)
            menuMusic.mute = !value;

        PlayerPrefs.SetInt("SoundOn", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnVolumeChanged(float value)
    {
        if (menuMusic != null)
            menuMusic.volume = value;

        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}
