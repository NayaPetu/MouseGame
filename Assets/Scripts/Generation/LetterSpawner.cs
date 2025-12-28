using UnityEngine;
using System.Collections.Generic;

public class LetterSpawner : MonoBehaviour
{
    [Header("Префаб письма")]
    [SerializeField] private GameObject letterPrefab;

    [Header("Тэг spawn-поинтов для писем")]
    [SerializeField] private string spawnPointTag = "SpawnPoint_Letter";

    [Header("Количество писем")]
    [SerializeField] private int minCount = 0;
    [SerializeField] private int maxCount = 1;

    [Header("Проверка коллизий")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private int maxAttempts = 20;

    private bool initialized;
    private List<GameObject> spawnedLetters = new List<GameObject>();
    // Словарь для хранения письем по этажам, чтобы знать, были ли они уже заспавнены
    private Dictionary<GameObject, List<GameObject>> lettersByRoom = new Dictionary<GameObject, List<GameObject>>();

    public void InitializeFromRoom(GameObject room)
    {
        // Проверяем, были ли письма уже заспавнены на этом этаже
        if (lettersByRoom.ContainsKey(room))
        {
            // Письма уже были заспавнены на этом этаже, не спавним повторно
            Debug.Log($"[LetterSpawner] Письма уже были заспавнены на этаже {room.name}, пропускаем спавн");
            return;
        }

        // Очищаем только письма этого этажа (на случай если есть старые)
        ClearRoom(room);
        
        // Создаем новый список для этого этажа
        lettersByRoom[room] = new List<GameObject>();
        
        if (letterPrefab == null)
        {
            Debug.LogWarning($"[LetterSpawner] Letter prefab is not assigned!");
            return;
        }

        initialized = true;

        // Получаем текущий этаж
        FloorManager.FloorCategory currentFloor = FloorManager.FloorCategory.Main;
        if (GameManager.Instance != null)
        {
            currentFloor = GameManager.Instance.PlayerCurrentFloor;
        }

        // Определяем этаж по имени комнаты
        string roomName = room.name.ToLower();
        if (roomName.Contains("basement") || roomName.Contains("подвал"))
        {
            currentFloor = FloorManager.FloorCategory.Basement;
        }

        // Получаем список доступных писем для этого этажа
        if (LetterManager.Instance == null)
        {
            Debug.LogWarning($"[LetterSpawner] LetterManager.Instance is null! Cannot spawn letters.");
            return;
        }

        List<LetterData> availableLetters = LetterManager.Instance.GetLettersForFloor(currentFloor);
        if (availableLetters.Count == 0)
        {
            Debug.LogWarning($"[LetterSpawner] No letters available for floor {currentFloor}!");
            return;
        }

        // Находим спавн-поинты для писем
        Transform[] allPoints = room.GetComponentsInChildren<Transform>(true);
        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform t in allPoints)
        {
            if (t.CompareTag(spawnPointTag))
            {
                spawnPoints.Add(t);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning($"[LetterSpawner] No spawn points with tag '{spawnPointTag}' found in {room.name}!");
            return;
        }

        // Определяем количество писем для спавна
        int count = Random.Range(minCount, maxCount + 1);
        Debug.Log($"[LetterSpawner] Spawning {count} letter(s) on floor {currentFloor} from {availableLetters.Count} available letters");

        // Спавним письма
        for (int i = 0; i < count && spawnPoints.Count > 0; i++)
        {
            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxAttempts && spawnPoints.Count > 0)
            {
                attempts++;

                // Выбираем случайную точку спавна
                int randomIndex = Random.Range(0, spawnPoints.Count);
                Transform point = spawnPoints[randomIndex];
                
                // Добавляем небольшое случайное смещение
                Vector2 pos = (Vector2)point.position + Random.insideUnitCircle * 0.3f;

                if (IsPositionFree(pos))
                {
                    // Создаем экземпляр письма
                    GameObject instance = Instantiate(letterPrefab, pos, Quaternion.identity, room.transform);
                    spawnedLetters.Add(instance);
                    lettersByRoom[room].Add(instance);

                    // Назначаем случайное письмо для этого экземпляра
                    FriendNote friendNote = instance.GetComponent<FriendNote>();
                    if (friendNote != null && availableLetters.Count > 0)
                    {
                        // Выбираем случайное письмо из доступных
                        LetterData randomLetter = availableLetters[Random.Range(0, availableLetters.Count)];
                        // Устанавливаем ID письма
                        friendNote.SetLetterId(randomLetter.id);
                    }

                    Debug.Log($"[LetterSpawner] Spawned letter at position {pos}");
                    spawned = true;

                    // Удаляем использованную точку, чтобы не спавнить два письма в одном месте
                    spawnPoints.RemoveAt(randomIndex);
                }
            }

            if (!spawned)
            {
                Debug.LogWarning($"[LetterSpawner] Failed to spawn letter {i + 1} after {maxAttempts} attempts.");
            }
        }
    }

    private bool IsPositionFree(Vector2 pos)
    {
        // Проверка коллайдеров на сцене
        if (Physics2D.OverlapCircle(pos, checkRadius, collisionLayers) != null)
            return false;

        // Проверка по уже заспавненным письмам
        foreach (var spawned in spawnedLetters)
        {
            if (spawned == null) continue;
            if (Vector2.Distance(spawned.transform.position, pos) < checkRadius * 2f)
                return false;
        }

        return true;
    }

    // Очистка всех писем (старый метод для совместимости)
    public void Clear()
    {
        foreach (var letter in spawnedLetters)
        {
            if (letter != null)
                Destroy(letter);
        }
        spawnedLetters.Clear();
        lettersByRoom.Clear();
        initialized = false;
    }

    // Очистка писем только для конкретного этажа
    private void ClearRoom(GameObject room)
    {
        if (lettersByRoom.ContainsKey(room))
        {
            foreach (var letter in lettersByRoom[room])
            {
                if (letter != null)
                {
                    spawnedLetters.Remove(letter);
                    Destroy(letter);
                }
            }
            lettersByRoom[room].Clear();
            lettersByRoom.Remove(room);
        }
    }

    // Сбрасываем состояние при включении (для нового этажа)
    private void OnEnable()
    {
        initialized = false;
    }
}

