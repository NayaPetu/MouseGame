using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("main"); // твоя игровая сцена
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
