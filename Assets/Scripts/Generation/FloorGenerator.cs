using UnityEngine;
using Unity.Cinemachine;

public class FloorGenerator : MonoBehaviour
{
    [Header("Main Floors (обычные этажи)")]
    public GameObject[] mainFloors;

    [Header("Basements (подвалы)")]
    public GameObject[] basements;

    [Header("Player / Enemy Prefabs")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    [Header("Layers")]
    public LayerMask floorLayerMask;

    [Header("Item Spawner")]
    public ItemSpawner itemSpawner;

    private GameObject playerInstance;
    private GameObject lastGeneratedFloor;


    // ===== СТАРТ ИГРЫ (только обычный этаж!) =====
    public void SpawnFloor()
    {
        if (mainFloors.Length == 0)
        {
            Debug.LogError("Нет обычных этажей!");
            return;
        }

        GameObject prefab = mainFloors[Random.Range(0, mainFloors.Length)];
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        lastGeneratedFloor = floor;
        SpawnEverythingOnFloor(floor);
    }


    // ===== Создание нового этажа по типу =====
    public GameObject SpawnFloorByType(FloorManager.FloorCategory type)
    {
        GameObject prefab = null;

        if (type == FloorManager.FloorCategory.Main)
            prefab = mainFloors.Length > 0 ? mainFloors[0] : null;
        else
            prefab = basements.Length > 0 ? basements[0] : null;

        if (prefab == null)
        {
            Debug.LogError("Нет префаба для " + type);
            return null;
        }

        if (lastGeneratedFloor != null)
            Destroy(lastGeneratedFloor);

        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        lastGeneratedFloor = floor;

        SpawnEverythingOnFloor(floor);
        return floor;
    }


    // ===== Спавн игрока, врага, предметов =====
    private void SpawnEverythingOnFloor(GameObject floor)
    {
        Transform playerSpawnPoint = floor.transform.Find("PlayerSpawnPoint");
        Vector3 playerPos = playerSpawnPoint != null ?
            playerSpawnPoint.position :
            CreateSafeSpawnPosition(floor, 0.4f);

        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        else
            playerInstance.transform.position = playerPos;

        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        Transform enemySpawnPoint = floor.transform.Find("EnemySpawnPoint");

        if (enemyPrefab != null)
        {
            Vector3 enemyPos = enemySpawnPoint != null ?
                enemySpawnPoint.position :
                CreateSafeSpawnPosition(floor, 0.4f);

            GameObject enemyInstance = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);

            Room startRoom = floor.GetComponentInChildren<Room>();
            if (startRoom != null)
            {
                EnemyAI ai = enemyInstance.GetComponent<EnemyAI>();
                if (ai != null)
                    ai.Init(startRoom, playerInstance.transform, enemyPos);
            }
        }

        if (itemSpawner != null)
            itemSpawner.InitializeFromRoom(floor);
    }


    // ===== Твой оригинальный метод безопасного спавна =====
    private Vector3 CreateSafeSpawnPosition(GameObject room, float radius)
    {
        Collider2D floorCollider = room.GetComponentInChildren<Collider2D>();
        if (!floorCollider)
            return room.transform.position;

        Bounds b = floorCollider.bounds;
        float margin = 0.5f;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = Random.Range(b.min.x + margin, b.max.x - margin);
            float y = Random.Range(b.min.y + margin, b.max.y - margin);
            Vector2 point = new(x, y);

            RaycastHit2D hit = Physics2D.Raycast(point + Vector2.up * 2f, Vector2.down, 5f, floorLayerMask);
            if (!hit.collider)
                continue;

            Vector3 pos = hit.point + Vector2.up * 0.3f;
            if (Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Walls")))
                continue;

            Collider2D doorHit = Physics2D.OverlapCircle(pos, radius);
            if (doorHit && doorHit.CompareTag("Door"))
                continue;

            return pos;
        }

        return b.center + Vector3.up * 0.5f;
    }
}
