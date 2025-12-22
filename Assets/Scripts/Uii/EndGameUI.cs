using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    public void BackToMenu()
    {
        SceneManager.LoadScene("menu"); // или любая другая сцена
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Игра завершена.");
    }
}
