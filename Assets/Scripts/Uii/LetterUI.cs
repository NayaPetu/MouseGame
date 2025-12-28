using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LetterUI : MonoBehaviour
{
    public static LetterUI Instance;

    [Header("UI Элементы")]
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private TextMeshProUGUI letterTitle; // Заголовок письма
    [SerializeField] private TextMeshProUGUI letterText;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject darkBackground;

    private bool isLetterOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Инициализация UI
        if (letterPanel != null)
        {
            letterPanel.SetActive(false);
        }

        if (darkBackground != null)
        {
            darkBackground.SetActive(false);
        }

        // Настройка кнопки закрытия
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseLetter);
        }
    }

    private void Update()
    {
        // Закрытие письма по Escape
        if (isLetterOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLetter();
        }
    }

    public void ShowLetter(LetterData letterData)
    {
        if (letterData == null)
        {
            Debug.LogWarning("Попытка показать письмо с null данными!");
            return;
        }

        // Устанавливаем заголовок письма
        if (letterTitle != null)
        {
            letterTitle.text = !string.IsNullOrEmpty(letterData.title) ? letterData.title : "Письмо друга";
        }
        else
        {
            Debug.LogWarning("letterTitle не назначен в LetterUI!");
        }

        // Устанавливаем текст письма
        if (letterText != null)
        {
            letterText.text = letterData.text;
        }
        else
        {
            Debug.LogWarning("letterText не назначен в LetterUI!");
        }

        if (letterPanel != null)
        {
            letterPanel.SetActive(true);
        }

        if (darkBackground != null)
        {
            darkBackground.SetActive(true);
        }

        isLetterOpen = true;
        Time.timeScale = 0f; // Останавливаем время
    }

    public void CloseLetter()
    {
        if (letterPanel != null)
        {
            letterPanel.SetActive(false);
        }

        if (darkBackground != null)
        {
            darkBackground.SetActive(false);
        }

        isLetterOpen = false;
        Time.timeScale = 1f; // Возобновляем время
    }

    public bool IsLetterOpen()
    {
        return isLetterOpen;
    }
}

