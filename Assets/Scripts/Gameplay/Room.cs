using UnityEngine;
using System.Linq;

public class Room : MonoBehaviour
{
    [Header("Имя комнаты")]
    public string roomName;

    [Header("Двери комнаты")]
    public Door[] doors;

    [Header("Коллайдеры комнаты")]
    public Collider2D[] roomColliders;

    private void Awake()
    {
        // Авто поиск коллайдеров, если не назначены
        if (roomColliders == null || roomColliders.Length == 0)
            roomColliders = GetComponentsInChildren<Collider2D>()
                .Where(c => c.enabled && !c.isTrigger)
                .ToArray();

        // Связь дверей с этой комнатой
        if (doors != null)
        {
            foreach (var d in doors)
            {
                if (d != null)
                    d.currentRoom = this;
            }
        }
    }

    public bool ContainsPoint(Vector2 point)
    {
        // Проверяем, что roomColliders назначен
        if (roomColliders == null || roomColliders.Length == 0)
            return false;
            
        foreach (var c in roomColliders)
        {
            if (c != null && c.OverlapPoint(point)) return true;
        }
        return false;
    }

    public Bounds GetRoomBounds()
    {
        if (roomColliders == null || roomColliders.Length == 0)
            return new Bounds(transform.position, Vector3.one * 5f);

        // Находим первый валидный коллайдер
        Collider2D firstValidCollider = null;
        foreach (var c in roomColliders)
        {
            if (c != null)
            {
                firstValidCollider = c;
                break;
            }
        }

        // Если нет валидных коллайдеров, возвращаем дефолтные границы
        if (firstValidCollider == null)
            return new Bounds(transform.position, Vector3.one * 5f);

        Bounds b = firstValidCollider.bounds;
        foreach (var c in roomColliders)
        {
            if (c != null)
                b.Encapsulate(c.bounds);
        }

        return b;
    }

    public Vector3 GetRandomPointInRoom()
    {
        Bounds b = GetRoomBounds();
        return new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            0f
        );
    }
}
