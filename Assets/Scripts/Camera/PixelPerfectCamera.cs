using UnityEngine;

public class PixelPerfectCamera : MonoBehaviour
{
    [SerializeField] private int pixelsPerUnit = 16;
    [SerializeField] private int referenceHeight = 180;
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
        // Обновляем при изменении размера окна
        if (Screen.height != lastScreenHeight)
        {
            UpdateCamera();
        }
    }

    void UpdateCamera()
    {
        // Устанавливаем ортографический режим
        cam.orthographic = true;
        
        // Вычисляем размер ортографической камеры
        float unitsPerPixel = 1f / pixelsPerUnit;
        cam.orthographicSize = (referenceHeight * unitsPerPixel) * 0.5f;
        
        lastScreenHeight = Screen.height;
    }
    
    private int lastScreenHeight;
}
