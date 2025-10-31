using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [Header("Список предметов для спавна")]
    public List<GameObject> itemPrefabs; // сыр, мята
    public string spawnPointTag = "SpawnPoint";

    // Вызывается при генерации комнаты
    public void InitializeFromRoom(GameObject room)
    {
        Transform[] allPoints = room.GetComponentsInChildren<Transform>(true);
        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform t in allPoints)
            if (t.CompareTag(spawnPointTag))
                spawnPoints.Add(t);

        if (spawnPoints.Count == 0) return;

        foreach (var prefab in itemPrefabs)
        {
            if (prefab == null) continue;

            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject instance = Instantiate(prefab, randomPoint.position, Quaternion.identity);

            // Для мяты отключаем Attractor
            Catnip catnip = instance.GetComponent<Catnip>();
            if (catnip != null && catnip.attractor != null)
                catnip.attractor.SetActive(false);
        }
    }
}
