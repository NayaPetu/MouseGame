using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
    }

    public void OnPlayerCaught()
    {
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        Time.timeScale = 0f; // стоп игра

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // КНОПКА "ЗАНОВО"
    public void RestartGame()
    {
        Time.timeScale = 1f;

        // ❗️ ОЧИЩАЕМ ВСЁ ПЕРЕД СТАРТОМ
        CleanupGameplayObjects();

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

