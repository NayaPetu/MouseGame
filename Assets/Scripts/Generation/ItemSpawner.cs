using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject itemPrefab; // Префаб предмета (например, сыр 🧀)
    public string spawnPointTag = "SpawnPoint"; // Тег для поиска точек

    // Метод инициализации из комнаты
    public void InitializeFromRoom(GameObject room)
    {
        // Находим все объекты с тегом SpawnPoint в комнате
        Transform[] allPoints = room.GetComponentsInChildren<Transform>(true);
        var spawnPoints = new System.Collections.Generic.List<Transform>();

        foreach (Transform t in allPoints)
        {
            if (t.CompareTag(spawnPointTag))
                spawnPoints.Add(t);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"⚠️ В комнате {room.name} нет точек спавна (SpawnPoint)!");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("⚠️ Не назначен prefab предмета в ItemSpawner!");
            return;
        }

        // Выбираем случайную точку
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Спавним предмет в этой точке
        SpawnItemAt(randomPoint.position);
    }

    // Метод спавна предмета
    private void SpawnItemAt(Vector3 position)
    {
        Instantiate(itemPrefab, position, Quaternion.identity);
    }
}
