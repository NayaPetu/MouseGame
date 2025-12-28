using UnityEngine;

public class Recipe : BaseItem
{
    private bool isPickedUp = false; // Флаг, чтобы не подбирать дважды
    
    private void Awake()
    {
        itemName = "Рецепт";
        isConsumable = true;
        Debug.Log("[Recipe] Awake вызван! itemName=" + itemName + ", isConsumable=" + isConsumable);
        
        // Проверяем коллайдер
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"[Recipe] Коллайдер найден: enabled={col.enabled}, isTrigger={col.isTrigger}, type={col.GetType().Name}");
            if (!col.isTrigger)
            {
                Debug.LogWarning("[Recipe] Коллайдер НЕ триггер! Исправляю...");
                col.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("[Recipe] НЕТ КОЛЛАЙДЕРА на объекте!");
        }
        
        // Проверяем наличие ItemPickup
        ItemPickup pickup = GetComponent<ItemPickup>();
        if (pickup == null)
        {
            Debug.LogWarning("[Recipe] ItemPickup не найден! Добавляю...");
            gameObject.AddComponent<ItemPickup>();
        }
        else
        {
            Debug.Log("[Recipe] ItemPickup найден!");
        }
    }
    
    private void Start()
    {
        Debug.Log("[Recipe] Start вызван! GameObject активен: " + gameObject.activeSelf + ", активен в иерархии: " + gameObject.activeInHierarchy);
        
        // Проверяем физические слои
        Debug.Log($"[Recipe] GameObject.layer = {gameObject.layer}, Layer.Name = {LayerMask.LayerToName(gameObject.layer)}");
        
        // Проверяем, может ли игрок взаимодействовать с этим слоем
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"[Recipe] Коллайдер в Start: enabled={col.enabled}, isTrigger={col.isTrigger}");
        }
    }
    
    private void Update()
    {
        // Альтернативный способ подбора через проверку расстояния, если триггер не работает
        if (isPickedUp) return;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < 1.5f) // Радиус подбора
            {
                Debug.Log($"[Recipe] Игрок рядом через Update! Расстояние: {distance}");
                isPickedUp = true;
                PickupRecipe();
            }
        }
    }

    // Прямая обработка триггера в Recipe (на случай, если ItemPickup не работает)
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Recipe] OnTriggerEnter2D вызван! Объект: {other.name}, тег: {other.tag}");
        
        if (isPickedUp) return;
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Recipe] Игрок вошел в триггер рецепта!");
            isPickedUp = true;
            PickupRecipe();
        }
        else
        {
            Debug.Log($"[Recipe] Объект {other.name} не игрок (тег: {other.tag})");
        }
    }

    private void PickupRecipe()
    {
        Debug.Log("[Recipe] PickupRecipe() вызван!");
        
        // Добавляем рецепт в инвентарь
        if (PlayerInventory.Instance != null)
        {
            Debug.Log("[Recipe] PlayerInventory.Instance найден, добавляю рецепт...");
            PlayerInventory.Instance.PickRecipe();
            Debug.Log("[Recipe] Рецепт добавлен в инвентарь!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("[Recipe] PlayerInventory.Instance равен null!");
            // Пытаемся найти PlayerInventory на сцене
            PlayerInventory foundInventory = FindFirstObjectByType<PlayerInventory>();
            if (foundInventory != null)
            {
                Debug.Log("[Recipe] PlayerInventory найден через FindFirstObjectByType, добавляю рецепт...");
                foundInventory.PickRecipe();
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("[Recipe] PlayerInventory не найден на сцене!");
            }
        }
    }

    public override void Use(PlayerController playerController)
    {
        Debug.Log("[Recipe] Use() вызван!");
        PickupRecipe();
        
        // Вызываем базовый метод (но объект уже уничтожен в PickupRecipe)
        // base.Use(playerController);
    }
}
