using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    public RoomGenerator roomGenerator;

    void Start()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("RoomGenerator не назначен!");
            return;
        }

        roomGenerator.GenerateRooms();
    }
}
