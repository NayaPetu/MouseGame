using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    public enum FloorCategory { Main, Basement }

    [Header("Генератор этажей")]
    public FloorGenerator floorGenerator;

    private GameObject currentFloor;

    private void Awake() => Instance = this;

    // ---------- Загрузка нового этажа (совместимый метод с 3 аргументами) ----------
    public void LoadFloor(FloorCategory type, string spawnPointName = "PlayerSpawnPoint", Transform playerTransform = null)
    {
        GameObject floor = floorGenerator.SpawnFloorByType(type);
        if (floor == null) return;

        currentFloor = floor;

        // Телепорт игрока если передан Transform
        if (playerTransform != null)
        {
            Transform spawn = floor.transform.Find(spawnPointName);
            if (spawn != null)
                playerTransform.position = spawn.position;
        }

        TeleportEnemyToFloor();
    }

    // ---------- Телепорт врага ----------
    public void TeleportEnemyToFloor()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy == null) return;
        if (currentFloor == null) return;

        Transform spawnPoint = currentFloor.transform.Find("EnemySpawnPoint");
        if (spawnPoint != null)
        {
            enemy.transform.position = spawnPoint.position;

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                Room startRoom = currentFloor.GetComponentInChildren<Room>();
                if (startRoom != null)
                    ai.Init(startRoom, GameObject.FindGameObjectWithTag("Player").transform, spawnPoint.position);
            }
        }
        else
        {
            Debug.LogWarning("Не найдена точка EnemySpawnPoint на этаже.");
        }
    }
}
