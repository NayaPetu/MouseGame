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
    public Button backFromOptionsButton;
    public Button backFromAboutButton;

    [Header("Panels")]
    public GameObject mainMenuPanel;   // панель с кнопками главного меню
    public GameObject optionsPanel;    // панель настроек
    public GameObject aboutPanel;      // панель "о программе"

    [Header("Options UI")]
    public Toggle soundToggle;
    public Slider volumeSlider;

    [Header("Audio")]
    public AudioSource menuMusic;

    private void Start()
    {
        // Подключаем кнопки
        if (startButton != null) startButton.onClick.AddListener(OnStartGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptions);
        if (aboutButton != null) aboutButton.onClick.AddListener(OnAbout);
        if (exitButton != null) exitButton.onClick.AddListener(OnExit);
        if (backFromOptionsButton != null) backFromOptionsButton.onClick.AddListener(CloseOptions);
        if (backFromAboutButton != null) backFromAboutButton.onClick.AddListener(CloseAbout);

        // Инициализация панелей
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
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

    // ---------------- Кнопки меню ----------------
    public void OnStartGame()
    {
        // Загружаем сцену игры
        SceneManager.LoadScene("main");
    }

    public void OnOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    public void OnAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void CloseAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------------- Настройки звука ----------------
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
