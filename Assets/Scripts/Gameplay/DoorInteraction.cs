using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public LockedDoor lockedDoor;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && lockedDoor != null)
        {
            lockedDoor.TryOpen();
        }
    }
}
