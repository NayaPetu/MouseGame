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

    [Header("Layers")]
    public LayerMask wallLayer;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    private GameObject playerInstance;

    public void GenerateRooms()
    {
        spawnedRooms.Clear();

        // 1?? Первая комната
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        // 2?? Генерация остальных комнат
        Vector3 lastRoomPos = startRoom.transform.position;
        BoxCollider2D lastBox = startRoom.GetComponentInChildren<BoxCollider2D>();

        for (int i = 1; i < numberOfRooms; i++)
        {
            GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            BoxCollider2D newBox = prefab.GetComponentInChildren<BoxCollider2D>();

            // Рассчитываем позицию так, чтобы стены касались друг друга
            float offsetX = (lastBox.bounds.max.x - newBox.bounds.min.x);
            Vector3 spawnPos = lastRoomPos + new Vector3(offsetX, 0, 0);

            GameObject room = Instantiate(prefab, spawnPos, Quaternion.identity);
            spawnedRooms.Add(room);

            // Создаем дверь между комнатами
            CreateDoorBetween(lastBox, newBox, room);

            // Спавн мебели и предметов
            SpawnRoomContents(room);

            lastRoomPos = room.transform.position;
            lastBox = room.GetComponentInChildren<BoxCollider2D>();
        }

        // 3?? Спавн игрока и врага после всех комнат
        SpawnPlayerAndEnemy(startRoom);
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

    private void SpawnPlayerAndEnemy(GameObject startRoom)
    {
        // Игрок
        Vector3 playerPos = GetRandomFloorPosition(startRoom);
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // Враг
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = GetRandomFloorPosition(startRoom);
            GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && furnitureSpawner != null)
            {
                ai.Init(furnitureSpawner.GenerateWalkableMapForRooms(spawnedRooms));
            }
        }
    }

    private Vector3 GetRandomFloorPosition(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box == null) return room.transform.position;

        Vector3 min = box.bounds.min;
        Vector3 max = box.bounds.max;

        Vector3 spawnPos = Vector3.zero;
        int attempts = 0;

        do
        {
            float x = Random.Range(min.x + 0.5f, max.x - 0.5f);
            float y = Random.Range(min.y + 0.5f, max.y - 0.5f);
            spawnPos = new Vector3(x, y, 0);
            attempts++;
        }
        while (Physics2D.OverlapCircle(spawnPos, 0.3f, wallLayer) != null && attempts < 20);

        return spawnPos;
    }

    private void CreateDoorBetween(BoxCollider2D lastBox, BoxCollider2D newBox, GameObject newRoom)
    {
        if (doorPrefab == null) return;

        // Ставим дверь на левую стену новой комнаты, по центру стены
        Vector3 doorPos = new Vector3(newBox.bounds.min.x, (newBox.bounds.min.y + newBox.bounds.max.y) / 2f, 0);
        Quaternion rotation = Quaternion.Euler(0, 0, 90); // для вертикальной стены

        GameObject door = Instantiate(doorPrefab, doorPos, rotation);
        door.layer = LayerMask.NameToLayer("Door");

        // Можно добавить ссылку на комнаты в скрипт двери
        Door doorScript = door.GetComponent<Door>();
        if (doorScript != null)
        {
            doorScript.roomA = lastBox.gameObject;
            doorScript.roomB = newRoom;
        }

        // Делаем дверь trigger, чтобы через неё можно было пройти
        Collider2D col = door.GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
}
