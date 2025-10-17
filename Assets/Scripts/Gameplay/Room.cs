using UnityEngine;
using System.Linq;

public class Room : MonoBehaviour
{
    [Header("Имя комнаты (для отладки)")]
    public string roomName;

    [Header("Двери комнаты")]
    public Door[] doors;

    [Header("Коллайдеры комнаты (определяют границы)")]
    public Collider2D[] roomColliders;

    private void Awake()
    {
        // Если не назначены — ищем автоматически
        if (roomColliders == null || roomColliders.Length == 0)
        {
            roomColliders = GetComponentsInChildren<Collider2D>()
                .Where(c => c.enabled)
                .ToArray();

            if (roomColliders.Length == 0)
                Debug.LogWarning($"⚠️ Комната {name} не имеет коллайдеров!");
        }

        // Назначаем ссылки на двери
        if (doors != null)
        {
            foreach (var d in doors)
                if (d != null)
                    d.currentRoom = this;
        }
    }

    public bool ContainsPoint(Vector2 point)
    {
        if (roomColliders == null || roomColliders.Length == 0)
            return false;

        foreach (var c in roomColliders)
        {
            if (c == null) continue;

            // CompositeCollider2D проверяем через bounds
            if (c is CompositeCollider2D)
            {
                if (c.bounds.Contains(point))
                    return true;
            }
            else
            {
                if (c.OverlapPoint(point) || c.bounds.Contains(point))
                    return true;
            }
        }

        return false;
    }

    public Bounds GetRoomBounds()
    {
        if (roomColliders == null || roomColliders.Length == 0)
            return new Bounds(transform.position, Vector3.one * 5f);

        Bounds total = roomColliders[0].bounds;
        foreach (var c in roomColliders)
            if (c != null)
                total.Encapsulate(c.bounds);

        return total;
    }
}
