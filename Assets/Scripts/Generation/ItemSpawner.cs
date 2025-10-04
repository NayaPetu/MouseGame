using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавнера")]
    public GameObject[] itemPrefabs; // все предметы
    public int itemsToSpawn = 5;     // сколько предметов заспавнить
    public Vector2 roomMin;          // нижний левый угол комнаты
    public Vector2 roomMax;          // верхний правый угол комнаты

    [Header("Опционально")]
    public bool randomRotation = false;

    public void SpawnItems()
    {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (itemPrefabs.Length == 0) return;

            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            Vector2 spawnPos = new Vector2(
                Random.Range(roomMin.x, roomMax.x),
                Random.Range(roomMin.y, roomMax.y)
            );

            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            if (randomRotation)
            {
                float zRot = Random.Range(0f, 360f);
                item.transform.Rotate(0f, 0f, zRot);
            }
        }
    }
}
