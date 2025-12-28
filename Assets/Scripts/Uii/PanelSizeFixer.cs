using UnityEngine;

/// <summary>
/// Скрипт для настройки фиксированного размера UI панелей.
/// Присоедините этот скрипт к панели и установите желаемые размеры в инспекторе.
/// </summary>
public class PanelSizeFixer : MonoBehaviour
{
    [Header("Фиксированный размер панели")]
    [Tooltip("Ширина панели в пикселях")]
    public float fixedWidth = 800f;
    
    [Tooltip("Высота панели в пикселях")]
    public float fixedHeight = 600f;
    
    [Header("Позиция")]
    [Tooltip("Позиция по X (0 = центр)")]
    public float positionX = 0f;
    
    [Tooltip("Позиция по Y (0 = центр)")]
    public float positionY = 0f;
    
    [Header("Опции")]
    [Tooltip("Применить настройки при запуске")]
    public bool applyOnStart = true;
    
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError($"[PanelSizeFixer] Компонент RectTransform не найден на {gameObject.name}!");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyFixedSize();
        }
    }

    /// <summary>
    /// Применяет фиксированный размер к панели
    /// </summary>
    public void ApplyFixedSize()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[PanelSizeFixer] Не удалось найти RectTransform на {gameObject.name}!");
                return;
            }
        }

        // Устанавливаем якоря в центр (фиксированная позиция)
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Устанавливаем фиксированный размер
        rectTransform.sizeDelta = new Vector2(fixedWidth, fixedHeight);

        // Устанавливаем позицию
        rectTransform.anchoredPosition = new Vector2(positionX, positionY);

        Debug.Log($"[PanelSizeFixer] Применен фиксированный размер {fixedWidth}x{fixedHeight} к {gameObject.name}");
    }

    /// <summary>
    /// Сбрасывает панель к растягиванию на весь экран
    /// </summary>
    public void ResetToStretch()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Debug.Log($"[PanelSizeFixer] Панель {gameObject.name} сброшена к растягиванию на весь экран");
    }

    // Метод для вызова из инспектора (кнопка в редакторе)
    [ContextMenu("Применить фиксированный размер")]
    private void ApplyFixedSizeContextMenu()
    {
        ApplyFixedSize();
    }

    [ContextMenu("Сбросить к растягиванию")]
    private void ResetToStretchContextMenu()
    {
        ResetToStretch();
    }
}

