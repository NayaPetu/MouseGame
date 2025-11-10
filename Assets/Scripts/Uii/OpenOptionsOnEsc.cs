using UnityEngine;

public class OpenOptionsOnEsc : MonoBehaviour
{
    [Header("Панель настроек")]
    public GameObject optionsPanel; // сюда перетащи свой Options_panel

    [Header("Затемняющий фон (опционально)")]
    public GameObject darkBackground; // сюда перетащи DarkBackground из Canvas

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);

        if (darkBackground != null)
            darkBackground.SetActive(true);

        Time.timeScale = 0f; // ставим игру на паузу
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);

        if (darkBackground != null)
            darkBackground.SetActive(false);

        Time.timeScale = 1f; // возобновляем игру
    }
}
