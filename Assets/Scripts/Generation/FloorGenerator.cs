using UnityEngine;
using Unity.Cinemachine;
using System.Linq;

public class FloorGenerator : MonoBehaviour
{
    [Header("Floor Prefabs")]
    public GameObject[] floorPrefabs;

    [Header("Player / Enemy")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    [Header("Layers")]
    public LayerMask floorLayerMask;

    private GameObject playerInstance;
    private GameObject enemyInstance;
    private Room[] allRooms;

    [Header("Настройки спавна")]
    public bool chooseRandomRoomForPlayer = true;
    public bool chooseRandomRoomForEnemy = true;
    public int playerRoomIndex = 0; // индекс комнаты (если не рандом)
    public int enemyRoomIndex = 1;

    // --- Основной запуск генерации ---
    public void SpawnFloor()
    {
        if (floorPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ Нет префабов комнат для спавна!");
            return;
        }

        // 1️⃣ Создаём этаж
        GameObject prefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 2️⃣ Получаем список всех комнат на этаже
        allRooms = floor.GetComponentsInChildren<Room>(true);
        if (allRooms == null || allRooms.Length == 0)
        {
            Debug.LogError("❌ На этаже не найдено ни одной комнаты!");
            return;
        }

        // 3️⃣ Выбираем комнату для игрока
        Room playerRoom = chooseRandomRoomForPlayer
            ? allRooms[Random.Range(0, allRooms.Length)]
            : allRooms[Mathf.Clamp(playerRoomIndex, 0, allRooms.Length - 1)];

        // 4️⃣ Выбираем комнату для врага
        Room enemyRoom = chooseRandomRoomForEnemy
            ? allRooms[Random.Range(0, allRooms.Length)]
            : allRooms[Mathf.Clamp(enemyRoomIndex, 0, allRooms.Length - 1)];

        // Чтобы не оказались в одной комнате
        if (enemyRoom == playerRoom && allRooms.Length > 1)
        {
            enemyRoom = allRooms.FirstOrDefault(r => r != playerRoom);
        }

        // 5️⃣ Спавним игрока
        Vector3 playerPos = GetRandomPointInRoom(playerRoom);
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);

        // Камера следует за игроком
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // 6️⃣ Спавним врага
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = GetRandomPointInRoom(enemyRoom);
            enemyInstance = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }

        Debug.Log($"✅ Игрок в {playerRoom.roomName}, враг в {enemyRoom.roomName}");
    }

    // --- Получаем случайную точку в комнате ---
    private Vector3 GetRandomPointInRoom(Room room)
    {
        if (room == null)
        {
            Debug.LogWarning("⚠️ Комната не указана для спавна!");
            return Vector3.zero;
        }

        Bounds b = room.GetRoomBounds();
        float safeMargin = 0.5f;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            float x = Random.Range(b.min.x + safeMargin, b.max.x - safeMargin);
            float y = Random.Range(b.min.y + safeMargin, b.max.y - safeMargin);
            Vector3 pos = new Vector3(x, y, 0f);

            // Проверяем, не в стене ли
            Collider2D hit = Physics2D.OverlapCircle(pos, 0.3f, LayerMask.GetMask("Walls"));
            if (hit == null)
                return pos;
        }

        Debug.LogWarning($"⚠️ Не удалось найти свободную позицию в {room.roomName}, берём центр");
        return b.center;
    }
}
