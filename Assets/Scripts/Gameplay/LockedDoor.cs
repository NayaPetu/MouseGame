using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Состояние двери")]
    public bool IsOpen = false;

    [Header("Спрайты")]
    public Sprite closedSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;
    private Collider2D[] allColliders; // Все коллайдеры на объекте

    private bool playerNearby = false;

    private bool awakeCalled = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        allColliders = GetComponents<Collider2D>(); // Получаем все коллайдеры
        
        // Исправляем настройки коллайдеров: отключаем "Used By Effector" если эффектора нет
        FixColliderEffectorSettings();
        
        // Проверяем наличие триггера для обнаружения игрока
        // Сначала проверяем на LockedDoor объекте
        bool hasTrigger = false;
        foreach (Collider2D col in allColliders)
        {
            if (col != null && col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log($"[LockedDoor] Найден триггер коллайдер на LockedDoor объекте {gameObject.name}");
                break;
            }
        }
        
        // Если не нашли на LockedDoor, проверяем на Door объекте
        if (!hasTrigger)
        {
            Door doorComponent = GetComponent<Door>();
            if (doorComponent == null && transform.parent != null)
            {
                doorComponent = transform.parent.GetComponent<Door>();
            }
            
            if (doorComponent != null)
            {
                Collider2D[] doorColliders = doorComponent.GetComponents<Collider2D>();
                foreach (Collider2D doorCol in doorColliders)
                {
                    if (doorCol != null && doorCol.isTrigger)
                    {
                        hasTrigger = true;
                        Debug.Log($"[LockedDoor] Найден триггер коллайдер на Door объекте {doorComponent.gameObject.name}");
                        break;
                    }
                }
            }
        }
        
        if (hasTrigger)
        {
            useTriggerDetection = true;
            Debug.Log($"[LockedDoor] Обнаружен триггер коллайдер, используется обнаружение через триггер. Коллайдеров на LockedDoor: {allColliders.Length}");
        }
        else
        {
            useTriggerDetection = false;
            Debug.LogWarning($"[LockedDoor] Нет триггера коллайдера! Будет использоваться проверка расстояния. GameObject: {gameObject.name}, Коллайдеров: {allColliders.Length}");
        }
        
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
            allColliders = GetComponents<Collider2D>();
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
        
        // Обновляем состояние всех коллайдеров (отключаем физические, оставляем триггеры для Door)
        UpdateCollidersState();
    }

    [Header("Настройки взаимодействия")]
    [SerializeField] private float interactionDistance = 1.5f; // Расстояние для взаимодействия если нет триггера
    [SerializeField] private bool useTriggerDetection = true; // Использовать триггер или проверку расстояния

    private void Update()
    {
        // Если дверь уже открыта, не проверяем взаимодействие
        if (IsOpen) return;

        // Если используем проверку расстояния вместо триггера
        if (!useTriggerDetection)
        {
            CheckPlayerDistance();
        }

        // Если игрок рядом, проверяем нажатие E
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[LockedDoor] Игрок нажал E рядом с дверью!");
            TryOpen();
        }
    }

    private void CheckPlayerDistance()
    {
        // Ищем игрока на сцене
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasNearby = playerNearby;
            playerNearby = distance <= interactionDistance;
            
            if (playerNearby && !wasNearby)
            {
                Debug.Log($"[LockedDoor] Игрок рядом с дверью! Расстояние: {distance}");
            }
        }
        else
        {
            playerNearby = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("[LockedDoor] Игрок вошел в триггер двери!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("[LockedDoor] Игрок вышел из триггера двери!");
        }
    }

    public void TryOpen()
    {
        Debug.Log($"[LockedDoor] TryOpen вызван! IsOpen: {IsOpen}, playerNearby: {playerNearby}");
        
        if (IsOpen)
        {
            Debug.Log("[LockedDoor] Дверь уже открыта!");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("[LockedDoor] PlayerInventory.Instance равен null! Проверьте, что PlayerInventory присутствует на сцене.");
            
            // Пытаемся найти PlayerInventory на сцене
            PlayerInventory foundInventory = FindFirstObjectByType<PlayerInventory>();
            if (foundInventory != null)
            {
                Debug.Log("[LockedDoor] PlayerInventory найден через FindFirstObjectByType, но Instance не установлен!");
            }
            return;
        }

        bool hasKey = PlayerInventory.Instance.HasKey();
        Debug.Log($"[LockedDoor] Проверка ключа: hasKey = {hasKey}");

        if (hasKey)
        {
            Debug.Log("[LockedDoor] Ключ найден! Открываю дверь...");
            PlayerInventory.Instance.UseKey();
            OpenDoor();
        }
        else
        {
            Debug.LogWarning("[LockedDoor] НЕТ КЛЮЧА в инвентаре! Убедитесь, что вы подобрали ключ.");
        }
    }

    private void OpenDoor()
    {
        IsOpen = true;
        Debug.Log("Дверь открыта ключом");

        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        // Отключаем все физические коллайдеры (не триггеры) чтобы игрок мог пройти
        UpdateCollidersState();
        
        // Принудительно обновляем физику Unity
        Physics2D.SyncTransforms();
        
        // ДОПОЛНИТЕЛЬНО: Игнорируем коллизии между игроком и дверью
        IgnorePlayerCollisions(true);
        
        // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: Ищем и отключаем коллайдеры на стенах рядом с дверью
        DisableNearbyWallColliders();
        
        // Еще раз обновляем физику после всех изменений
        Physics2D.SyncTransforms();
        
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
                        // Отключаем все коллайдеры на LockedDoor
                        door.lockedDoor.UpdateCollidersState();
                    }
                }
            }
        }
    }
    
    // Обновляет состояние всех коллайдеров на основе состояния двери
    public void UpdateCollidersState()
    {
        // Обновляем массив коллайдеров
        allColliders = GetComponents<Collider2D>();
        
        // Получаем Door компонент
        Door doorComponent = GetComponent<Door>();
        if (doorComponent == null && transform.parent != null)
        {
            doorComponent = transform.parent.GetComponent<Door>();
        }
        
        // Если Door на другом объекте, ищем его через lockedDoor ссылку
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
        
        // ВАЖНО: Если LockedDoor и Door на одном объекте, нужно сохранить Door триггер для телепортации
        bool sameObjectAsDoor = doorComponent != null && doorComponent.gameObject == gameObject;
        
        // Если дверь открыта - отключаем все физические коллайдеры, оставляем только триггеры для Door
        if (IsOpen)
        {
            foreach (Collider2D col in allColliders)
            {
                if (col == null) continue;
                
                if (!col.isTrigger)
                {
                    // Отключаем физические коллайдеры (блокируют проход)
                    col.enabled = false;
                }
                else
                {
                    // Триггеры:
                    // - Если это тот же объект что и Door - оставляем включенным (нужен для телепортации)
                    // - Иначе отключаем (это был триггер для LockedDoor обнаружения игрока)
                    if (!sameObjectAsDoor)
                    {
                        col.enabled = false;
                    }
                    // Если sameObjectAsDoor - оставляем включенным (по умолчанию enabled уже true)
                }
            }
        }
        else
        {
            // Если дверь закрыта - включаем все коллайдеры
            foreach (Collider2D col in allColliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }
        
        // Если Door на другом объекте, обрабатываем его коллайдеры отдельно
        if (doorComponent != null && doorComponent.gameObject != gameObject)
        {
            Collider2D[] doorColliders = doorComponent.GetComponents<Collider2D>();
            
            foreach (Collider2D doorCol in doorColliders)
            {
                if (doorCol == null) continue;
                
                if (IsOpen)
                {
                    // Если дверь открыта:
                    // - Отключаем физические коллайдеры (блокировки)
                    // - Триггеры ОСТАВЛЯЕМ включенными (нужны для телепортации Door)
                    if (!doorCol.isTrigger)
                    {
                        doorCol.enabled = false;
                    }
                    // Триггеры остаются включенными
                }
                else
                {
                    // Если дверь закрыта, включаем все коллайдеры
                    doorCol.enabled = true;
                }
            }
        }
    }
    
    // Игнорирует или включает коллизии между игроком и дверью
    private void IgnorePlayerCollisions(bool ignore)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return;
        
        // Получаем все коллайдеры на двери (включая дочерние)
        Collider2D[] doorColliders = GetComponentsInChildren<Collider2D>(true);
        
        foreach (Collider2D doorCol in doorColliders)
        {
            if (doorCol == null) continue;
            
            // Игнорируем коллизии только с физическими коллайдерами (не триггерами)
            if (!doorCol.isTrigger)
            {
                Physics2D.IgnoreCollision(playerCollider, doorCol, ignore);
                Debug.Log($"[LockedDoor] Игнорирование коллизий между игроком и {doorCol.gameObject.name}: {ignore}");
            }
        }
        
        // Также проверяем Door объект, если он на другом GameObject
        Door doorComponent = GetComponent<Door>();
        if (doorComponent == null && transform.parent != null)
        {
            doorComponent = transform.parent.GetComponent<Door>();
        }
        
        if (doorComponent != null && doorComponent.gameObject != gameObject)
        {
            Collider2D[] doorObjColliders = doorComponent.GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D doorObjCol in doorObjColliders)
            {
                if (doorObjCol == null) continue;
                if (!doorObjCol.isTrigger)
                {
                    Physics2D.IgnoreCollision(playerCollider, doorObjCol, ignore);
                    Debug.Log($"[LockedDoor] Игнорирование коллизий между игроком и Door объектом {doorObjCol.gameObject.name}: {ignore}");
                }
            }
        }
    }
    
    // Исправляет настройки коллайдеров: отключает "Used By Effector" если эффектора нет
    private void FixColliderEffectorSettings()
    {
        // Проверяем наличие эффекторов на объекте
        bool hasEffector = GetComponent<PlatformEffector2D>() != null ||
                          GetComponent<SurfaceEffector2D>() != null ||
                          GetComponent<AreaEffector2D>() != null ||
                          GetComponent<PointEffector2D>() != null ||
                          GetComponent<BuoyancyEffector2D>() != null;
        
        // Если эффекторов нет, отключаем "Used By Effector" на всех коллайдерах
        if (!hasEffector)
        {
            foreach (Collider2D col in allColliders)
            {
                if (col == null) continue;
                
                // Используем рефлексию для доступа к свойству usedByEffector
                var boxCollider = col as BoxCollider2D;
                if (boxCollider != null)
                {
                    // Получаем тип и свойство через рефлексию
                    var type = typeof(BoxCollider2D);
                    var property = type.GetProperty("usedByEffector");
                    if (property != null)
                    {
                        bool usedByEffector = (bool)property.GetValue(boxCollider);
                        if (usedByEffector)
                        {
                            property.SetValue(boxCollider, false);
                            Debug.Log($"[LockedDoor] Отключена опция 'Used By Effector' на коллайдере {col.gameObject.name} (эффекторов нет)");
                        }
                    }
                }
                
                // Также проверяем CircleCollider2D, CapsuleCollider2D и другие
                var circleCollider = col as CircleCollider2D;
                if (circleCollider != null)
                {
                    var type = typeof(CircleCollider2D);
                    var property = type.GetProperty("usedByEffector");
                    if (property != null)
                    {
                        bool usedByEffector = (bool)property.GetValue(circleCollider);
                        if (usedByEffector)
                        {
                            property.SetValue(circleCollider, false);
                            Debug.Log($"[LockedDoor] Отключена опция 'Used By Effector' на коллайдере {col.gameObject.name} (эффекторов нет)");
                        }
                    }
                }
            }
        }
    }
    
    // Отключает коллайдеры на стенах рядом с дверью, которые могут блокировать проход
    private void DisableNearbyWallColliders()
    {
        // Ищем все коллайдеры в радиусе вокруг двери
        float searchRadius = 1.5f; // Радиус поиска коллайдеров
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        
        Debug.Log($"[LockedDoor] Поиск коллайдеров вокруг двери в радиусе {searchRadius}. Найдено: {nearbyColliders.Length}");
        
        foreach (Collider2D nearbyCol in nearbyColliders)
        {
            if (nearbyCol == null) continue;
            if (nearbyCol.isTrigger) continue; // Пропускаем триггеры
            if (nearbyCol.gameObject == gameObject) continue; // Пропускаем коллайдеры самой двери
            
            // Проверяем, не является ли это коллайдером игрока, врага или других важных объектов
            try
            {
                if (nearbyCol.CompareTag("Player") || nearbyCol.CompareTag("Enemy"))
                    continue;
            }
            catch
            {
                // Игнорируем ошибки с тегами
            }
            
            // Проверяем, что это похоже на стену (не предмет, не дверь)
            // Если объект находится очень близко к двери и это физический коллайдер - возможно это стена
            float distance = Vector2.Distance(nearbyCol.transform.position, transform.position);
            
            // Если коллайдер очень близко к двери (в пределах 0.8 единиц), возможно это стена
            if (distance < 0.8f && nearbyCol.enabled)
            {
                // Сохраняем информацию о коллайдере для возможного восстановления
                // Пока просто отключаем его
                nearbyCol.enabled = false;
                Debug.LogWarning($"[LockedDoor] ОТКЛЮЧЕН коллайдер на объекте '{nearbyCol.gameObject.name}' рядом с дверью (расстояние: {distance:F2}). Это может быть стена, блокирующая проход. enabled теперь: {nearbyCol.enabled}");
            }
            else if (distance < 0.8f)
            {
                Debug.Log($"[LockedDoor] Коллайдер на '{nearbyCol.gameObject.name}' рядом с дверью (расстояние: {distance:F2}) уже отключен");
            }
        }
    }
}
