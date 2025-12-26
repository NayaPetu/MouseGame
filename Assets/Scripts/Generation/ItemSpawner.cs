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
    public string spawnPointTag = "SpawnPoint_Main";

    [Header("Проверка коллизий")]
    public LayerMask collisionLayers;
    public float checkRadius = 0.5f;
    public int maxAttempts = 20;

    private bool initialized;

    public List<GameObject> spawnedItems = new List<GameObject>();

    public void InitializeFromRoom(GameObject room)
    {
        if (initialized) return;
        initialized = true;

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

        foreach (var item in items)
        {
            if (item.prefab == null) continue;

            int count = Random.Range(item.minCount, item.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                bool spawned = false;
                int attempts = 0;

                while (!spawned && attempts < maxAttempts)
                {
                    attempts++;

                    Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    // Добавляем небольшое случайное смещение, чтобы предметы не накладывались идеально
                    Vector2 pos = (Vector2)point.position + Random.insideUnitCircle * 0.3f;

                    if (IsPositionFree(pos))
                    {
                        GameObject instance = Instantiate(item.prefab, pos, Quaternion.identity, room.transform);
                        spawnedItems.Add(instance);

                        Catnip catnip = instance.GetComponent<Catnip>();
                        if (catnip != null && catnip.attractor != null)
                            catnip.attractor.SetActive(false);

                        spawned = true;
                    }
                }

                if (!spawned)
                    Debug.LogWarning($"[{room.name}] Не удалось заспавнить {item.prefab.name} после {maxAttempts} попыток.");
            }
        }
    }

    // -------------------- Проверка свободной позиции --------------------
    private bool IsPositionFree(Vector2 pos)
    {
        // Проверка коллайдеров на сцене
        if (Physics2D.OverlapCircle(pos, checkRadius, collisionLayers) != null)
            return false;

        // Проверка по уже заспавненным предметам
        foreach (var spawned in spawnedItems)
        {
            if (spawned == null) continue;
            if (Vector2.Distance(spawned.transform.position, pos) < checkRadius * 2f)
                return false;
        }

        return true;
    }
}
