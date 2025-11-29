using UnityEngine;
using Unity.Cinemachine;

public class FloorGenerator : MonoBehaviour
{
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

    [Header("Item Spawner")]
    public ItemSpawner itemSpawner;

    private GameObject playerInstance;
    private GameObject lastGeneratedFloor;

    // -------------------- Публичные методы --------------------
    public void SpawnFloor() => SpawnFloorByType(FloorManager.FloorCategory.Main);

    public GameObject SpawnFloorByType(FloorManager.FloorCategory type)
    {
        GameObject prefab = null;
        if (type == FloorManager.FloorCategory.Main)
            prefab = mainFloors.Length > 0 ? mainFloors[Random.Range(0, mainFloors.Length)] : null;
        else
            prefab = basements.Length > 0 ? basements[Random.Range(0, basements.Length)] : null;

        if (prefab == null)
        {
            Debug.LogError("Нет префаба для типа: " + type);
            return null;
        }

        // Удаляем прошлый пол
        if (lastGeneratedFloor != null)
            Destroy(lastGeneratedFloor);

        // Создаём новый пол
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        lastGeneratedFloor = floor;

        // Спавн игрока в безопасном месте
        SpawnPlayer(floor);

        // Инициализация предметов
        if (itemSpawner != null)
            itemSpawner.InitializeFromRoom(floor);

        return floor;
    }

    // -------------------- Спавн игрока --------------------
    private void SpawnPlayer(GameObject floor)
    {
        Transform spawnPoint = floor.transform.Find("PlayerSpawnPoint");

        Vector3 spawnPos;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
        }
        else
        {
            // Ищем безопасное место с увеличенным радиусом
            spawnPos = FindSafePosition(floor, 0.6f);
        }

        // Немного сдвигаем игрока вверх, чтобы Rigidbody не залипал
        spawnPos += Vector3.up * 0.3f;

        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            playerInstance.transform.position = spawnPos;
        }

        // Устанавливаем игроку возможность двигаться
        var controller = playerInstance.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMovement(true);

        // Камера остаётся без изменений
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

            // Проверка на пересечение с полом
            if (Physics2D.OverlapCircle(point, radius, floorLayerMask)) continue;

            // Проверка на стены
            if (Physics2D.OverlapCircle(point, radius, wallsLayerMask)) continue;

            // Проверка на двери (тег "Doors")
            Collider2D doorHit = Physics2D.OverlapCircle(point, radius);
            if (doorHit != null && doorHit.CompareTag("Doors")) continue;

            return new Vector3(x, y, 0f);
        }

        // Если не нашли безопасное место, ставим чуть выше центра
        return b.center + Vector3.up * 0.5f;
    }

    // -------------------- Получение игрока извне --------------------
    public GameObject GetPlayerInstance() => playerInstance;
}
