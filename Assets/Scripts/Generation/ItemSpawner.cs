using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавнера")]
    public GameObject[] itemPrefabs; // все предметы (BaseItem и наследники)
    public int itemsToSpawn = 5;      // сколько предметов заспавнить
    public Vector2 roomMin;           // нижний левый угол комнаты
    public Vector2 roomMax;           // верхний правый угол комнаты

    [Header("Опционально")]
    public bool randomRotation = false; // повернуть предмет случайно

    void Start()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (itemPrefabs.Length == 0) return;

            // Выбираем случайный предмет
            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            // Выбираем случайную позицию в комнате
            Vector2 spawnPos = new Vector2(
                Random.Range(roomMin.x, roomMax.x),
                Random.Range(roomMin.y, roomMax.y)
            );

            // Создаём объект
            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            // Случайный поворот (если включено)
            if (randomRotation)
            {
                float zRot = Random.Range(0f, 360f);
                item.transform.Rotate(0f, 0f, zRot);
            }

            // Убедимся, что предметы не спавнятся внутри стены (если нужно)
            Collider2D col = item.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }
    }
}
