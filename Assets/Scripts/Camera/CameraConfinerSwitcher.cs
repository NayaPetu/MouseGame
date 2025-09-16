using UnityEngine;
using Unity.Cinemachine; // в CM3.x, если CM2.x то: using Cinemachine;

public class CameraConfinerSwitcher : MonoBehaviour
{
    public CinemachineConfiner2D confiner;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collider2D newShape = GetComponent<Collider2D>();
            confiner.BoundingShape2D = newShape;
            confiner.InvalidateBoundingShapeCache();
        }
    }
}
