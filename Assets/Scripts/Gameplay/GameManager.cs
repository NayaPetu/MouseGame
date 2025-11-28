using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject); // если нужно сохранять между сценами
    }

    // Вызывается, когда игрок пойман котом
    public void OnPlayerCaught()
    {
        Debug.Log("Игрок пойман котом! Game Over.");
        GameOver();
    }

    public void GameOver()
    {
        // Например, перезагрузить сцену main
        SceneManager.LoadScene("main");
    }
}
