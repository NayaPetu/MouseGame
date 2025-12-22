using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string introCutsceneSceneName = "IntroCutscene"; // сцена с интро-катсценой

    public void StartGame()
    {
        // Вместо прямой загрузки main сначала загружаем сцену с видео
        SceneManager.LoadScene(introCutsceneSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
