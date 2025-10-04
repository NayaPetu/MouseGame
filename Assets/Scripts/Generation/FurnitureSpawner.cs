using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FurnitureSpawner : MonoBehaviour
{
    [Header("Furniture")]
    public GameObject[] furniturePrefabs;
    public int maxFurniturePerRoom = 3;

    [Header("Items")]
    public GameObject[] itemPrefabs;
    public int maxItemsPerRoom = 3;

    public void SpawnFurniture(RectInt roomBounds)
    {
        if (furniturePrefabs.Length == 0) return;

        int count = Random.Range(1, maxFurniturePerRoom + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
            Vector2 pos = GetRandomPositionInRoom(roomBounds, prefab);
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }

    public void SpawnItems(RectInt roomBounds)
    {
        if (itemPrefabs.Length == 0) return;

        int count = Random.Range(1, maxItemsPerRoom + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            Vector2 pos = new Vector2(
                Random.Range(roomBounds.xMin + 0.5f, roomBounds.xMax - 0.5f),
                Random.Range(roomBounds.yMin + 0.5f, roomBounds.yMax - 0.5f)
            );
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }

    public Vector2 GetRandomPositionInRoom(RectInt roomBounds, GameObject prefab)
    {
        Vector2 size = Vector2.one;
        BoxCollider2D col = prefab.GetComponent<BoxCollider2D>();
        if (col != null) size = col.size;

        float x = Random.Range(roomBounds.xMin + size.x / 2f, roomBounds.xMax - size.x / 2f);
        float y = Random.Range(roomBounds.yMin + size.y / 2f, roomBounds.yMax - size.y / 2f);

        return new Vector2(x, y);
    }

    public RectInt GetRoomBounds(GameObject room)
    {
        BoxCollider2D box = room.GetComponentInChildren<BoxCollider2D>();
        if (box == null) return new RectInt(0, 0, 1, 1);

        Vector3 pos = box.transform.position + (Vector3)box.offset;
        Vector2 size = box.size;
        return new RectInt(Mathf.FloorToInt(pos.x - size.x / 2), Mathf.FloorToInt(pos.y - size.y / 2),
                           Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
    }

    public bool[,] GenerateWalkableMapForRooms(List<GameObject> rooms)
    {
        if (rooms.Count == 0) return new bool[1,1];

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var room in rooms)
        {
            RectInt r = GetRoomBounds(room);
            minX = Mathf.Min(minX, r.xMin);
            minY = Mathf.Min(minY, r.yMin);
            maxX = Mathf.Max(maxX, r.xMax);
            maxY = Mathf.Max(maxY, r.yMax);
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        bool[,] map = new bool[width, height];

        foreach (var room in rooms)
        {
            RectInt r = GetRoomBounds(room);
            for (int x = r.xMin; x < r.xMax; x++)
                for (int y = r.yMin; y < r.yMax; y++)
                    map[x - minX, y - minY] = true;
        }

        return map;
    }
}
