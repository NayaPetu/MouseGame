using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string introCutsceneSceneName = "IntroCutscene"; // сцена с интро-катсценой
    
    [Header("Настройки кат-сцены")]
    [SerializeField] private bool skipIntroCutscene = false; // Пропустить интро-катсцену (можно легко вернуть обратно)
    
    private const string INTRO_WATCHED_KEY = "IntroCutsceneWatched"; // Ключ для PlayerPrefs
    
    [Header("Кнопка начала игры")]
    [SerializeField] private Button startButton; // Кнопка "Начать"
    
    private static bool isLoadingScene = false; // Флаг для предотвращения повторных вызовов
    
    private void Awake()
    {
        Debug.LogError("[MenuButtons] Awake вызван! Скрипт загружен!");
        
        // Сбрасываем флаг при загрузке новой сцены
        isLoadingScene = false;
        
        // Подписываемся на событие загрузки сцены для сброса флага
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Сбрасываем флаг при загрузке новой сцены
        isLoadingScene = false;
        Debug.LogError($"[MenuButtons] Сцена загружена: {scene.name}, флаг isLoadingScene сброшен");
        
        // Если загрузили меню - сбрасываем флаг просмотра кат-сцены и запускаем музыку
        if (scene.name == "menu")
        {
            PlayerPrefs.DeleteKey(INTRO_WATCHED_KEY);
            PlayerPrefs.Save();
            Debug.LogError("[MenuButtons] Флаг просмотра кат-сцены сброшен при загрузке меню");
            
            // Запускаем музыку меню
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMenuMusic();
                Debug.LogError("[MenuButtons] Музыка меню запущена");
            }
        }
    }
    
    private void Start()
    {
        Debug.LogError("[MenuButtons] Start вызван!");
        
        // Сбрасываем флаг просмотра кат-сцены при загрузке меню (чтобы показывать кат-сцену при каждом запуске)
        PlayerPrefs.DeleteKey(INTRO_WATCHED_KEY);
        PlayerPrefs.Save();
        Debug.LogError("[MenuButtons] Флаг просмотра кат-сцены сброшен для нового запуска игры");
        
        // Если кнопка не назначена в Inspector, пытаемся найти её автоматически
        if (startButton == null)
        {
            Debug.LogError("[MenuButtons] startButton не назначен, ищем Button_start...");
            GameObject buttonObj = GameObject.Find("Button_start");
            if (buttonObj != null)
            {
                startButton = buttonObj.GetComponent<Button>();
                Debug.LogError($"[MenuButtons] Кнопка Button_start найдена: {startButton != null}");
            }
        }
        
        // Настраиваем кнопку программно
        if (startButton != null)
        {
            Debug.LogError("[MenuButtons] Настраиваю кнопку startButton на вызов StartGame()");
            
            // ВАЖНО: НЕ отключаем MainMenuUI сразу, чтобы его Start() успел выполниться
            // и настроить все кнопки (включая optionsButton и aboutButton)
            // MainMenuUI будет отключен позже, если потребуется
            
            // Проверяем Persistent Listeners (настроены в Inspector)
            int persistentCount = startButton.onClick.GetPersistentEventCount();
            Debug.LogError($"[MenuButtons] Количество Persistent Listeners на кнопке: {persistentCount}");
            
            if (persistentCount > 0)
            {
                Debug.LogError("[MenuButtons] ВНИМАНИЕ: В Inspector настроены Persistent Listeners! Удаляю их через Reflection!");
                
                // Удаляем Persistent Listeners через Reflection
                RemoveAllPersistentListeners(startButton.onClick);
                
                // Проверяем, что удалились
                persistentCount = startButton.onClick.GetPersistentEventCount();
                Debug.LogError($"[MenuButtons] Количество Persistent Listeners после удаления: {persistentCount}");
            }
            
            // Очищаем динамические слушатели
            startButton.onClick.RemoveAllListeners();
            
            // Добавляем наш метод
            startButton.onClick.AddListener(OnButtonClicked);
            
            Debug.LogError("[MenuButtons] Кнопка настроена!");
        }
        else
        {
            Debug.LogError("[MenuButtons] ОШИБКА: startButton не найден! Проверьте назначение в Inspector!");
        }
    }
    
    // Промежуточный метод для перехвата клика и блокировки других слушателей
    private void OnButtonClicked()
    {
        Debug.LogError("[MenuButtons] OnButtonClicked вызван! Блокирую другие слушатели и вызываю StartGame()");
        
        // Блокируем повторные вызовы
        if (isLoadingScene)
        {
            Debug.LogError("[MenuButtons] Сцена уже загружается, игнорирую повторный клик!");
            return;
        }
        
        // Отключаем кнопку сразу, чтобы предотвратить повторные клики
        if (startButton != null)
        {
            startButton.interactable = false;
            // Также пытаемся удалить все слушатели еще раз (на всякий случай)
            RemoveAllPersistentListeners(startButton.onClick);
            startButton.onClick.RemoveAllListeners();
        }
        
        // Вызываем основной метод (флаг будет установлен внутри StartGame)
        StartGame();
    }
    
    [Tooltip("Если skipIntroCutscene = false, будет загружаться кат-сцена")]
    public void StartGame()
    {
        // Блокируем повторные вызовы
        if (isLoadingScene)
        {
            Debug.LogError("[MenuButtons] StartGame: Сцена уже загружается, игнорирую повторный вызов!");
            return;
        }
        
        isLoadingScene = true; // Устанавливаем флаг
        Debug.LogError("[MenuButtons] StartGame ВЫЗВАН! Загружаю кат-сцену!");
        
        // НЕ отключаем MainMenuUI здесь - пусть кнопки работают нормально
        // MainMenuUI управляет своими панелями и кнопками корректно
        
        // Проверяем, нужно ли пропустить кат-сцену
        // Примечание: флаг просмотра сбрасывается при загрузке меню, поэтому кат-сцена будет показываться при каждом запуске игры
        if (skipIntroCutscene)
        {
            // Прямо загружаем main, пропуская кат-сцену
            Debug.LogError($"[MenuButtons] Пропускаем кат-сцену (skipIntroCutscene: {skipIntroCutscene}), загружаем main");
            
            // Скрываем Canvas из menu перед загрузкой main
            HideMenuCanvas();
            
            SceneManager.LoadScene("main");
        }
        else
        {
            // Вместо прямой загрузки main сначала загружаем сцену с кат-сценой
            Debug.LogError($"[MenuButtons] Загружаем кат-сцену: {introCutsceneSceneName}");
            
            // Проверка что сцена существует
            if (Application.CanStreamedLevelBeLoaded(introCutsceneSceneName))
            {
                Debug.LogError($"[MenuButtons] Сцена {introCutsceneSceneName} доступна, загружаю...");
                Debug.LogError($"[MenuButtons] Текущая сцена перед загрузкой: {SceneManager.GetActiveScene().name}");
                
                // Скрываем Canvas из menu перед загрузкой кат-сцены
                HideMenuCanvas();
                
                // Используем LoadSceneMode.Single для гарантии, что предыдущая сцена выгрузится
                SceneManager.LoadScene(introCutsceneSceneName, LoadSceneMode.Single);
                Debug.LogError($"[MenuButtons] SceneManager.LoadScene вызван для {introCutsceneSceneName} с режимом Single");
                
                // Сбрасываем флаг после небольшой задержки (на случай ошибки)
                StartCoroutine(ResetLoadingFlagAfterDelay(2f));
            }
            else
            {
                Debug.LogError($"[MenuButtons] ОШИБКА: Сцена {introCutsceneSceneName} НЕ доступна! Проверьте Build Settings!");
                Debug.LogError($"[MenuButtons] Загружаю main вместо кат-сцены");
                
                HideMenuCanvas();
                isLoadingScene = false; // Сбрасываем флаг перед загрузкой main
                SceneManager.LoadScene("main");
            }
        }
    }
    
    private void HideMenuCanvas()
    {
        // Находим и скрываем все Canvas на сцене menu
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.scene.name == "menu")
            {
                Debug.LogError($"[MenuButtons] Скрываю Canvas перед загрузкой новой сцены: {canvas.gameObject.name}");
                canvas.gameObject.SetActive(false);
            }
        }
    }
    
    private System.Collections.IEnumerator ResetLoadingFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isLoadingScene && SceneManager.GetActiveScene().name != introCutsceneSceneName)
        {
            Debug.LogError("[MenuButtons] Сцена не загрузилась за отведенное время, сбрасываю флаг!");
            isLoadingScene = false;
        }
    }

    // Метод для удаления всех Persistent Listeners через Reflection
    private void RemoveAllPersistentListeners(UnityEventBase unityEvent)
    {
        try
        {
            // Получаем поле m_PersistentCalls через Reflection
            FieldInfo persistentCallsField = typeof(UnityEventBase).GetField("m_PersistentCalls", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (persistentCallsField != null)
            {
                object persistentCalls = persistentCallsField.GetValue(unityEvent);
                
                if (persistentCalls != null)
                {
                    // Получаем метод Clear() через Reflection
                    MethodInfo clearMethod = persistentCalls.GetType().GetMethod("Clear", 
                        BindingFlags.Public | BindingFlags.Instance);
                    
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(persistentCalls, null);
                        Debug.LogError("[MenuButtons] Persistent Listeners удалены через Reflection!");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MenuButtons] Ошибка при удалении Persistent Listeners: {e.Message}");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
