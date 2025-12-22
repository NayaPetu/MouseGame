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
        // ������ ����� �������
        if (enemyInstance == null && enemyPrefab != null)
        {
            enemyInstance = Instantiate(enemyPrefab);
            enemyAI = enemyInstance.GetComponent<EnemyAI>();
            enemyInstance.SetActive(false); // ���� �� �������
        }

        // ��������� ������ ���� �������������
        StartCoroutine(LoadInitialFloor());
    }

    private IEnumerator LoadInitialFloor()
    {
        yield return null; // ��� ���� ����, ����� ������� ������ ������������������

        // Загружаем первый этаж
        LoadFloor(FloorCategory.Main, "PlayerSpawnPoint", floorGenerator.GetPlayerInstance().transform);

        // После генерации этажа телепортируем врага на него (один раз при старте игры)
        yield return null;
        yield return StartCoroutine(LateTeleportEnemy());
    }

    public void LoadFloor(FloorCategory type, string spawnPointName, Transform playerTransform)
    {
        GameObject floor = floorGenerator.SpawnFloorByType(type);
        if (floor == null) return;

        currentFloor = floor;

        // �������� ������
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
        // ��� ����, ����� ���� ����� ���������
        yield return null;

        if (enemyInstance == null || currentFloor == null || enemyAI == null)
            yield break;

        Transform spawnPoint = currentFloor.transform.Find("EnemySpawnPoint");
        if (spawnPoint == null)
        {
            Debug.LogWarning("�� ����� ��� EnemySpawnPoint!");
            yield break;
        }

        enemyInstance.transform.position = spawnPoint.position;

        Room room = currentFloor.GetComponentInChildren<Room>();
        if (room == null)
        {
            Debug.LogWarning("�� ����� ��� Room!");
            yield break;
        }

        enemyAI.Init(room, floorGenerator.GetPlayerInstance().transform, spawnPoint.position);

        enemyInstance.SetActive(true);
    }
}
