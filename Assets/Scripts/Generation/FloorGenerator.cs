using UnityEngine;
using Unity.Cinemachine;

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

    [Header("Item Spawner")]
    public ItemSpawner itemSpawner; // Ссылка на спавнер предметов
    public int itemsToSpawn = 5;     // Количество предметов заспавнить

    private GameObject playerInstance;

    public void SpawnFloor()
    {
        if (floorPrefabs.Length == 0) return;

        // Выбираем случайную комнату
        GameObject prefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // Спавн игрока
        Vector3 playerPos = CreateSafeSpawnPosition(floor, 0.3f);
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);

        // Камера следует за игроком
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // Спавн врага
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = CreateSafeSpawnPosition(floor, 0.3f);
            Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }

        // Спавн предметов
        if (itemSpawner != null)
        {
            Collider2D floorCollider = floor.GetComponentInChildren<Collider2D>();
            if (floorCollider != null)
            {
                Bounds b = floorCollider.bounds;
                itemSpawner.roomMin = b.min;
                itemSpawner.roomMax = b.max;
                itemSpawner.itemsToSpawn = itemsToSpawn;

                itemSpawner.SpawnItems();
            }
            else
            {
                Debug.LogWarning("⚠️ У комнаты нет Collider2D для спавна предметов!");
            }
        }
    }

    // Создание безопасной позиции на полу
    private Vector3 CreateSafeSpawnPosition(GameObject room, float radius)
    {
        Collider2D floorCollider = room.GetComponentInChildren<Collider2D>();
        if (floorCollider == null)
        {
            Debug.LogWarning($"⚠️ У комнаты {room.name} нет Collider2D для пола!");
            return room.transform.position;
        }

        Bounds b = floorCollider.bounds;
        float safeMargin = 0.5f;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            float x = Random.Range(b.min.x + safeMargin, b.max.x - safeMargin);
            float y = Random.Range(b.min.y + safeMargin, b.max.y - safeMargin);
            Vector2 rayOrigin = new Vector2(x, y + 5f);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 10f, floorLayerMask);
            if (hit.collider != null)
            {
                Vector3 pos = (Vector3)hit.point + Vector3.up * 0.3f;
                Collider2D overlap = Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Walls"));
                if (overlap == null)
                    return pos;
            }
        }

        Debug.LogWarning("⚠️ Не удалось найти безопасную точку спавна, используем центр пола");
        return b.center + Vector3.up * 0.5f;
    }
}
