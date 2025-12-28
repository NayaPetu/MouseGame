using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemSpawnData
{
    [Header("Префаб предмета")]
    public GameObject prefab;

    [Header("Количество предметов")]
    public int minCount = 1;
    public int maxCount = 1;
}

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки предметов")]
    public List<ItemSpawnData> items;

    [Header("Тэг spawn-поинтов")]
    public string spawnPointTag = "SpawnPoint";

    [Header("Проверка коллизий")]
    public LayerMask collisionLayers;
    public float checkRadius = 0.5f;
    public int maxAttempts = 20;

    private bool initialized;

    public List<GameObject> spawnedItems = new List<GameObject>();

    public void InitializeFromRoom(GameObject room)
    {
        // Очищаем предыдущие предметы перед спавном нового этажа
        Clear();

        Transform[] allPoints = room.GetComponentsInChildren<Transform>(true);
        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform t in allPoints)
            if (t.CompareTag(spawnPointTag))
                spawnPoints.Add(t);

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"[{room.name}] Spawn points not found");
            return;
        }

        initialized = true;

        foreach (var item in items)
        {
            if (item.prefab == null) continue;

            int count = Random.Range(item.minCount, item.maxCount + 1);
            Debug.Log($"[ItemSpawner] Пытаемся заспавнить {count} предметов типа {item.prefab.name} (min: {item.minCount}, max: {item.maxCount})");

            int successfullySpawned = 0;
            // Создаем список доступных спавн-поинтов, которые мы будем перемешивать
            List<Transform> availablePoints = new List<Transform>(spawnPoints);
            
            for (int i = 0; i < count; i++)
            {
                bool spawned = false;
                int attempts = 0;

                // Перемешиваем список доступных поинтов для более равномерного распределения
                if (availablePoints.Count > 0)
                {
                    for (int j = 0; j < availablePoints.Count; j++)
                    {
                        Transform temp = availablePoints[j];
                        int randomIndex = Random.Range(0, availablePoints.Count);
                        availablePoints[j] = availablePoints[randomIndex];
                        availablePoints[randomIndex] = temp;
                    }
                }

                while (!spawned && attempts < maxAttempts)
                {
                    attempts++;

                    // Пробуем разные поинты по очереди, а не случайные
                    Transform point = availablePoints.Count > 0 
                        ? availablePoints[attempts % availablePoints.Count]
                        : spawnPoints[Random.Range(0, spawnPoints.Count)];
                    
                    // Добавляем случайное смещение, чтобы предметы не накладывались идеально
                    Vector2 pos = (Vector2)point.position + Random.insideUnitCircle * 0.3f;

                    if (IsPositionFree(pos))
                    {
                        GameObject instance = Instantiate(item.prefab, pos, Quaternion.identity, room.transform);
                        spawnedItems.Add(instance);

                        Catnip catnip = instance.GetComponent<Catnip>();
                        if (catnip != null && catnip.attractor != null)
                            catnip.attractor.SetActive(false);

                        spawned = true;
                        successfullySpawned++;
                        Debug.Log($"[ItemSpawner] Заспавнен {item.prefab.name} #{successfullySpawned} в позиции {pos}");
                    }
                }

                if (!spawned)
                    Debug.LogWarning($"[ItemSpawner] Не удалось заспавнить {item.prefab.name} #{i + 1} после {maxAttempts} попыток. Успешно заспавнено: {successfullySpawned}/{count}. Доступно спавн-поинтов: {spawnPoints.Count}");
            }
            
            Debug.Log($"[ItemSpawner] Итого заспавнено {successfullySpawned} из {count} предметов типа {item.prefab.name}");
        }
    }

    // -------------------- Проверка свободной позиции --------------------
    private bool IsPositionFree(Vector2 pos)
    {
        // Проверка коллайдеров на сцене (стены, препятствия)
        if (Physics2D.OverlapCircle(pos, checkRadius, collisionLayers) != null)
            return false;

        // Проверка по уже заспавненным предметам - используем меньший радиус, чтобы предметы могли быть ближе
        foreach (var spawned in spawnedItems)
        {
            if (spawned == null) continue;
            // Используем checkRadius вместо checkRadius * 2f, чтобы предметы могли быть ближе друг к другу
            if (Vector2.Distance(spawned.transform.position, pos) < checkRadius * 1.5f)
                return false;
        }

        return true;
    }

    public void Clear()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }
        spawnedItems.Clear();
        initialized = false;
    }
}
