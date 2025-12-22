using UnityEngine;

public class PixelPerfectCamera : MonoBehaviour
{
    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private int referenceHeight = 180;

    // Целевое соотношение сторон (как в старых фильмах/мониторах)
    [SerializeField] private float targetAspectWidth = 4f;
    [SerializeField] private float targetAspectHeight = 3f;
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
        // ��������� ��� ��������� ������� ����
        if (Screen.height != lastScreenHeight)
        {
            UpdateCamera();
        }
    }

    void UpdateCamera()
    {
        // ������������� ��������������� �����
        cam.orthographic = true;
        
        // ��������� ������ ��������������� ������
        float unitsPerPixel = 1f / pixelsPerUnit;
        cam.orthographicSize = (referenceHeight * unitsPerPixel) * 0.5f;

        // Принудительное соотношение сторон 4:3 с «чёрными полосами»
        float targetAspect = targetAspectWidth / targetAspectHeight; // 4/3
        float windowAspect = (float)Screen.width / Screen.height;

        // Если окно шире, чем 4:3 — добавляем вертикальные чёрные полосы (pillarbox)
        if (windowAspect > targetAspect)
        {
            float scale = targetAspect / windowAspect;
            float viewportWidth = scale;
            float viewportX = (1f - viewportWidth) * 0.5f;
            cam.rect = new Rect(viewportX, 0f, viewportWidth, 1f);
        }
        // Если окно уже, чем 4:3 — добавляем горизонтальные чёрные полосы (letterbox)
        else if (windowAspect < targetAspect)
        {
            float scale = windowAspect / targetAspect;
            float viewportHeight = scale;
            float viewportY = (1f - viewportHeight) * 0.5f;
            cam.rect = new Rect(0f, viewportY, 1f, viewportHeight);
        }
        else
        {
            // Экран уже в нужном соотношении — занимаем всё пространство
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }

        lastScreenHeight = Screen.height;
    }
    
    private int lastScreenHeight;
}
