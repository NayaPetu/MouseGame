using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FurnitureSpawner : MonoBehaviour
{
    [Header("Furniture")]
    public GameObject[] furniturePrefabs;
    public int maxFurniturePerRoom = 3;

    public void SpawnFurniture(RectInt roomBounds)
    {
        if (furniturePrefabs.Length == 0) return;

        int count = Random.Range(1, maxFurniturePerRoom + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
            Vector2 pos = GetRandomPositionInRoom(roomBounds);
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }

    public Vector2 GetRandomPositionInRoom(RectInt roomBounds)
    {
        float x = Random.Range(roomBounds.xMin + 0.5f, roomBounds.xMax - 0.5f);
        float y = Random.Range(roomBounds.yMin + 0.5f, roomBounds.yMax - 0.5f);
        return new Vector2(x, y);
    }

    public RectInt GetRoomBounds(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box == null) return new RectInt(0,0,1,1);

        Vector3 pos = box.transform.position + (Vector3)box.offset;
        Vector2 size = box.size;
        return new RectInt(
            Mathf.FloorToInt(pos.x - size.x / 2),
            Mathf.FloorToInt(pos.y - size.y / 2),
            Mathf.CeilToInt(size.x),
            Mathf.CeilToInt(size.y)
        );
    }

    public bool[,] GenerateWalkableMapForRooms(List<GameObject> rooms)
    {
        if (rooms.Count == 0) return null;

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var room in rooms)
        {
            BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
            if (box == null) continue;

            Vector3 pos = box.transform.position + (Vector3)box.offset;
            Vector2 size = box.size;

            minX = Mathf.Min(minX, pos.x - size.x/2);
            minY = Mathf.Min(minY, pos.y - size.y/2);
            maxX = Mathf.Max(maxX, pos.x + size.x/2);
            maxY = Mathf.Max(maxY, pos.y + size.y/2);
        }

        int width = Mathf.CeilToInt(maxX - minX);
        int height = Mathf.CeilToInt(maxY - minY);

        bool[,] map = new bool[width, height];

        foreach (var room in rooms)
        {
            BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
            if (box == null) continue;

            Vector3 pos = box.transform.position + (Vector3)box.offset;
            Vector2 size = box.size;

            int startX = Mathf.FloorToInt(pos.x - size.x/2 - minX);
            int startY = Mathf.FloorToInt(pos.y - size.y/2 - minY);

            for (int x = 0; x < Mathf.CeilToInt(size.x); x++)
                for (int y = 0; y < Mathf.CeilToInt(size.y); y++)
                {
                    int mx = startX + x;
                    int my = startY + y;
                    if (mx >= 0 && mx < width && my >= 0 && my < height)
                        map[mx, my] = true;
                }
        }

        return map;
    }
}
