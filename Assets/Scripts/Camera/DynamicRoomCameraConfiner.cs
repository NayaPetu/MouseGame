using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Динамически подстраивает форму Confiner-а под комнату, в которой находится игрок.
/// Работает с процедурно сгенерированными комнатами (Room) без ручной настройки на префабах.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class DynamicRoomCameraConfiner : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private Transform player;

    // Как часто обновлять комнату (в секундах), чтобы не делать поиск каждый кадр
    [SerializeField] private float roomCheckInterval = 0.1f;

    private BoxCollider2D confinerCollider;
    private Room currentRoom;
    private float roomCheckTimer;

    private void Awake()
    {
        confinerCollider = GetComponent<BoxCollider2D>();
        confinerCollider.isTrigger = false;

        if (confiner != null)
        {
            confiner.BoundingShape2D = confinerCollider;
            confiner.InvalidateBoundingShapeCache();
        }
    }

    private void Start()
    {
        // Если игрок не задан в инспекторе — попробуем найти по тегу
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
        }
    }

    private void Update()
    {
        if (player == null || confiner == null) return;

        roomCheckTimer -= Time.deltaTime;
        if (roomCheckTimer > 0f) return;
        roomCheckTimer = roomCheckInterval;

        UpdateCurrentRoom();
    }

    private void UpdateCurrentRoom()
    {
        // Ищем комнату, в которой сейчас стоит игрок
        Room[] rooms = FindObjectsOfType<Room>();

        Room newRoom = null;
        Vector2 playerPos = player.position;

        foreach (var room in rooms)
        {
            if (room != null && room.ContainsPoint(playerPos))
            {
                newRoom = room;
                break;
            }
        }

        if (newRoom == null || newRoom == currentRoom)
            return;

        currentRoom = newRoom;
        ApplyRoomBounds(currentRoom);
    }

    private void ApplyRoomBounds(Room room)
    {
        Bounds b = room.GetRoomBounds();

        // Переносим бокс-коллайдер так, чтобы он совпадал с границами комнаты
        // Коллайдер находится на отдельном объекте (например, "CameraConfinerRoot") в мире.
        Vector3 worldCenter = b.center;
        Vector3 worldSize = b.size;

        // Пересчитываем в локальные координаты объекта, на котором висит этот скрипт
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);

        confinerCollider.offset = localCenter;
        confinerCollider.size = worldSize;

        confiner.BoundingShape2D = confinerCollider;
        confiner.InvalidateBoundingShapeCache();
    }
}



