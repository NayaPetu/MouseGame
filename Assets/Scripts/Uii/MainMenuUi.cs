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

        // -------------------- Настройки звука --------------------
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // Устанавливаем AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(savedVolume);
            AudioManager.Instance.SetSound(soundOn);

            // Запускаем музыку меню
            AudioManager.Instance.PlayMenuMusic();
        }

        // Настройка UI слайдера и тумблера
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetVolume);
        }

        if (soundToggle != null)
        {
            soundToggle.isOn = soundOn;
            soundToggle.onValueChanged.AddListener(AudioManager.Instance.SetSound);
        }
    }

    // ---------------- Кнопки меню ----------------
    public void OnStartGame()
    {
        // Загружаем сцену игры
        SceneManager.LoadScene("main");

        // В игре позже можно вызвать:
        // AudioManager.Instance.PlayGameMusic();
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
}
