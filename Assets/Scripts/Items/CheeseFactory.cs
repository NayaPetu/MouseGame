using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CheeseFactory : MonoBehaviour
{
    [SerializeField] private string endCutsceneSceneName = "EndCutscene"; // сцена с финальной катсценой
    
    [Header("UI для сообщений")]
    [SerializeField] private GameObject messagePanel; // Панель для отображения сообщений
    [SerializeField] private Text messageText; // Текст сообщения
    
    private bool playerNearby = false;
    private bool showingMessage = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void Update()
    {
        if (!playerNearby || showingMessage) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (PlayerInventory.Instance.HasRecipe())
            {
                // Проверяем, спасена ли подруга
                if (GameManager.Instance != null && GameManager.Instance.friendRescued)
                {
                    // Подруга спасена - запускаем концовку
                    PlayerInventory.Instance.UseRecipe();
                    Debug.Log("Рецепт использован в CheeseFactory! Подруга спасена, запускаем концовку!");
                    EndGame();
                }
                else
                {
                    // Подруга не спасена - показываем сообщение
                    Debug.Log("Подруга не спасена! Показываю сообщение.");
                    ShowMessage("Найди Лулу!");
                }
            }
            else
            {
                Debug.Log("У тебя нет рецепта!");
            }
        }
    }

    private void ShowMessage(string message)
    {
        if (showingMessage) return;
        
        showingMessage = true;
        
        // Ищем или создаем UI элементы для сообщения
        if (messagePanel == null || messageText == null)
        {
            SetupMessageUI();
        }
        
        if (messageText != null)
        {
            messageText.text = message;
            if (messagePanel != null)
            {
                messagePanel.SetActive(true);
            }
            
            // Скрываем сообщение через 3 секунды
            StartCoroutine(HideMessageAfterDelay(3f));
        }
    }
    
    private void SetupMessageUI()
    {
        // Пытаемся найти Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[CheeseFactory] Canvas не найден для отображения сообщения!");
            showingMessage = false;
            return;
        }
        
        // Ищем существующую панель
        if (messagePanel == null)
        {
            GameObject panelObj = GameObject.Find("MessagePanel");
            if (panelObj == null)
            {
                // Создаем новую панель
                messagePanel = new GameObject("MessagePanel");
                messagePanel.transform.SetParent(canvas.transform, false);
                
                RectTransform panelRect = messagePanel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.sizeDelta = new Vector2(400, 100);
                panelRect.anchoredPosition = new Vector2(0, 100);
                
                Image panelImage = messagePanel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.8f);
            }
            else
            {
                messagePanel = panelObj;
            }
        }
        
        // Ищем текст
        if (messageText == null)
        {
            messageText = messagePanel.GetComponentInChildren<Text>();
            if (messageText == null)
            {
                // Создаем текст
                GameObject textObj = new GameObject("MessageText");
                textObj.transform.SetParent(messagePanel.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                messageText = textObj.AddComponent<Text>();
                messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                messageText.fontSize = 36;
                messageText.color = Color.white;
                messageText.alignment = TextAnchor.MiddleCenter;
                messageText.text = "";
            }
        }
        
        // Скрываем панель по умолчанию
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    
    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        
        showingMessage = false;
    }

    private void EndGame()
    {
        // Загружаем сцену с финальной катсценой
        SceneManager.LoadScene(endCutsceneSceneName);
    }
}
