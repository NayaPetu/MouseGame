using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button startButton;
    public Button optionsButton;
    public Button aboutButton;
    public Button exitButton;

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject aboutPanel;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartGame);
        optionsButton.onClick.AddListener(OnOptions);
        aboutButton.onClick.AddListener(OnAbout);
        exitButton.onClick.AddListener(OnExit);

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    public void OnStartGame()
    {
        // Загружаем сцену с игрой (например "GameScene")
        SceneManager.LoadScene("GameScene");
    }

    public void OnOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void OnAbout()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(!aboutPanel.activeSelf);
    }

    public void OnExit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
