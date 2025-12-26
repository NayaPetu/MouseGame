using UnityEngine;

/// <summary>
/// Камера следует за игроком, но её позиция ограничена рамками текущей комнаты (Room).
/// Работает с процедурно сгенерированными комнатами.
/// Скрипт нужно повесить на виртуальную камеру (CinemachineCamera) или обычную ортокамеру.
/// </summary>
public class RoomCameraClamp : MonoBehaviour
{
    [SerializeField] private Transform player;   // Игрок
    [SerializeField] private float smoothTime = 0.15f;

    private Camera cam;
    private Vector3 velocity;

    private void Awake()
    {
        // Берём камеру с этого объекта (виртуальная или обычная)
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null || cam == null) return;

        // Находим комнату, в которой сейчас игрок
        Room currentRoom = FindCurrentRoom(player.position);
        if (currentRoom == null)
        {
            // Если комнату не нашли — просто следуем за игроком
            FollowWithoutClamp();
            return;
        }

        Bounds b = currentRoom.GetRoomBounds();

        // Размер видимой области камеры в юнитах
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        Vector3 targetPos = player.position;
        targetPos.z = transform.position.z; // сохраняем z камеры

        // Если комната меньше камеры — центрируем камеру в комнате
        if (b.size.x <= camHalfWidth * 2f || b.size.y <= camHalfHeight * 2f)
        {
            targetPos.x = b.center.x;
            targetPos.y = b.center.y;
        }
        else
        {
            targetPos.x = Mathf.Clamp(targetPos.x, b.min.x + camHalfWidth, b.max.x - camHalfWidth);
            targetPos.y = Mathf.Clamp(targetPos.y, b.min.y + camHalfHeight, b.max.y - camHalfHeight);
        }

        // Плавное движение камеры
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    private Room FindCurrentRoom(Vector2 playerPos)
    {
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var room in rooms)
        {
            if (room != null && room.ContainsPoint(playerPos))
                return room;
        }
        return null;
    }

    private void FollowWithoutClamp()
    {
        Vector3 targetPos = player.position;
        targetPos.z = transform.position.z;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}