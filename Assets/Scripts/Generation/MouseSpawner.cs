using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public BSPGenerator generator;

    private System.Random rng = new System.Random();

    void Start()
    {
        // ����, ���� BSPGenerator �������� ���������
        StartCoroutine(SpawnAfterGeneration());
    }

    private System.Collections.IEnumerator SpawnAfterGeneration()
    {
        // ���� ���� ����, ����� BSPGenerator ����� ������� �������
        yield return null;

        SpawnPlayerInRoom();
    }

    private void SpawnPlayerInRoom()
    {
        if (playerPrefab == null || generator == null) return;

        // �������� ������ ������, ������� ��� �������
        List<Leaf> rooms = generator.GetRoomLeaves();
        if (rooms.Count == 0)
        {
            Debug.LogError("��� ������ ��� ������ ������!");
            return;
        }

        // �������� ��������� �������
        Leaf roomLeaf = rooms[rng.Next(rooms.Count)];

        // ��������, ��� ������� ����������
        if (roomLeaf.room == RectInt.zero)
        {
            Debug.LogError("��������� ������� ������!");
            return;
        }

        // ���������� ��������� ������� ������ ������� � �������� �� ����
        Vector3 spawnPos = GetRandomPositionInsideRoom(roomLeaf);

        // ������� ������
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // ���������� ������ �������, � ������� ��������� �����
        generator.RevealRoom(roomLeaf.room);
    }

    private Vector3 GetRandomPositionInsideRoom(Leaf roomLeaf)
    {
        int padding = 1;

        int xMin = roomLeaf.room.xMin + padding;
        int xMax = roomLeaf.room.xMax - padding;
        int yMin = roomLeaf.room.yMin + padding;
        int yMax = roomLeaf.room.yMax - padding;

        // ������ �� ������ ��������� �������
        if (xMax <= xMin) xMax = xMin + 1;
        if (yMax <= yMin) yMax = yMin + 1;

        int x = rng.Next(xMin, xMax);
        int y = rng.Next(yMin, yMax);

        return new Vector3(x + 0.5f, y + 0.5f, 0);
    }
}
