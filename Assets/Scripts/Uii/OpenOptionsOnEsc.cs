using UnityEngine;

public class OpenOptionsOnEsc : MonoBehaviour
{
    [Header("������ ��������")]
    public GameObject optionsPanel; // ���� �������� ���� Options_panel

    [Header("����������� ��� (�����������)")]
    public GameObject darkBackground; // ���� �������� DarkBackground �� Canvas

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
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
        if (optionsPanel == null)
        {
            Debug.LogWarning("[OpenOptionsOnEsc] optionsPanel is null!");
            return;
        }
        
        optionsPanel.SetActive(true);

        if (darkBackground != null)
            darkBackground.SetActive(true);

        Time.timeScale = 0f; // ������ ���� �� �����
    }

    public void CloseOptions()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("[OpenOptionsOnEsc] optionsPanel is null!");
            return;
        }
        
        optionsPanel.SetActive(false);

        if (darkBackground != null)
            darkBackground.SetActive(false);

        Time.timeScale = 1f; // ������������ ����
    }
}
