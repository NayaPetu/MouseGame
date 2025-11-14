using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance;

    public enum FloorCategory
    {
        Main,
        Basement
    }

    [Header("Генератор")]
    public FloorGenerator floorGenerator;

    private GameObject currentFloor;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadFloor(FloorCategory type, string spawnPointName, Transform player)
    {
        GameObject newFloor = floorGenerator.SpawnFloorByType(type);
        if (newFloor == null)
        {
            Debug.LogError("Не удалось создать этаж!");
            return;
        }

        currentFloor = newFloor;

        Transform spawn = newFloor.transform.Find(spawnPointName);

        if (spawn != null)
            player.position = spawn.position;
        else
            Debug.LogWarning("Не найдена точка спавна: " + spawnPointName);
    }
}
