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
        // Создаём врага заранее
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
        yield return null; // ждём один кадр, чтобы система успела инициализироваться

        LoadFloor(FloorCategory.Main, "PlayerSpawnPoint", floorGenerator.GetPlayerInstance().transform);
    }

    public void LoadFloor(FloorCategory type, string spawnPointName, Transform playerTransform)
    {
        GameObject floor = floorGenerator.SpawnFloorByType(type);
        if (floor == null) return;

        currentFloor = floor;

        // Телепорт игрока
        if (playerTransform != null)
        {
            Transform spawn = floor.transform.Find(spawnPointName);
            if (spawn != null)
                playerTransform.position = spawn.position;
        }

        // Телепорт врага
        StartCoroutine(LateTeleportEnemy());
    }

    public void TeleportEnemyToFloorPublic()
    {
        StartCoroutine(LateTeleportEnemy());
    }

    private IEnumerator LateTeleportEnemy()
    {
        // Ждём кадр, чтобы этаж успел создаться
        yield return null;

        if (enemyInstance == null || currentFloor == null || enemyAI == null)
            yield break;

        Transform spawnPoint = currentFloor.transform.Find("EnemySpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogWarning("На этаже нет EnemySpawnPoint!");
            yield break;
        }

        enemyInstance.transform.position = spawnPoint.position;

        Room room = currentFloor.GetComponentInChildren<Room>();
        if (room == null)
        {
            Debug.LogWarning("На этаже нет Room!");
            yield break;
        }

        enemyAI.Init(room, floorGenerator.GetPlayerInstance().transform, spawnPoint.position);

        enemyInstance.SetActive(true);
    }
}
