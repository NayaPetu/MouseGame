using UnityEngine;
using System.Collections;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    public enum FloorCategory { Main, Basement }

    [Header("Generator")]
    public FloorGenerator floorGenerator;

    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    private GameObject currentFloor;
    private EnemyAI enemyAI;
    private GameObject enemyInstance;

    private void Awake()
    {
        Instance = this;

    }

    private void Start()
    {
        // Создаём врага один раз
        if (enemyInstance == null && enemyPrefab != null)
        {
            enemyInstance = Instantiate(enemyPrefab);
            enemyAI = enemyInstance.GetComponent<EnemyAI>();
            enemyInstance.SetActive(false); // пока не активен
        }

        // Загружаем первый этаж автоматически
        StartCoroutine(LoadInitialFloor());
    }

    private IEnumerator LoadInitialFloor()
    {
        yield return null; // ждём один кадр, чтобы генератор точно инициализировался

        // Загружаем первый этаж (это создаст игрока внутри SpawnFloorByType)
        LoadFloor(FloorCategory.Main, "PlayerSpawnPoint", null);

        // Ждём, пока игрок точно создастся
        yield return null;
        yield return null;

        // Теперь получаем игрока и телепортируем врага
        Transform playerTransform = floorGenerator.GetPlayerInstance()?.transform;
        if (playerTransform != null)
        {
            // Обновляем позицию игрока на правильную точку спавна
            Transform spawn = currentFloor.transform.Find("PlayerSpawnPoint");
            if (spawn != null)
                playerTransform.position = spawn.position;
        }

        // После генерации этажа телепортируем врага на него (один раз при старте игры)
        yield return StartCoroutine(LateTeleportEnemy());
    }

    public void LoadFloor(FloorCategory type, string spawnPointName, Transform playerTransform)
    {
        GameObject floor = floorGenerator.SpawnFloorByType(type);
        if (floor == null) return;

        currentFloor = floor;

        // Телепортируем игрока
        if (playerTransform != null)
        {
            Transform spawn = floor.transform.Find(spawnPointName);
            if (spawn != null)
                playerTransform.position = spawn.position;
        }
    }

    public void TeleportEnemyToFloorPublic()
    {
        StartCoroutine(LateTeleportEnemy());
    }

    private IEnumerator LateTeleportEnemy()
    {
        // Ждём кадр, чтобы пол точно сгенерировался
        yield return null;
        yield return null; // Дополнительный кадр для надёжности

        if (enemyInstance == null || currentFloor == null || enemyAI == null)
            yield break;

        Room room = currentFloor.GetComponentInChildren<Room>();
        if (room == null)
        {
            Debug.LogWarning("Нет Room в этаже!");
            yield break;
        }

        Vector3 spawnPos;

        // Сначала пытаемся найти EnemySpawnPoint
        Transform spawnPoint = currentFloor.transform.Find("EnemySpawnPoint");
        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
        }
        else
        {
            // Если точки спавна нет - берём случайную точку внутри комнаты
            Bounds roomBounds = room.GetRoomBounds();
            spawnPos = new Vector3(
                Random.Range(roomBounds.min.x + 1f, roomBounds.max.x - 1f),
                Random.Range(roomBounds.min.y + 1f, roomBounds.max.y - 1f),
                0f
            );

            // Проверяем, что точка действительно внутри комнаты
            int attempts = 0;
            while (!room.ContainsPoint(spawnPos) && attempts < 10)
            {
                spawnPos = new Vector3(
                    Random.Range(roomBounds.min.x + 1f, roomBounds.max.x - 1f),
                    Random.Range(roomBounds.min.y + 1f, roomBounds.max.y - 1f),
                    0f
                );
                attempts++;
            }

            // Если всё равно не нашли - используем центр комнаты
            if (!room.ContainsPoint(spawnPos))
            {
                spawnPos = roomBounds.center;
            }
        }

        // Убеждаемся, что игрок создан
        GameObject playerObj = floorGenerator.GetPlayerInstance();
        if (playerObj == null)
        {
            Debug.LogWarning("Игрок ещё не создан! Ждём...");
            yield return null;
            playerObj = floorGenerator.GetPlayerInstance();
            if (playerObj == null)
            {
                Debug.LogError("Не удалось найти игрока для инициализации врага!");
                yield break;
            }
        }

        enemyInstance.transform.position = spawnPos;
        enemyAI.Init(room, playerObj.transform, spawnPos);

        enemyInstance.SetActive(true);
    }
}
