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
    }
    
    private void Start()
    {
        Debug.LogError("[MenuButtons] Start вызван!");
        
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
            
            // КРИТИЧЕСКИ ВАЖНО: Отключаем MainMenuUI, если он есть
            MainMenuUI mainMenuUI = FindFirstObjectByType<MainMenuUI>();
            if (mainMenuUI != null)
            {
                Debug.LogError($"[MenuButtons] Найден MainMenuUI! ОТКЛЮЧАЮ его, чтобы избежать конфликта! GameObject: {mainMenuUI.gameObject.name}");
                mainMenuUI.enabled = false; // Отключаем компонент
            }
            
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
        
        // КРИТИЧЕСКИ ВАЖНО: Отключаем MainMenuUI окончательно
        MainMenuUI mainMenuUI = FindFirstObjectByType<MainMenuUI>();
        if (mainMenuUI != null)
        {
            Debug.LogError("[MenuButtons] Окончательно отключаю MainMenuUI перед загрузкой сцены!");
            mainMenuUI.enabled = false;
        }
        
        if (skipIntroCutscene)
        {
            // Прямо загружаем main, пропуская кат-сцену
            Debug.LogError("[MenuButtons] Пропускаем кат-сцену, загружаем main");
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
                isLoadingScene = false; // Сбрасываем флаг перед загрузкой main
                SceneManager.LoadScene("main");
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
