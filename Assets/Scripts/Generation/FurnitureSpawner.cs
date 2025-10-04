using UnityEngine;

[System.Serializable]
public class FurnitureSpawner : MonoBehaviour
{
    [Header("Furniture Settings")]
    public GameObject[] furniturePrefabs; // Декоративная мебель
    public int maxFurniturePerRoom = 3;   // Максимум мебели в комнате

    /// <summary>
    /// Спавнит мебель в указанной комнате
    /// </summary>
    /// <param name="roomBounds">Границы комнаты в координатах Tilemap или мира</param>
    public void SpawnFurniture(RectInt roomBounds)
    {
        if (furniturePrefabs.Length == 0) return;

        int furnitureCount = Random.Range(1, maxFurniturePerRoom + 1);

        for (int i = 0; i < furnitureCount; i++)
        {
            GameObject prefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];

            Vector2 spawnPos = GetRandomPositionInRoom(roomBounds, prefab);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Выбирает случайную позицию внутри комнаты с учётом размера мебели
    /// </summary>
    private Vector2 GetRandomPositionInRoom(RectInt roomBounds, GameObject prefab)
    {
        Vector2 size = Vector2.one;
        Furniture f = prefab.GetComponent<Furniture>();
        if (f != null)
        {
            size = new Vector2(f.sizeInTiles.x, f.sizeInTiles.y);
        }

        float x = Random.Range(roomBounds.xMin + size.x / 2f, roomBounds.xMax - size.x / 2f);
        float y = Random.Range(roomBounds.yMin + size.y / 2f, roomBounds.yMax - size.y / 2f);

        return new Vector2(x, y);
    }
}

/// <summary>
/// Скрипт для самой мебели, чтобы задать размер в клетках Tilemap
/// </summary>
public class Furniture : MonoBehaviour
{
    public Vector2Int sizeInTiles = Vector2Int.one; // ширина и высота в тайлах
}
