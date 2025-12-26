using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool friendRescued = false;

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
        
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке сцены main - находим панель заново и сбрасываем состояние
        if (scene.name == "main")
        {
            // Ищем панель проигрыша в новой сцене
            FindGameOverPanel();
            ResetGameState();
        }
        // При загрузке сцены menu - также сбрасываем состояние
        else if (scene.name == "menu")
        {
            ResetGameState();
        }
    }
    
    private void FindGameOverPanel()
    {
        // Всегда ищем панель проигрыша в сцене при загрузке main
        // чтобы получить актуальную ссылку на объект в новой сцене
        GameObject panelObj = GameObject.Find("GameOverPanel");
        if (panelObj != null)
        {
            gameOverPanel = panelObj;
        }
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
    }

    public void OnPlayerCaught()
    {
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        Time.timeScale = 0f; // стоп игра

        // Если панель не найдена, пытаемся найти её
        if (gameOverPanel == null)
        {
            FindGameOverPanel();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // КНОПКА "ЗАНОВО"
    public void RestartGame()
    {
        // ❗️ ОЧИЩАЕМ ВСЁ ПЕРЕД СТАРТОМ
        CleanupGameplayObjects();
        
        // Сбрасываем состояние перед загрузкой меню
        ResetGameState();

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
}
