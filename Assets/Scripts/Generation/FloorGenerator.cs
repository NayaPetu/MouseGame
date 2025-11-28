using UnityEngine;
using Unity.Cinemachine;

public class FloorGenerator : MonoBehaviour
{
    [Header("Main Floors (обычные этажи)")]
    public GameObject[] mainFloors;

    [Header("Basements (подвалы)")]
    public GameObject[] basements;

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera; // исправлено с CinemachineVirtualCamera

    [Header("Layers")]
    public LayerMask floorLayerMask;

    [Header("Item Spawner")]
    public ItemSpawner itemSpawner;

    private GameObject playerInstance;
    private GameObject lastGeneratedFloor;

    // ===== Старый вызов SpawnFloor() для совместимости =====
    public void SpawnFloor() => SpawnFloorByType(FloorManager.FloorCategory.Main);

    // ===== Создание нового этажа по типу =====
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

        if (lastGeneratedFloor != null)
            Destroy(lastGeneratedFloor);

        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        lastGeneratedFloor = floor;

        SpawnPlayer(floor);

        if (itemSpawner != null)
            itemSpawner.InitializeFromRoom(floor);

        return floor;
    }

    // ===== Спавн игрока один раз =====
    private void SpawnPlayer(GameObject floor)
    {
        Transform spawnPoint = floor.transform.Find("PlayerSpawnPoint");
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : CreateSafeSpawnPosition(floor, 0.4f);

        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            playerInstance.transform.position = spawnPos;
        }

        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;
    }

    // ===== Создание безопасной позиции (если SpawnPoint нет) =====
    private Vector3 CreateSafeSpawnPosition(GameObject floor, float radius)
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

            RaycastHit2D hit = Physics2D.Raycast(point + Vector2.up * 2f, Vector2.down, 5f, floorLayerMask);
            if (!hit.collider) continue;

            Vector3 pos = new Vector3(hit.point.x, hit.point.y + 0.3f, 0f);

            if (Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Walls"))) continue;

            Collider2D doorHit = Physics2D.OverlapCircle(pos, radius);
            if (doorHit && doorHit.CompareTag("Door")) continue;

            return pos;
        }

        return b.center + Vector3.up * 0.5f;
    }
}
