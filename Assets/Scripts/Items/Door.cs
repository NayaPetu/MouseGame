using UnityEngine;
using Unity.Cinemachine;


public class Door : MonoBehaviour
{
    public Transform targetDoor; // дверь, куда переходит игрок
    public Collider2D roomCollider; // коллайдер комнаты, для камеры

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // перемещаем игрока к целевой двери
            other.transform.position = targetDoor.position;
            
            // переключаем камеру на новую комнату
            Camera.main.GetComponent<CinemachineConfiner2D>().BoundingShape2D = roomCollider;
        }
    }
}
