using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject startRoomPrefab; // фиксированная комната
    public GameObject[] roomPrefabs; // вариативные комнаты

    [Header("Room Connections")]
    public int numberOfRooms = 5;
    public float roomSpacing = 10f; // расстояние между центрами комнат

    private List<GameObject> spawnedRooms = new List<GameObject>();

    public FurnitureSpawner furnitureSpawner;

    public void GenerateRooms()
    {
        spawnedRooms.Clear();

        // Спавн первой фиксированной комнаты
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        Vector3 lastPos = startRoom.transform.position;

        for (int i = 1; i < numberOfRooms; i++)
        {
            GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            Vector3 offset = new Vector3(Random.Range(-roomSpacing, roomSpacing), Random.Range(-roomSpacing, roomSpacing), 0);
            Vector3 spawnPos = lastPos + offset;

            GameObject room = Instantiate(prefab, spawnPos, Quaternion.identity);
            spawnedRooms.Add(room);

            // Создаём арку/дверь между комнатами
            CreateDoorBetween(lastPos, spawnPos);

            lastPos = spawnPos;

            // Спавн мебели и предметов
            if (furnitureSpawner != null)
            {
                RectInt roomBounds = GetRoomBounds(room);
                furnitureSpawner.SpawnFurniture(roomBounds);
            }
        }
    }

    private void CreateDoorBetween(Vector3 a, Vector3 b)
    {
      
        Instantiate(doorPrefab, (a+b)/2, Quaternion.identity);
    }

    private RectInt GetRoomBounds(GameObject room)
    {
     
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box != null)
        {
            Vector3 pos = box.transform.position + (Vector3)box.offset;
            Vector2 size = box.size;
            return new RectInt(Mathf.FloorToInt(pos.x - size.x/2), Mathf.FloorToInt(pos.y - size.y/2), Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
        }
        return new RectInt(0,0,1,1);
    }
}
