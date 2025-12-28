using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool friendRescued = false;
    
    // Список открытых комнат (по именам комнат)
    private static HashSet<string> openedRooms = new HashSet<string>();

    // 🔹 Текущий этаж игрока
    private FloorManager.FloorCategory currentFloor = FloorManager.FloorCategory.Main;

    public FloorManager.FloorCategory PlayerCurrentFloor
    {
        get => currentFloor;
        set => currentFloor = value;
    }

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Настраиваем разрешение экрана
        SetupScreenResolution();
        
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void SetupScreenResolution()
    {
        // Параметры разрешения для формата 3:4
        int targetWidth = 768;  // Ширина
        int targetHeight = 1024; // Высота (формат 3:4)
        bool fullscreen = false; // Оконный режим
        
        Debug.Log($"[GameManager] Устанавливаю разрешение: {targetWidth}x{targetHeight}");
        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
        
        // Настраиваем letterboxing для поддержания соотношения сторон
        SetupLetterboxing(targetWidth, targetHeight);
    }
    
    private void SetupLetterboxing(int targetWidth, int targetHeight)
    {
        float targetAspect = (float)targetWidth / targetHeight; // 3:4 = 0.75
        float windowAspect = (float)Screen.width / Screen.height;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // Если окно шире, чем нужно - добавляем вертикальные черные полосы (pillarbox)
            if (windowAspect > targetAspect)
            {
                float scaleHeight = windowAspect / targetAspect;
                float viewportWidth = 1f / scaleHeight;
                float viewportX = (1f - viewportWidth) * 0.5f;
                mainCamera.rect = new Rect(viewportX, 0f, viewportWidth, 1f);
            }
            // Если окно уже или выше - добавляем горизонтальные черные полосы (letterbox)
            else
            {
                float scaleWidth = targetAspect / windowAspect;
                float viewportHeight = 1f / scaleWidth;
                float viewportY = (1f - viewportHeight) * 0.5f;
                mainCamera.rect = new Rect(0f, viewportY, 1f, viewportHeight);
            }
            
            // Устанавливаем черный цвет для пустых областей
            mainCamera.backgroundColor = Color.black;
            
            Debug.Log($"[GameManager] Letterboxing настроен. Camera rect: {mainCamera.rect}");
        }
    }
    
    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogError($"[GameManager] OnSceneLoaded вызван для сцены: {scene.name}, режим: {mode}");
        
        // Обновляем letterboxing для новой сцены
        SetupLetterboxing(768, 1024);
        
        // КРИТИЧЕСКАЯ ПРОВЕРКА: если только что загрузили IntroCutscene, но получаем main - это ошибка!
        if (scene.name == "main")
        {
            // Получаем полный стек вызовов для отладки
            Debug.LogError($"[GameManager] КРИТИЧЕСКОЕ: Загружена сцена main! Стек вызовов:");
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
            Debug.LogError(stackTrace.ToString());
            
            // Ищем панель проигрыша в новой сцене
            FindGameOverPanel();
            ResetGameState();
        }
        // При загрузке сцены menu - также сбрасываем состояние
        else if (scene.name == "menu")
        {
            ResetGameState();
        }
        // При загрузке IntroCutscene - ничего не делаем, просто логируем
        else if (scene.name == "IntroCutscene")
        {
            Debug.LogError("[GameManager] Загружена сцена IntroCutscene - ничего не делаю");
        }
        else
        {
            Debug.LogError($"[GameManager] Загружена неизвестная сцена: {scene.name}");
        }
    }
    
    private void FindGameOverPanel()
    {
        // Всегда ищем панель проигрыша в сцене при загрузке main
        // чтобы получить актуальную ссылку на объект в новой сцене
        
        GameObject panelObj = null;
        
        // Пробуем разные варианты имени
        string[] possibleNames = { "GameOverPanel", "Panel_GameOver", "Game Over Panel", "GameOver" };
        foreach (string name in possibleNames)
        {
            panelObj = GameObject.Find(name);
            if (panelObj != null)
            {
                Debug.Log($"[GameManager] Found panel by name: {name}");
                break;
            }
        }
        
        // Если не нашли по имени, ищем все объекты с тегом Canvas и ищем внутри них
        if (panelObj == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                foreach (string name in possibleNames)
                {
                    Transform panelTransform = canvas.transform.Find(name);
                    if (panelTransform == null)
                    {
                        // Пробуем найти дочерний объект с нужным именем рекурсивно
                        panelTransform = FindChildRecursive(canvas.transform, name);
                    }
                    if (panelTransform != null)
                    {
                        panelObj = panelTransform.gameObject;
                        Debug.Log($"[GameManager] Found panel in Canvas by name: {name}");
                        break;
                    }
                }
                if (panelObj != null) break;
            }
        }
        
        // Пробуем найти по компоненту EndGameUI
        if (panelObj == null)
        {
            UnityEngine.Object endGameUI = FindFirstObjectByType<EndGameUI>();
            if (endGameUI != null)
            {
                panelObj = ((MonoBehaviour)endGameUI).gameObject;
                Debug.Log("[GameManager] Found panel by EndGameUI component");
            }
        }
        
        // Поиск через корневые объекты сцены (даже неактивные)
        if (panelObj == null)
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                // Ищем рекурсивно во всех дочерних объектах
                foreach (string name in possibleNames)
                {
                    Transform found = FindChildRecursive(rootObj.transform, name);
                    if (found != null)
                    {
                        panelObj = found.gameObject;
                        Debug.Log($"[GameManager] Found panel in scene root objects by name: {name}");
                        break;
                    }
                }
                if (panelObj != null) break;
            }
        }
        
        // Последняя попытка - поиск по части имени во всех объектах (включая неактивные)
        if (panelObj == null)
        {
            // Используем Resources.FindObjectsOfTypeAll для поиска даже неактивных объектов
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                // Пропускаем префабы, которые не инстанцированы в сцене
                if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                    continue;
                    
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("gameover") || lowerName.Contains("game over") || 
                    lowerName.Contains("endgame") || lowerName.Contains("panel_gameover"))
                {
                    // Проверяем, что это действительно UI панель (имеет CanvasRenderer или RectTransform)
                    if (obj.GetComponent<CanvasRenderer>() != null || obj.GetComponent<RectTransform>() != null)
                    {
                        // Проверяем, что объект в текущей сцене
                        if (obj.scene.name == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                        {
                            panelObj = obj;
                            Debug.Log($"[GameManager] Found panel by partial name search: {obj.name}");
                            break;
                        }
                    }
                }
            }
        }
        
        if (panelObj != null)
        {
            gameOverPanel = panelObj;
            Debug.Log($"[GameManager] GameOverPanel found and assigned: {panelObj.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] GameOverPanel not found in scene! Game over menu will not appear.");
        }
    }
    
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            
            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    
    private void ResetGameState()
    {
        // Сбрасываем timeScale
        Time.timeScale = 1f;
        
        // Скрываем панель проигрыша
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Сбрасываем другие флаги состояния
        friendRescued = false;
        hasKey = false;
        // НЕ сбрасываем openedRooms - они должны сохраняться до конца игры
    }

    public void OnPlayerCaught()
    {
        Debug.Log("[GameManager] OnPlayerCaught called!");
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        Debug.Log("[GameManager] ShowGameOver called!");
        
        // Всегда пытаемся найти панель перед показом
        FindGameOverPanel();

        if (gameOverPanel != null)
        {
            Debug.Log($"[GameManager] Activating gameOverPanel! Panel name: {gameOverPanel.name}, Active: {gameOverPanel.activeSelf}, ActiveInHierarchy: {gameOverPanel.activeInHierarchy}");
            
            // Убеждаемся, что панель активна
            gameOverPanel.SetActive(true);
            
            // Также убеждаемся, что родительский Canvas активен
            Canvas canvas = gameOverPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
                Debug.Log($"[GameManager] Canvas activated: {canvas.gameObject.name}");
            }
            
            // Проверяем, что панель действительно активна
            if (!gameOverPanel.activeInHierarchy)
            {
                Debug.LogWarning("[GameManager] Panel is not active in hierarchy! Checking parent hierarchy...");
                Transform parent = gameOverPanel.transform.parent;
                while (parent != null)
                {
                    if (!parent.gameObject.activeSelf)
                    {
                        Debug.LogWarning($"[GameManager] Found inactive parent: {parent.name}. Activating...");
                        parent.gameObject.SetActive(true);
                    }
                    parent = parent.parent;
                }
            }
            
            Time.timeScale = 0f; // стоп игра ПОСЛЕ активации панели
            Debug.Log($"[GameManager] Game over panel should now be visible. Final state - Active: {gameOverPanel.activeSelf}, ActiveInHierarchy: {gameOverPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[GameManager] gameOverPanel is still null after search! Cannot show game over menu. Trying to find any panel with 'Game' in name...");
            
            // Последняя попытка - найти любой объект с "Game" или "Over" в имени
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("gameover") || obj.name.ToLower().Contains("game over") || obj.name.ToLower().Contains("endgame"))
                {
                    gameOverPanel = obj;
                    gameOverPanel.SetActive(true);
                    
                    // Активируем родительский Canvas, если есть
                    Canvas canvas = gameOverPanel.GetComponentInParent<Canvas>();
                    if (canvas != null)
                        canvas.gameObject.SetActive(true);
                    
                    Time.timeScale = 0f;
                    Debug.Log($"[GameManager] Found panel by name search: {obj.name}");
                    return;
                }
            }
            
            Debug.LogError("[GameManager] Failed to find game over panel! Game will be paused but no menu will appear.");
            Time.timeScale = 0f; // Все равно останавливаем игру
        }
    }

    // КНОПКА "ЗАНОВО"
    public void RestartGame()
    {
        // ❗️ ОЧИЩАЕМ ВСЁ ПЕРЕД СТАРТОМ
        CleanupGameplayObjects();
        
        // Сбрасываем состояние перед загрузкой меню
        ResetGameState();
        
        // Сбрасываем список открытых комнат при перезапуске игры
        ResetOpenedRooms();

        SceneManager.LoadScene("menu");
    }

    private void CleanupGameplayObjects()
    {
        // Уничтожаем FloorManager
        FloorManager fm = FindFirstObjectByType<FloorManager>();
        if (fm != null)
            Destroy(fm.gameObject);

        // Уничтожаем врагов
        EnemyAI enemy = FindFirstObjectByType<EnemyAI>();
        if (enemy != null)
            Destroy(enemy.gameObject);
    }

    public bool hasKey = false;

    public void CollectKey()
    {
        hasKey = true;
    }
    
    // Добавить комнату в список открытых
    public static void MarkRoomAsOpened(string roomName)
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            openedRooms.Add(roomName);
        }
    }
    
    // Проверить, открыта ли комната
    public static bool IsRoomOpened(string roomName)
    {
        return !string.IsNullOrEmpty(roomName) && openedRooms.Contains(roomName);
    }
    
    // Получить список всех открытых комнат
    public static HashSet<string> GetOpenedRooms()
    {
        return new HashSet<string>(openedRooms);
    }
    
    // Сбросить список открытых комнат (при перезапуске игры)
    public static void ResetOpenedRooms()
    {
        openedRooms.Clear();
    }
}
