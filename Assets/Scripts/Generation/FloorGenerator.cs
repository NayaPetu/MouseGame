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

    private GameObject playerInstance;

    // --- Генерация этажа ---
    public void SpawnFloor()
    {
        if (floorPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ Нет префабов комнат для спавна!");
            return;
        }

        // Выбираем случайную комнату
        GameObject prefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // === Находим точки спавна внутри префаба ===
        Transform playerSpawnPoint = floor.transform.Find("PlayerSpawnPoint");
        Transform enemySpawnPoint = floor.transform.Find("EnemySpawnPoint");

        // === СПАВН ИГРОКА ===
        Vector3 playerPos = playerSpawnPoint != null ? playerSpawnPoint.position : CreateSafeSpawnPosition(floor, 0.4f);
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);

        // Камера следует за игроком
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // === СПАВН ВРАГА ===
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = enemySpawnPoint != null ? enemySpawnPoint.position : CreateSafeSpawnPosition(floor, 0.4f);
            GameObject enemyInstance = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);

            // Инициализируем врага, чтобы он знал игрока
            Room startRoom = floor.GetComponentInChildren<Room>();
            if (startRoom != null)
            {
                EnemyAI enemyAI = enemyInstance.GetComponent<EnemyAI>();
                if (enemyAI != null)
                    enemyAI.Init(startRoom, playerInstance.transform, enemyPos);
            }
        }
    }

    // --- Создание безопасной позиции ---
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

        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = Random.Range(b.min.x + safeMargin, b.max.x - safeMargin);
            float y = Random.Range(b.min.y + safeMargin, b.max.y - safeMargin);
            Vector2 point = new Vector2(x, y);

            // Проверяем, что точка на полу
            RaycastHit2D hit = Physics2D.Raycast(point + Vector2.up * 2f, Vector2.down, 5f, floorLayerMask);
            if (hit.collider == null)
                continue;

            Vector3 pos = hit.point + Vector2.up * 0.3f;

            // Проверяем коллизии со стенами и дверями
            Collider2D wallHit = Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Walls"));
            if (wallHit != null)
                continue;

            Collider2D doorHit = Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Default"));
            if (doorHit != null && doorHit.CompareTag("Door"))
                continue;

            return pos;
        }

        Debug.LogWarning("⚠️ Не удалось найти безопасную точку спавна, используем центр пола");
        return b.center + Vector3.up * 0.5f;
    }
}
