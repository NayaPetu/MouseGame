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
    public GameObject mainMenuPanel;   // ������ � �������� �������� ����
    public GameObject optionsPanel;    // ������ ��������
    public GameObject aboutPanel;      // ������ "� ���������"

    [Header("Options UI")]
    public Toggle soundToggle;
    public Slider volumeSlider;
    
    [Header("About Panel UI")]
    [TextArea(10, 20)]
    public string aboutText = "О игре:\n\nЭто игра о мыши, которая ищет сыр...\n\nЗдесь вы можете добавить описание вашей игры, информацию о разработчиках, версии игры и другую полезную информацию.";
    public Text aboutTextDisplay; // Текстовое поле для отображения информации в панели "О игре"

    private void Awake()
    {
        Debug.LogError("[MainMenuUI] Awake вызван! Скрипт загружен!");
    }

    private void Start()
    {
        // ���������� ������
        Debug.LogError("[MainMenuUI] Start вызван! Скрипт инициализируется!");
        if (startButton != null)
        {
            Debug.Log("[MainMenuUI] startButton найден, добавляю слушатель OnStartGame");
            startButton.onClick.AddListener(OnStartGame);
        }
        else
        {
            Debug.LogError("[MainMenuUI] startButton НЕ НАЙДЕН! Проверьте назначение в инспекторе!");
        }
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OnOptions);
            Debug.Log("[MainMenuUI] optionsButton найден и настроен");
        }
        else
        {
            Debug.LogError("[MainMenuUI] optionsButton НЕ НАЙДЕН! Проверьте назначение в инспекторе!");
        }
        
        if (aboutButton != null)
        {
            aboutButton.onClick.RemoveAllListeners();
            aboutButton.onClick.AddListener(OnAbout);
            Debug.Log("[MainMenuUI] aboutButton найден и настроен");
        }
        else
        {
            Debug.LogError("[MainMenuUI] aboutButton НЕ НАЙДЕН! Проверьте назначение в инспекторе!");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExit);
            Debug.Log("[MainMenuUI] exitButton найден и настроен");
        }
        else
        {
            Debug.LogError("[MainMenuUI] exitButton НЕ НАЙДЕН! Проверьте назначение в инспекторе!");
        }
        if (backFromOptionsButton != null) backFromOptionsButton.onClick.AddListener(CloseOptions);
        if (backFromAboutButton != null) backFromAboutButton.onClick.AddListener(CloseAbout);

        // ������������� �������
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);

        // -------------------- ��������� ����� --------------------
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // ������������� AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(savedVolume);
            AudioManager.Instance.SetSound(soundOn);

            // ��������� ������ ����
            AudioManager.Instance.PlayMenuMusic();
        }

        // ��������� UI �������� � ��������
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
        
        // Настраиваем текст в панели "О игре"
        SetupAboutText();
    }

    // ---------------- ������ ���� ----------------
    public void OnStartGame()
    {
        // ��������� ����� ����
        Debug.Log("[MainMenuUI] OnStartGame вызван! Загружаю сцену IntroCutscene...");
        
        // Проверка доступности сцены
        if (Application.CanStreamedLevelBeLoaded("IntroCutscene"))
        {
            Debug.Log("[MainMenuUI] Сцена IntroCutscene доступна для загрузки");
            SceneManager.LoadScene("IntroCutscene");
        }
        else
        {
            Debug.LogError("[MainMenuUI] Сцена IntroCutscene НЕ доступна! Проверьте Build Settings! Загружаю main напрямую.");
            SceneManager.LoadScene("main");
        }

        // � ���� ����� ����� �������:
        // AudioManager.Instance.PlayGameMusic();
    }

    public void OnOptions()
    {
        Debug.Log("[MainMenuUI] OnOptions вызван!");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            Debug.Log("[MainMenuUI] Панель настроек активирована");
        }
        else
        {
            Debug.LogError("[MainMenuUI] optionsPanel не назначен!");
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("[MainMenuUI] Главная панель меню деактивирована");
        }
    }

    public void OnAbout()
    {
        Debug.Log("[MainMenuUI] OnAbout вызван!");
        if (aboutPanel != null)
        {
            aboutPanel.SetActive(true);
            Debug.Log("[MainMenuUI] Панель 'О игре' активирована");
        }
        else
        {
            Debug.LogError("[MainMenuUI] aboutPanel не назначен!");
        }
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("[MainMenuUI] Главная панель меню деактивирована");
        }
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
        Debug.Log("[MainMenuUI] Кнопка Exit нажата. Закрываю игру...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    private void SetupAboutText()
    {
        // Если текстовое поле не назначено, пытаемся найти его в панели
        if (aboutTextDisplay == null && aboutPanel != null)
        {
            // Ищем Text компонент в панели "О игре"
            aboutTextDisplay = aboutPanel.GetComponentInChildren<Text>();
            
            // Если не найден Text, пытаемся найти TMPro
            if (aboutTextDisplay == null)
            {
                var tmpText = aboutPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    Debug.LogWarning("[MainMenuUI] Найден TextMeshProUGUI в панели 'О игре', но требуется обычный Text компонент. Создаю Text компонент.");
                }
            }
            
            // Если все еще не найден, создаем новый Text компонент
            if (aboutTextDisplay == null)
            {
                // Ищем Panel внутри About_panel для размещения текста
                Transform panelTransform = aboutPanel.transform.Find("Panel");
                if (panelTransform == null)
                {
                    // Если нет панели, используем саму aboutPanel
                    panelTransform = aboutPanel.transform;
                }
                
                GameObject textObject = new GameObject("AboutText");
                textObject.transform.SetParent(panelTransform, false);
                
                RectTransform textRect = textObject.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(20, 20);
                textRect.offsetMax = new Vector2(-20, -20);
                
                aboutTextDisplay = textObject.AddComponent<Text>();
                try
                {
                    aboutTextDisplay.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                catch
                {
                    // Используем шрифт по умолчанию, если не удалось загрузить
                }
                aboutTextDisplay.fontSize = 20;
                aboutTextDisplay.color = Color.white;
                aboutTextDisplay.alignment = TextAnchor.UpperLeft;
                aboutTextDisplay.horizontalOverflow = HorizontalWrapMode.Wrap;
                aboutTextDisplay.verticalOverflow = VerticalWrapMode.Overflow;
                
                Debug.Log("[MainMenuUI] Создан новый Text компонент для панели 'О игре'");
            }
        }
        
        // Устанавливаем текст, если компонент найден
        if (aboutTextDisplay != null)
        {
            aboutTextDisplay.text = aboutText;
            Debug.Log("[MainMenuUI] Текст установлен в панели 'О игре'");
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] Не удалось найти или создать Text компонент для панели 'О игре'. Убедитесь, что aboutPanel назначен в инспекторе.");
        }
    }
}
