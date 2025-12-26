using Unity.Cinemachine;
using UnityEngine;

public class FloorGenerator : MonoBehaviour
{
    public static FloorGenerator Instance;

    [Header("Main Floors")]
    public GameObject[] mainFloors;

    [Header("Basements")]
    public GameObject[] basements;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    [Header("Layers")]
    public LayerMask floorLayerMask;
    public LayerMask wallsLayerMask;

    [Header("Friend Spawner")]
    public FriendSpawner friendSpawner;

    private GameObject playerInstance;
    private GameObject lastGeneratedFloor;

    void Awake()
    {
        Instance = this;
    }

    // -------------------- Совместимость со старым кодом --------------------
    public void SpawnFloor()
    {
        SpawnFloorByType(FloorManager.FloorCategory.Main);
    }

    // -------------------- Основной метод --------------------
    public GameObject SpawnFloorByType(FloorManager.FloorCategory type)
    {
        GameObject prefab = type == FloorManager.FloorCategory.Main
            ? mainFloors.Length > 0 ? mainFloors[Random.Range(0, mainFloors.Length)] : null
            : basements.Length > 0 ? basements[Random.Range(0, basements.Length)] : null;

        if (prefab == null)
        {
            Debug.LogError("Нет префаба для типа: " + type);
            return null;
        }

        // Скрываем предыдущий этаж
        if (lastGeneratedFloor != null)
            lastGeneratedFloor.SetActive(false);

        // Создаём новый этаж
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        lastGeneratedFloor = floor;

        // Спавн игрока
        SpawnPlayer(floor);

        // Спавн друзей
        if (type == FloorManager.FloorCategory.Basement && friendSpawner != null)
            friendSpawner.TrySpawnFriend(floor);

        // Спавн предметов
        var spawner = floor.GetComponentInChildren<ItemSpawner>();
        if (spawner != null)
            spawner.InitializeFromRoom(floor);

        return floor;
    }

    // -------------------- Спавн игрока --------------------
    private void SpawnPlayer(GameObject floor)
    {
        Transform spawnPoint = floor.transform.Find("PlayerSpawnPoint");
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : FindSafePosition(floor, 0.6f);
        spawnPos += Vector3.up * 0.3f;

        if (playerInstance == null)
        {
            // Создаём нового игрока
            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            var controller = playerInstance.GetComponent<PlayerController>();
            if (controller != null)
                controller.SetMovement(true); // включаем движение
        }
        else
        {
            // Телепортируем существующего игрока
            playerInstance.transform.position = spawnPos;
            // ⚠ не трогаем canMove
        }

        // Камера остаётся привязанной
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;
    }

    // -------------------- Поиск безопасной позиции --------------------
    private Vector3 FindSafePosition(GameObject floor, float radius)
    {
        Collider2D floorCollider = floor.GetComponentInChildren<Collider2D>();
        if (!floorCollider) return floor.transform.position;

        Bounds b = floorCollider.bounds;
        float margin = 0.5f;

        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(b.min.x + margin, b.max.x - margin);
            float y = Random.Range(b.min.y + margin, b.max.y - margin);
            Vector2 point = new Vector2(x, y);

            if (Physics2D.OverlapCircle(point, radius, floorLayerMask)) continue;
            if (Physics2D.OverlapCircle(point, radius, wallsLayerMask)) continue;

            Collider2D doorHit = Physics2D.OverlapCircle(point, radius);
            if (doorHit != null && doorHit.CompareTag("Doors")) continue;

            return new Vector3(x, y, 0f);
        }

        return b.center + Vector3.up * 0.5f;
    }

    // -------------------- Получение игрока извне --------------------
    public GameObject GetPlayerInstance() => playerInstance;
}
