using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Список предметов для спавна")]
    public List<GameObject> itemPrefabs;

    [Header("Тэг spawn-поинтов для конкретного типа этажа")]
    public string spawnPointTag = "SpawnPoint_Main";

    [Header("Проверка коллизий")]
    public LayerMask collisionLayers;
    public float checkRadius = 0.5f;
    public int maxAttempts = 10;

    public void InitializeFromRoom(GameObject room)
    {
        Transform[] allPoints = room.GetComponentsInChildren<Transform>(true);
        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform t in allPoints)
        {
            if (t.CompareTag(spawnPointTag))
                spawnPoints.Add(t);
        }

        if (spawnPoints.Count == 0) return;

        foreach (var prefab in itemPrefabs)
        {
            if (prefab == null) continue;

            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxAttempts)
            {
                attempts++;
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                Vector2 spawnPos = randomPoint.position;

                Collider2D hit = Physics2D.OverlapCircle(spawnPos, checkRadius, collisionLayers);
                if (hit == null)
                {
                    GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

                    Catnip catnip = instance.GetComponent<Catnip>();
                    if (catnip != null && catnip.attractor != null)
                        catnip.attractor.SetActive(false);

                    spawned = true;
                }
            }

            if (!spawned)
                Debug.LogWarning($"Не удалось спавнить {prefab.name} после {maxAttempts} попыток.");
        }
    }
}
