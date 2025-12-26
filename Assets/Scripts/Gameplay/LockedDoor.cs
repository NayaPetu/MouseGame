using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Состояние двери")]
    public bool IsOpen = false;

    [Header("Спрайты")]
    public Sprite closedSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;
    private Collider2D col;

    private bool playerNearby = false;

    private bool awakeCalled = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        // При первом создании объекта устанавливаем закрытый спрайт
        if (closedSprite != null && sr != null)
        {
            sr.sprite = closedSprite;
        }
        awakeCalled = true;
    }

    private void OnEnable()
    {
        // Инициализируем компоненты если Awake еще не вызван
        if (!awakeCalled)
        {
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            if (closedSprite != null && sr != null && !IsOpen)
            {
                sr.sprite = closedSprite;
            }
        }
        
        // Проверяем, должна ли дверь быть открыта (если комната уже открыта)
        if (!IsOpen)
        {
            // Получаем Door компонент (LockedDoor может быть на том же GameObject, что и Door, или на дочернем)
            Door doorComponent = GetComponent<Door>();
            if (doorComponent == null && transform.parent != null)
            {
                doorComponent = transform.parent.GetComponent<Door>();
            }
            
            // Если не нашли через GetComponent, ищем через все Door в сцене
            if (doorComponent == null)
            {
                Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
                foreach (Door door in allDoors)
                {
                    if (door != null && door.lockedDoor == this)
                    {
                        doorComponent = door;
                        break;
                    }
                }
            }
            
            if (doorComponent != null && doorComponent.targetDoor != null)
            {
                Door targetDoorComponent = doorComponent.targetDoor.GetComponent<Door>();
                if (targetDoorComponent != null && targetDoorComponent.currentRoom != null)
                {
                    string targetRoomName = targetDoorComponent.currentRoom.roomName;
                    if (!string.IsNullOrEmpty(targetRoomName) && GameManager.IsRoomOpened(targetRoomName))
                    {
                        // Комната уже открыта, открываем эту дверь тоже
                        IsOpen = true;
                    }
                }
            }
        }
        
        // При активации объекта обновляем спрайт в соответствии с текущим состоянием
        if (sr != null)
        {
            if (IsOpen && openSprite != null)
            {
                sr.sprite = openSprite;
            }
            else if (!IsOpen && closedSprite != null)
            {
                sr.sprite = closedSprite;
            }
        }
        
        // Обновляем состояние коллайдера
        if (col != null)
        {
            col.enabled = !IsOpen;
        }
    }

    private void Update()
    {
        if (!playerNearby || IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    public void TryOpen()
    {
        if (IsOpen) return;

        if (PlayerInventory.Instance.HasKey())
        {
            PlayerInventory.Instance.UseKey();
            OpenDoor();
        }
        else
        {
            Debug.Log("Нет ключа");
        }
    }

    private void OpenDoor()
    {
        IsOpen = true;
        Debug.Log("Дверь открыта ключом");

        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        if (col != null)
            col.enabled = false;
        
        // Получаем Door компонент (LockedDoor может быть на том же GameObject, что и Door, или на дочернем)
        Door doorComponent = GetComponent<Door>();
        if (doorComponent == null && transform.parent != null)
        {
            doorComponent = transform.parent.GetComponent<Door>();
        }
        
        // Если не нашли через GetComponent, ищем через все Door в сцене
        if (doorComponent == null)
        {
            Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
            foreach (Door door in allDoors)
            {
                if (door != null && door.lockedDoor == this)
                {
                    doorComponent = door;
                    break;
                }
            }
        }
        
        if (doorComponent != null && doorComponent.targetDoor != null)
        {
            Door targetDoorComponent = doorComponent.targetDoor.GetComponent<Door>();
            if (targetDoorComponent != null && targetDoorComponent.currentRoom != null)
            {
                string targetRoomName = targetDoorComponent.currentRoom.roomName;
                if (!string.IsNullOrEmpty(targetRoomName))
                {
                    // Помечаем комнату как открытую
                    GameManager.MarkRoomAsOpened(targetRoomName);
                    
                    // Открываем все двери, ведущие в эту комнату
                    OpenAllDoorsToRoom(targetRoomName);
                }
            }
        }
    }
    
    // Открывает все двери, ведущие в указанную комнату
    private void OpenAllDoorsToRoom(string roomName)
    {
        // Находим все двери в сцене
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        
        foreach (Door door in allDoors)
        {
            if (door == null || door.targetDoor == null) continue;
            
            // Проверяем, ведет ли эта дверь в целевую комнату
            Door targetDoor = door.targetDoor.GetComponent<Door>();
            if (targetDoor != null && targetDoor.currentRoom != null)
            {
                if (targetDoor.currentRoom.roomName == roomName)
                {
                    // Открываем LockedDoor на этой двери, если она есть
                    if (door.lockedDoor != null && !door.lockedDoor.IsOpen)
                    {
                        door.lockedDoor.IsOpen = true;
                        if (door.lockedDoor.openSprite != null)
                        {
                            SpriteRenderer sr = door.lockedDoor.GetComponent<SpriteRenderer>();
                            if (sr != null)
                                sr.sprite = door.lockedDoor.openSprite;
                        }
                        Collider2D col = door.lockedDoor.GetComponent<Collider2D>();
                        if (col != null)
                            col.enabled = false;
                    }
                }
            }
        }
    }
}
