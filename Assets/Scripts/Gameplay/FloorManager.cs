using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    public enum FloorCategory { Main, Basement }

    [Header("Генератор этажей")]
    public FloorGenerator floorGenerator;

    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    private GameObject currentFloor;
    private GameObject enemyInstance;

    private void Awake() => Instance = this;

    private void Start()
    {
        // Создаём врага один раз при старте
        if (enemyInstance == null && enemyPrefab != null)
        {
            enemyInstance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            enemyInstance.SetActive(false); // пока не активен
        }
    }

    // Загрузка нового этажа
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
    TeleportEnemyToFloor();
}


    public void TeleportEnemyToFloor()
    {
        if (enemyInstance == null || currentFloor == null) return;

        Transform spawnPoint = currentFloor.transform.Find("EnemySpawnPoint");
        if (spawnPoint != null)
        {
            enemyInstance.transform.position = spawnPoint.position;
            enemyInstance.SetActive(true);

            EnemyAI ai = enemyInstance.GetComponent<EnemyAI>();
            if (ai != null)
            {
                Room startRoom = currentFloor.GetComponentInChildren<Room>();
                if (startRoom != null)
                    ai.Init(startRoom, floorGenerator.GetPlayerInstance().transform, spawnPoint.position);
            }
        }
    }
}
