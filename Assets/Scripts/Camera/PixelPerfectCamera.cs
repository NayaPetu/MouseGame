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
        
        lastScreenHeight = Screen.height;
    }
    
    private int lastScreenHeight;
}
