using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string introCutsceneSceneName = "IntroCutscene"; // сцена с интро-катсценой
    
    [Header("Настройки кат-сцены")]
    [SerializeField] private bool skipIntroCutscene = true; // Пропустить интро-катсцену (можно легко вернуть обратно)
    
    [Tooltip("Если skipIntroCutscene = false, будет загружаться кат-сцена")]
    public void StartGame()
    {
        if (skipIntroCutscene)
        {
            // Прямо загружаем main, пропуская кат-сцену
            SceneManager.LoadScene("main");
        }
        else
        {
            // Вместо прямой загрузки main сначала загружаем сцену с видео
            SceneManager.LoadScene(introCutsceneSceneName);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
