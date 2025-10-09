using UnityEngine;
using Unity.Cinemachine;

public class Door : MonoBehaviour
{
    [Header("Целевая дверь/комната")]
    public Transform targetDoor;      // Дверь или точка в целевой комнате
    public Collider2D roomCollider;   // Коллайдер новой комнаты для камеры

    [Header("Телепорт смещение")]
    public Vector3 safeOffset = new Vector3(0.5f, 0f, 0f); // Смещение игрока при телепорте

    private bool teleported = false;  // Флаг, чтобы предотвратить повторный телепорт

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || teleported) return;

        if (targetDoor == null)
        {
            Debug.LogWarning($"targetDoor не назначен на двери {name}!");
            return;
        }

        // 🔹 Телепортируем игрока с безопасным смещением
        other.transform.position = targetDoor.position + safeOffset;

        // 🔹 Обновляем камеру на новую комнату
        if (roomCollider != null)
        {
            CinemachineConfiner2D confiner = Camera.main.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
                confiner.BoundingShape2D = roomCollider;
        }
        else
        {
            Debug.LogWarning($"roomCollider не назначен на двери {name}!");
        }

        // 🔹 Блокируем повторное срабатывание, пока игрок внутри коллайдера
        teleported = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Разблокируем дверь, когда игрок покидает её коллайдер
            teleported = false;
        }
    }
}
