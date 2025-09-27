using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public BSPGenerator generator;

    private System.Random rng = new System.Random();

    void Start()
    {
        // ∆дем, пока BSPGenerator закончит генерацию
        StartCoroutine(SpawnAfterGeneration());
    }

    private System.Collections.IEnumerator SpawnAfterGeneration()
    {
        // ∆дем один кадр, чтобы BSPGenerator успел создать комнаты
        yield return null;

        SpawnPlayerInRoom();
    }

    private void SpawnPlayerInRoom()
    {
        if (playerPrefab == null || generator == null) return;

        // ѕолучаем список комнат, которые уже созданы
        List<Leaf> rooms = generator.GetRoomLeaves();
        if (rooms.Count == 0)
        {
            Debug.LogError("Ќет комнат дл€ спавна игрока!");
            return;
        }

        // ¬ыбираем случайную комнату
        Leaf roomLeaf = rooms[rng.Next(rooms.Count)];

        // ѕроверка, что комната существует
        if (roomLeaf.room == RectInt.zero)
        {
            Debug.LogError("¬ыбранна€ комната пуста€!");
            return;
        }

        // √енерируем случайную позицию внутри комнаты с отступом от стен
        Vector3 spawnPos = GetRandomPositionInsideRoom(roomLeaf);

        // —оздаем игрока
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // ѕоказываем только комнату, в которой находитс€ игрок
        generator.RevealRoom(roomLeaf.room);
    }

    private Vector3 GetRandomPositionInsideRoom(Leaf roomLeaf)
    {
        int padding = 1;

        int xMin = roomLeaf.room.xMin + padding;
        int xMax = roomLeaf.room.xMax - padding;
        int yMin = roomLeaf.room.yMin + padding;
        int yMax = roomLeaf.room.yMax - padding;

        // «ащита на случай маленькой комнаты
        if (xMax <= xMin) xMax = xMin + 1;
        if (yMax <= yMin) yMax = yMin + 1;

        int x = rng.Next(xMin, xMax);
        int y = rng.Next(yMin, yMax);

        return new Vector3(x + 0.5f, y + 0.5f, 0);
    }
}
