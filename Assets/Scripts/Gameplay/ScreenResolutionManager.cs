using UnityEngine;

public class ScreenResolutionManager : MonoBehaviour
{
    [Header("Настройки разрешения")]
    [SerializeField] private int targetWidth = 768; // Ширина в пикселях (для формата 3:4)
    [SerializeField] private int targetHeight = 1024; // Высота в пикселях (для формата 3:4)
    [SerializeField] private bool fullscreen = false; // Режим окна (false = оконный режим с возможностью черных краев)
    [SerializeField] private bool maintainAspectRatio = true; // Поддерживать соотношение сторон (letterboxing)
    
    private static bool resolutionSet = false; // Флаг для установки разрешения только один раз
    
    private void Awake()
    {
        // Устанавливаем разрешение только один раз при первом запуске
        if (!resolutionSet)
        {
            SetResolution();
            resolutionSet = true;
        }
        
        // Всегда настраиваем соотношение сторон для letterboxing
        if (maintainAspectRatio)
        {
            SetupLetterboxing();
        }
    }
    
    private void Start()
    {
        // Дополнительная проверка после загрузки сцены
        if (maintainAspectRatio)
        {
            SetupLetterboxing();
        }
    }
    
    private void Update()
    {
        // Обновляем letterboxing при изменении размера окна
        if (maintainAspectRatio && Input.GetKeyDown(KeyCode.F11))
        {
            // Переключение полноэкранного режима
            Screen.fullScreen = !Screen.fullScreen;
            SetupLetterboxing();
        }
    }
    
    private void SetResolution()
    {
        Debug.Log($"[ScreenResolutionManager] Устанавливаю разрешение: {targetWidth}x{targetHeight}, Fullscreen: {fullscreen}");
        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
    }
    
    private void SetupLetterboxing()
    {
        float targetAspect = (float)targetWidth / targetHeight; // 3:4 = 0.75
        float windowAspect = (float)Screen.width / Screen.height;
        
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // Если окно шире, чем нужно - добавляем вертикальные черные полосы (pillarbox)
            if (windowAspect > targetAspect)
            {
                float scaleHeight = windowAspect / targetAspect;
                float scaleWidth = 1f;
                float viewportWidth = scaleWidth / scaleHeight;
                float viewportX = (1f - viewportWidth) * 0.5f;
                mainCamera.rect = new Rect(viewportX, 0f, viewportWidth, 1f);
            }
            // Если окно уже или выше, чем нужно - добавляем горизонтальные черные полосы (letterbox)
            else
            {
                float scaleWidth = targetAspect / windowAspect;
                float scaleHeight = 1f;
                float viewportHeight = scaleHeight / scaleWidth;
                float viewportY = (1f - viewportHeight) * 0.5f;
                mainCamera.rect = new Rect(0f, viewportY, 1f, viewportHeight);
            }
            
            Debug.Log($"[ScreenResolutionManager] Letterboxing настроен. Window: {Screen.width}x{Screen.height}, Target: {targetAspect:F2}, Camera rect: {mainCamera.rect}");
        }
    }
    
    // Метод для изменения разрешения во время выполнения
    public void ChangeResolution(int width, int height, bool isFullscreen)
    {
        targetWidth = width;
        targetHeight = height;
        fullscreen = isFullscreen;
        SetResolution();
        if (maintainAspectRatio)
        {
            SetupLetterboxing();
        }
    }
}

