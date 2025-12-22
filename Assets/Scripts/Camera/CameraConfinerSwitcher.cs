using UnityEngine;
using Unity.Cinemachine; // � CM3.x, ���� CM2.x ��: using Cinemachine;

public class CameraConfinerSwitcher : MonoBehaviour
{
    public CinemachineConfiner2D confiner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Если конфайнер не назначен — просто выходим, чтобы не ловить NullReference
            if (confiner == null)
                return;

            Collider2D newShape = GetComponent<Collider2D>();
            if (newShape == null)
                return;

            confiner.BoundingShape2D = newShape;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}
