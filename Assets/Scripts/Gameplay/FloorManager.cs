using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // 🔹 Список всех этажей, чтобы не уничтожать их
    private Dictionary<FloorCategory, GameObject> floors = new Dictionary<FloorCategory, GameObject>();

    private void Awake() => Instance = this;

    private void Start()
    {
        // Создаём врага один раз
        if (enemyInstance == null && enemyPrefab != null)
        {
            enemyInstance = Instantiate(enemyPrefab);
            enemyAI = enemyInstance.GetComponent<EnemyAI>();
            enemyInstance.SetActive(false);
        }

        LoadInitialFloor();
    }

    private void LoadInitialFloor()
    {
        LoadFloor(FloorCategory.Main, "PlayerSpawnPoint", null);
    }

    public void LoadFloor(FloorCategory type, string spawnPointName, Transform playerTransform)
    {
        // Скрываем текущий этаж
        if (currentFloor != null)
            currentFloor.SetActive(false);

        // Если этаж уже был создан — показываем его
        if (floors.ContainsKey(type))
        {
            currentFloor = floors[type];
            currentFloor.SetActive(true);
        }
        else
        {
            // Генерируем новый этаж
            GameObject floor = floorGenerator.SpawnFloorByType(type);
            if (floor == null) return;

            currentFloor = floor;
            floors[type] = floor;
        }

        // Телепортируем игрока
        if (playerTransform != null)
        {
            Transform spawn = currentFloor.transform.Find(spawnPointName);
            if (spawn != null)
                playerTransform.position = spawn.position;

            GameManager.Instance.PlayerCurrentFloor = type;
        }

        // Телепорт врага
        StartCoroutine(TeleportEnemyDelayed(0.5f));
    }

    private IEnumerator TeleportEnemyDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemyInstance == null || currentFloor == null || enemyAI == null)
            yield break;

        Room room = currentFloor.GetComponentInChildren<Room>();
        if (room == null) yield break;

        Vector3 spawnPos = room.GetRoomBounds().center;

        GameObject playerObj = floorGenerator.GetPlayerInstance();
        if (playerObj == null) yield break;

        enemyInstance.transform.position = spawnPos;
        enemyAI.Init(room, playerObj.transform, spawnPos);
        enemyInstance.SetActive(true);
    }
}
