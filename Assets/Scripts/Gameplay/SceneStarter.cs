using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    public FloorGenerator floorGenerator;

    void Start()
    {
        if (floorGenerator == null)
        {
            Debug.LogError("RoomGenerator не назначен!");
            return;
        }

        floorGenerator.SpawnFloor(); 
    }
}
