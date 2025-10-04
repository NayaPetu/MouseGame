using UnityEngine;

public class SceneStarter : MonoBehaviour
{
    public RoomGenerator roomGenerator;

    void Start()
    {
        if (roomGenerator == null)
        {
            Debug.LogError("RoomGenerator �� ��������!");
            return;
        }

        roomGenerator.GenerateRooms();
    }
}
