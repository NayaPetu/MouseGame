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
    
    private int currentImageIndex = 0;
    private bool isTransitioning = false;
    private bool skipRequested = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Debug.Log("[ImageCutscene] Awake вызван! Компонент ImageCutscene инициализируется.");
    }

    private void Start()
    {
        Debug.Log("[ImageCutscene] Start вызван. Проверяю настройки...");
        
        // Проверяем, что imageDisplay назначен
        if (imageDisplay == null)
        {
            Debug.LogError("[ImageCutscene] Image Display не назначен! Кат-сцена не может работать. Укажите Image Display в инспекторе компонента ImageCutscene!");
            return;
        }
        Debug.Log("[ImageCutscene] Image Display назначен: " + imageDisplay.name);

        // Получаем или создаем CanvasGroup для плавных переходов
        canvasGroup = imageDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = imageDisplay.gameObject.AddComponent<CanvasGroup>();
            Debug.Log("[ImageCutscene] CanvasGroup добавлен к Image Display");
        }

        // Проверяем, что есть изображения
        if (images == null || images.Count == 0)
        {
            Debug.LogWarning("[ImageCutscene] Нет изображений для кат-сцены! Добавьте изображения в список Images в инспекторе. Загружаю следующую сцену через 1 секунду.");
            Invoke(nameof(LoadNextScene), 1f);
            return;
        }
        Debug.Log($"[ImageCutscene] Найдено изображений: {images.Count}");

        // Начинаем показ первого изображения
        StartCoroutine(ShowCutscene());
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
                Debug.LogWarning($"[ImageCutscene] Изображение {i} не назначено! Пропускаю.");
                continue;
            }

            // Устанавливаем спрайт
            imageDisplay.sprite = images[i].image;
            
            // Показываем изображение с плавным появлением (можно прервать кликом)
            yield return StartCoroutine(FadeIn(i));
            
            // Если был запрошен пропуск во время fadeIn, переходим к следующему
            if (skipRequested && currentImageIndex != i)
            {
                continue;
            }
            
            // Ждем указанное время (или до клика мыши)
            float elapsedTime = 0f;
            float displayTime = images[i].displayTime;
            
            while (elapsedTime < displayTime && currentImageIndex == i && !skipRequested)
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

    private IEnumerator FadeIn(int imageIndex)
    {
        if (canvasGroup == null) yield break;
        
        isTransitioning = true;
        canvasGroup.alpha = 0f;
        
        while (canvasGroup.alpha < 1f && currentImageIndex == imageIndex && !skipRequested)
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        isTransitioning = false;
    }

    private IEnumerator FadeOut(int imageIndex)
    {
        if (canvasGroup == null) yield break;
        
        isTransitioning = true;
        
        while (canvasGroup.alpha > 0f && currentImageIndex == imageIndex && !skipRequested)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        isTransitioning = false;
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[ImageCutscene] Next Scene Name не назначен!");
        }
    }
}

