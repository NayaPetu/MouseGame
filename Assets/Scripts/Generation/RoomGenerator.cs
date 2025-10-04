using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;

    [Header("Room Settings")]
    public int numberOfRooms = 5;

    [Header("Door Prefab")]
    public GameObject doorPrefab;

    [Header("Spawners")]
    public FurnitureSpawner furnitureSpawner;
    public ItemSpawner itemSpawnerPrefab;

    [Header("Player/Enemy")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private GameObject playerInstance;

    public void GenerateRooms()
    {
        spawnedRooms.Clear();

        // Первая комната
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        // Спавн игрока на полу
        playerInstance = Instantiate(playerPrefab, GetRandomSpawnPosition(startRoom), Quaternion.identity);
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // Спавн врага
        if (enemyPrefab != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, GetRandomSpawnPosition(startRoom), Quaternion.identity);
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && furnitureSpawner != null)
            {
                ai.Init(furnitureSpawner.GenerateWalkableMapForRooms(spawnedRooms));
            }
        }

        // Спавн мебели и предметов
        SpawnRoomContents(startRoom);

        Vector3 lastPos = startRoom.transform.position;

        // Остальные комнаты
        for (int i = 1; i < numberOfRooms; i++)
        {
            GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];

            // Спавн комнаты справа от предыдущей (без перекрытия)
            Vector3 spawnPos = lastPos + new Vector3(GetRoomWidth(lastPos, prefab), 0, 0);
            GameObject room = Instantiate(prefab, spawnPos, Quaternion.identity);
            spawnedRooms.Add(room);

            // Дверь между комнатами
            CreateDoorBetween(lastPos, spawnPos, room);

            // Спавн мебели и предметов
            SpawnRoomContents(room);

            lastPos = spawnPos;
        }
    }

    private void SpawnRoomContents(GameObject room)
    {
        if (furnitureSpawner != null)
        {
            RectInt bounds = furnitureSpawner.GetRoomBounds(room);
            furnitureSpawner.SpawnFurniture(bounds);

            if (itemSpawnerPrefab != null)
            {
                ItemSpawner spawner = Instantiate(itemSpawnerPrefab);
                spawner.roomMin = new Vector2(bounds.xMin, bounds.yMin);
                spawner.roomMax = new Vector2(bounds.xMax, bounds.yMax);
                spawner.SpawnItems();
            }
        }
    }

    private Vector3 GetRandomSpawnPosition(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box == null) return room.transform.position;

        Vector3 localMin = (Vector3)box.offset - (Vector3)(box.size / 2f);
        Vector3 localMax = (Vector3)box.offset + (Vector3)(box.size / 2f);

        float padding = 0.5f;
        float x = Random.Range(localMin.x + padding, localMax.x - padding);
        float y = Random.Range(localMin.y + padding, localMax.y - padding);

        Vector3 spawnPos = room.transform.position + new Vector3(x, y, 0);

        // Проверка на пересечение со стеной
        Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.2f, LayerMask.GetMask("Wall"));
        int attempts = 0;
        while (hit != null && attempts < 10)
        {
            x = Random.Range(localMin.x + padding, localMax.x - padding);
            y = Random.Range(localMin.y + padding, localMax.y - padding);
            spawnPos = room.transform.position + new Vector3(x, y, 0);
            hit = Physics2D.OverlapCircle(spawnPos, 0.2f, LayerMask.GetMask("Wall"));
            attempts++;
        }

        return spawnPos;
    }

    private float GetRoomWidth(Vector3 lastPos, GameObject prefab)
    {
        BoxCollider2D box = prefab.GetComponentInChildren<BoxCollider2D>();
        if (box != null)
            return box.size.x;
        return 10f; // запасная ширина
    }

    private void CreateDoorBetween(Vector3 previousRoomPos, Vector3 newRoomPos, GameObject newRoom)
    {
        if (doorPrefab == null) return;

        BoxCollider2D box = newRoom.GetComponentInChildren<BoxCollider2D>();
        if (box == null) return;

        // Дверь на левой стене новой комнаты
        float yDoor = Random.Range(box.bounds.min.y + 1, box.bounds.max.y - 1);
        Vector3 doorPos = new Vector3(box.bounds.min.x, yDoor, 0);
        Quaternion rotation = Quaternion.Euler(0, 0, 90);

        GameObject door = Instantiate(doorPrefab, doorPos, rotation);

        // Назначение комнат для двери
        Door doorScript = door.GetComponent<Door>();
        if (doorScript != null)
        {
            doorScript.roomA = spawnedRooms[spawnedRooms.Count - 2]; // предыдущая комната
            doorScript.roomB = newRoom;                               // новая комната
        }
    }
}
