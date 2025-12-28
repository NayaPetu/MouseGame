using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CutsceneImage
{
    [Header("Изображение")]
    public Sprite image; // Спрайт изображения для кат-сцены
    
    [Header("Время показа (секунды)")]
    public float displayTime = 3f; // Время показа этого изображения в секундах
}

public class ImageCutscene : MonoBehaviour
{
    [Header("Настройки кат-сцены")]
    [SerializeField] private List<CutsceneImage> images = new List<CutsceneImage>(); // Список изображений
    [SerializeField] private Image imageDisplay; // UI Image компонент для отображения спрайтов
    [SerializeField] private string nextSceneName = "main"; // Имя сцены, которая загрузится после кат-сцены
    
    [Header("Настройки переходов")]
    [SerializeField] private float fadeSpeed = 2f; // Скорость появления/исчезновения
    [SerializeField] private bool useFadeEffect = false; // Использовать эффект плавного появления (false = мгновенное появление)
    [SerializeField] private Color backgroundColor = Color.black; // Цвет фона (по умолчанию черный)
    
    [Header("Кнопка пропуска")]
    [SerializeField] private Button skipButton; // Кнопка для пропуска кат-сцены
    
    private int currentImageIndex = 0;
    private bool skipRequested = false;
    private CanvasGroup canvasGroup;
    private Image backgroundImage; // Фоновое изображение для черного фона
    private const string INTRO_WATCHED_KEY = "IntroCutsceneWatched"; // Ключ для PlayerPrefs

    private void Awake()
    {
        // Debug.LogError("[ImageCutscene] Awake вызван! Компонент ImageCutscene инициализируется.");
        // Debug.LogError($"[ImageCutscene] Текущая сцена: {SceneManager.GetActiveScene().name}");
    }
    
    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        // Debug.LogError($"[ImageCutscene] Start вызван! Текущая сцена: {currentScene}");
        
        // Запускаем музыку для кат-сцены
        if (AudioManager.Instance != null)
        {
            if (currentScene == "IntroCutscene")
            {
                AudioManager.Instance.PlayIntroAnimationMusic();
                // Debug.LogError("[ImageCutscene] Музыка интро-кат-сцены запущена");
            }
            else if (currentScene == "EndCutscene")
            {
                AudioManager.Instance.PlayEndAnimationMusic();
                // Debug.LogError("[ImageCutscene] Музыка финальной кат-сцены запущена");
            }
        }
        
        // Debug.LogError("[ImageCutscene] Проверяю настройки...");
        
        // Проверяем, что imageDisplay назначен
        if (imageDisplay == null)
        {
            // Debug.LogError("[ImageCutscene] ОШИБКА: Image Display не назначен! Кат-сцена не может работать. Укажите Image Display в инспекторе компонента ImageCutscene!");
            // Debug.LogError("[ImageCutscene] Загружаю следующую сцену через 1 секунду из-за отсутствия Image Display.");
            Invoke(nameof(LoadNextScene), 1f);
            return;
        }
        // Debug.LogError($"[ImageCutscene] Image Display назначен: {imageDisplay.name}");
        
        // Убеждаемся, что Image компонент включен и GameObject активен
        if (!imageDisplay.gameObject.activeInHierarchy)
        {
            imageDisplay.gameObject.SetActive(true);
            // Debug.LogError("[ImageCutscene] Image Display GameObject был неактивен! Активировал его.");
        }
        if (!imageDisplay.enabled)
        {
            imageDisplay.enabled = true;
            // Debug.LogError("[ImageCutscene] Image Display компонент был выключен! Включил его.");
        }

        // Настраиваем черный фон
        SetupBlackBackground();

        // Получаем или создаем CanvasGroup для плавных переходов
        canvasGroup = imageDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = imageDisplay.gameObject.AddComponent<CanvasGroup>();
            // Debug.LogError("[ImageCutscene] CanvasGroup добавлен к Image Display");
        }
        
        // Если fade эффект отключен, сразу устанавливаем alpha = 1
        if (!useFadeEffect && canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Проверяем, что есть изображения
        if (images == null || images.Count == 0)
        {
            // Debug.LogError("[ImageCutscene] ОШИБКА: Нет изображений для кат-сцены! Список Images пустой или null!");
            // Debug.LogError("[ImageCutscene] Добавьте изображения в список Images в инспекторе компонента ImageCutscene!");
            // Debug.LogError("[ImageCutscene] Загружаю следующую сцену через 1 секунду из-за отсутствия изображений.");
            Invoke(nameof(LoadNextScene), 1f);
            return;
        }
        // Debug.LogError($"[ImageCutscene] Найдено изображений: {images.Count}");

        // Проверяем Canvas
        Canvas canvas = imageDisplay.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        if (canvas != null)
        {
            // Debug.LogError($"[ImageCutscene] Canvas найден: {canvas.name}, активен: {canvas.gameObject.activeSelf}, активен в иерархии: {canvas.gameObject.activeInHierarchy}");
            if (!canvas.gameObject.activeInHierarchy)
            {
                canvas.gameObject.SetActive(true);
                // Debug.LogError("[ImageCutscene] Canvas был неактивен! Активировал его.");
            }
        }
        else
        {
            // Debug.LogError("[ImageCutscene] ОШИБКА: Canvas не найден!");
        }

        // Настраиваем кнопку пропуска
        SetupSkipButton();

        // Начинаем показ первого изображения
        // Debug.LogError("[ImageCutscene] Запускаю корутину ShowCutscene()...");
        StartCoroutine(ShowCutscene());
    }
    
    private void SetupSkipButton()
    {
        // Если кнопка не назначена, пытаемся найти её автоматически
        if (skipButton == null)
        {
            GameObject buttonObj = GameObject.Find("Button_Skip");
            if (buttonObj != null)
            {
                skipButton = buttonObj.GetComponent<Button>();
                // Debug.LogError($"[ImageCutscene] Кнопка Button_Skip найдена: {skipButton != null}");
            }
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipCutscene);
            skipButton.gameObject.SetActive(true); // Показываем кнопку
            // Debug.LogError("[ImageCutscene] Кнопка пропуска настроена!");
        }
        else
        {
            // Debug.LogWarning("[ImageCutscene] Кнопка пропуска не найдена. Можно пропустить кат-сцену кликом мыши.");
        }
    }
    
    private void SkipCutscene()
    {
        // Debug.LogError("[ImageCutscene] Кнопка пропуска нажата! Пропускаю кат-сцену.");
        skipRequested = true;
        currentImageIndex = images.Count; // Устанавливаем индекс за пределы, чтобы завершить цикл
        LoadNextScene();
    }

    private void Update()
    {
        // Обработка клика мыши для досрочного перехода
        if (Input.GetMouseButtonDown(0))
        {
            SkipToNextImage();
        }
    }

    private IEnumerator ShowCutscene()
    {
        for (int i = 0; i < images.Count; i++)
        {
            currentImageIndex = i;
            skipRequested = false;
            
            // Проверяем, что изображение назначено
            if (images[i].image == null)
            {
                // Debug.LogWarning($"[ImageCutscene] Изображение {i} не назначено! Пропускаю.");
                continue;
            }

            // Устанавливаем спрайт
            imageDisplay.sprite = images[i].image;
            // Debug.LogError($"[ImageCutscene] Установлен спрайт для изображения {i}: {images[i].image?.name ?? "NULL"}");
            
            // Убеждаемся, что Image видимый
            if (canvasGroup != null && canvasGroup.alpha < 0.01f && !useFadeEffect)
            {
                canvasGroup.alpha = 1f;
            }
            imageDisplay.color = Color.white; // Убеждаемся, что цвет белый (полная видимость)
            
            // Показываем изображение с плавным появлением (можно прервать кликом)
            yield return StartCoroutine(FadeIn(i));
            
            // Если был запрошен пропуск, выходим
            if (skipRequested)
            {
                break;
            }
            
            // Ждем указанное время (или до клика мыши)
            float elapsedTime = 0f;
            while (elapsedTime < images[i].displayTime && currentImageIndex == i && !skipRequested)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Если мы все еще на этом изображении (не было клика), скрываем его
            if (currentImageIndex == i && !skipRequested)
            {
                yield return StartCoroutine(FadeOut(i));
            }
        }

        // Все изображения показаны, загружаем следующую сцену
        LoadNextScene();
    }
    
    private void SkipToNextImage()
    {
        // Запрашиваем пропуск текущего изображения
        skipRequested = true;
        currentImageIndex++;
    }

    private void SetupBlackBackground()
    {
        // Устанавливаем черный цвет фона камеры
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
            // Debug.LogError($"[ImageCutscene] Цвет фона камеры установлен: {backgroundColor}");
        }
        
        // Ищем Canvas в иерархии
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            // Ищем существующее фоновое изображение или создаем новое
            Image[] allImages = canvas.GetComponentsInChildren<Image>();
            Image existingBackground = null;
            
            foreach (Image img in allImages)
            {
                if (img != imageDisplay && img.transform.GetSiblingIndex() < imageDisplay.transform.GetSiblingIndex())
                {
                    existingBackground = img;
                    break;
                }
            }
            
            if (existingBackground != null)
            {
                backgroundImage = existingBackground;
            }
            else
            {
                // Создаем новое фоновое изображение
                GameObject bgObject = new GameObject("BlackBackground");
                bgObject.transform.SetParent(canvas.transform, false);
                bgObject.transform.SetAsFirstSibling(); // Размещаем за всеми элементами
                
                RectTransform bgRect = bgObject.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                bgRect.anchoredPosition = Vector2.zero;
                
                backgroundImage = bgObject.AddComponent<Image>();
            }
            
            // Устанавливаем цвет фона
            backgroundImage.color = backgroundColor;
            
            // Debug.LogError($"[ImageCutscene] Фоновое изображение установлено с цветом: {backgroundColor}");
        }
        else
        {
            // Debug.LogWarning("[ImageCutscene] Canvas не найден, невозможно установить фоновое изображение!");
        }
    }
    
    private IEnumerator FadeIn(int imageIndex)
    {
        if (canvasGroup == null) yield break;
        
        // Если fade эффект отключен, сразу показываем изображение
        if (!useFadeEffect)
        {
            canvasGroup.alpha = 1f;
            yield break;
        }
        
        canvasGroup.alpha = 0f;
        
        while (canvasGroup.alpha < 1f && currentImageIndex == imageIndex && !skipRequested)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut(int imageIndex)
    {
        if (canvasGroup == null) yield break;
        
        // Если fade эффект отключен, сразу скрываем изображение
        if (!useFadeEffect)
        {
            canvasGroup.alpha = 0f;
            yield break;
        }
        
        while (canvasGroup.alpha > 0f && currentImageIndex == imageIndex && !skipRequested)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    private void LoadNextScene()
    {
        // Отмечаем, что кат-сцена была просмотрена
        PlayerPrefs.SetInt(INTRO_WATCHED_KEY, 1);
        PlayerPrefs.Save();
        // Debug.LogError("[ImageCutscene] Кат-сцена отмечена как просмотренная. В следующий раз она будет пропущена.");
        
        // Скрываем кнопку пропуска
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }
        
        // Debug.LogError($"[ImageCutscene] LoadNextScene вызван! Загружаю сцену: {nextSceneName}");
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // Debug.LogError("[ImageCutscene] ОШИБКА: Next Scene Name не назначен!");
        }
    }
}
