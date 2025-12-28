using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private BaseItem item;

    private void Awake()
    {
        item = GetComponent<BaseItem>();
        if (item == null)
        {
            Debug.LogError($"[ItemPickup] На объекте {gameObject.name} нет компонента, наследующего BaseItem!");
        }
        else
        {
            Debug.Log($"[ItemPickup] Найден BaseItem: {item.itemName} на объекте {gameObject.name}");
        }
    }
    
    private void Start()
    {
        // Дополнительная проверка - попробуем найти снова, если не нашли в Awake
        if (item == null)
        {
            item = GetComponent<BaseItem>();
            if (item != null)
            {
                Debug.Log($"[ItemPickup] BaseItem найден в Start: {item.itemName} на объекте {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[ItemPickup] BaseItem не найден на объекте {gameObject.name} даже в Start!");
                // Попробуем найти любой компонент, наследующий BaseItem
                MonoBehaviour[] components = GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp is BaseItem)
                    {
                        item = comp as BaseItem;
                        Debug.Log($"[ItemPickup] Найден BaseItem через перебор компонентов: {item.itemName}");
                        break;
                    }
                }
            }
        }
        
        // Дополнительная проверка компонентов
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"[ItemPickup] Коллайдер на {gameObject.name}: enabled={col.enabled}, isTrigger={col.isTrigger}, type={col.GetType().Name}");
        }
        else
        {
            Debug.LogError($"[ItemPickup] На объекте {gameObject.name} нет коллайдера!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игнорируем, если это не игрок
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        // Если item не найден, пытаемся найти снова
        if (item == null)
        {
            item = GetComponent<BaseItem>();
            if (item == null)
            {
                Debug.LogError($"[ItemPickup] BaseItem не найден на {gameObject.name}! Не могу подобрать предмет.");
                return;
            }
        }
        
        Debug.Log($"[ItemPickup] Игрок вошел в триггер {gameObject.name}! Предмет: {item.itemName}");
        
        // Вызываем метод использования предмета
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log($"[ItemPickup] Вызываю item.Use() для {item.itemName}...");
            item.Use(player);
        }
        else
        {
            Debug.LogError($"[ItemPickup] PlayerController не найден на объекте игрока!");
        }

        // Уничтожаем предмет только если это не письмо друга
        // Письма не уничтожаются, чтобы их можно было читать повторно
        FriendNote friendNote = GetComponent<FriendNote>();
        if (friendNote == null)
        {
            // Если предмет consumable, он уже уничтожен в BaseItem.Use()
            // Если нет - уничтожаем здесь
            if (item != null && !item.isConsumable)
            {
                Debug.Log($"[ItemPickup] Уничтожаю не-consumable предмет {item.itemName}");
                Destroy(gameObject);
            }
        }
    }
}
