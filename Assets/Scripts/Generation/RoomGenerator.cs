using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    public GameObject startRoomPrefab;
    public GameObject[] roomPrefabs;

    [Header("Doors")]
    public GameObject doorPrefab;

    [Header("Player/Enemy")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    private List<GameObject> spawnedRooms = new List<GameObject>();
    public List<GameObject> SpawnedRooms => spawnedRooms;

    // ������������ ������
    private Vector2Int currentPos = Vector2Int.zero;

    public void GenerateRooms()
    {
        spawnedRooms.Clear();
        currentPos = Vector2Int.zero;

        // --- ��������� ������� ---
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        // ����� ������ � ������ ��������� �������
        Vector3 spawnPlayerPos = GetRoomCenter(startRoom);
        GameObject player = Instantiate(playerPrefab, spawnPlayerPos, Quaternion.identity);
        if (virtualCamera != null) virtualCamera.Follow = player.transform;

        // ����� ����� ����� � �������
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = spawnPlayerPos + Vector3.right * 1.5f;
            GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }

        // --- ��������� ��������� ������ ---
        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            GameObject prefab = roomPrefabs[i];

            // �������� ������� ���������� �������
            GameObject lastRoom = spawnedRooms[spawnedRooms.Count - 1];
            Vector2 lastSize = GetRoomSize(lastRoom);

            // �������� ����� ������� ������, ����� ����� �������������
            Vector3 newPos = lastRoom.transform.position + new Vector3(lastSize.x, 0, 0);
            GameObject room = Instantiate(prefab, newPos, Quaternion.identity);
            spawnedRooms.Add(room);

            // ������ ����� ����� ���������
            if (doorPrefab != null)
            {
                Vector3 doorPos = lastRoom.transform.position + new Vector3(lastSize.x / 2f, 0, 0);
                GameObject door = Instantiate(doorPrefab, doorPos, Quaternion.identity);
                door.transform.rotation = Quaternion.Euler(0, 0, 90f); // ����� �����������
            }
        }
    }

    private Vector3 GetRoomCenter(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box != null)
        {
            Vector3 pos = box.transform.position + (Vector3)box.offset;
            return pos;
        }
        return room.transform.position;
    }

    private Vector2 GetRoomSize(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box != null)
        {
            Vector2 size = box.size;
            return size;
        }
        return Vector2.one;
    }
}
