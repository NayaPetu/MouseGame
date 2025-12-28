using UnityEngine;

public class Furniture : MonoBehaviour
{
    [Header("Настройки мебели")]
    [Tooltip("Размер мебели в тайлах")]
    public Vector2Int sizeInTiles = Vector2Int.one;
    
    [Tooltip("Можно ли ходить через эту мебель (для ковров)")]
    public bool isWalkable = false;
    
    [Header("Автоматическая настройка")]
    [Tooltip("Автоматически настраивать коллайдер при старте")]
    public bool autoSetupCollider = true;
    
    private void Awake()
    {
        if (autoSetupCollider)
        {
            SetupColliderForWalkability();
        }
    }
    
    private void SetupColliderForWalkability()
    {
        if (isWalkable)
        {
            // Для проходимых объектов (ковров) делаем коллайдер триггером или убираем его
            Collider2D[] colliders = GetComponents<Collider2D>();
            
            foreach (Collider2D col in colliders)
            {
                // Если коллайдер нужен для визуальных эффектов или других целей, делаем его триггером
                // Если не нужен вообще - отключаем
                if (col != null)
                {
                    // Проверяем, нужен ли коллайдер для чего-то еще (например, для определения границ)
                    // Для ковров обычно коллайдер не нужен для физики, но может быть нужен для визуализации
                    col.isTrigger = true; // Делаем триггером, чтобы не блокировать движение
                    Debug.Log($"[Furniture] Коллайдер {col.name} на объекте {gameObject.name} установлен как триггер (ковер)");
                }
            }
            
            // Также игнорируем коллизии с игроком через Physics2D
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider != null)
                {
                    foreach (Collider2D col in colliders)
                    {
                        if (col != null)
                        {
                            Physics2D.IgnoreCollision(playerCollider, col, true);
                            Debug.Log($"[Furniture] Игнорирование коллизий между игроком и {gameObject.name}");
                        }
                    }
                }
            }
        }
        else
        {
            // Для непроходимых объектов убеждаемся, что коллайдер не триггер
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                if (col != null && col.isTrigger)
                {
                    // Если коллайдер был триггером, но мебель непроходимая - делаем его обычным
                    // (но только если это не специальный триггер для других целей)
                    // Для безопасности оставляем как есть, если это триггер
                }
            }
        }
    }
    
    // Метод для ручной настройки (можно вызвать из других скриптов)
    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        SetupColliderForWalkability();
    }
}
