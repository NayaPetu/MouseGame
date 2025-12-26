using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    // Последняя дверь, через которую проходил игрок (вход)
    public static Door LastPlayerDoor;

    [Header("Целевая дверь")]
    public Transform targetDoor;

    [Header("Комната этой двери")]
    public Room currentRoom;

    [Header("Телепорт смещение")]
    public Vector3 safeOffset = new Vector3(0.5f, 0f, 0f);

    [Header("Замок")]
    public LockedDoor lockedDoor; // 👈 ССЫЛКА НА ЗАМОК

    private bool enemyTeleported = false;
    private bool playerTeleported = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ---------- ВРАГ ----------
        if (other.CompareTag("Enemy") && !enemyTeleported)
        {
            Debug.Log($"[Door] Враг вошел в триггер двери! Позиция врага: {other.transform.position}, Позиция двери: {transform.position}");
            TeleportEnemyToTarget(other.transform);
            enemyTeleported = true;
        }

        // ---------- ИГРОК ----------
        if (other.CompareTag("Player") && !playerTeleported)
        {
            // 🚫 ЕСЛИ ДВЕРЬ ЗАКРЫТА — НИЧЕГО НЕ ДЕЛАЕМ
            if (lockedDoor != null && !lockedDoor.IsOpen)
                return;

            StartCoroutine(TeleportPlayer(other.transform));
            playerTeleported = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            enemyTeleported = false;

        if (other.CompareTag("Player"))
            playerTeleported = false;
    }

    // --- Телепорт врага ---
    public void TeleportEnemyToTarget(Transform enemy)
    {
        if (targetDoor == null)
        {
            Debug.LogWarning($"[Door] Попытка телепортации врага, но targetDoor == null!");
            return;
        }
        
        DoTeleportEnemy(enemy);
    }
    
    // Принудительная телепортация врага (вызывается из EnemyAI если враг застрял)
    public void ForceTeleportEnemy(Transform enemy)
    {
        if (targetDoor == null) return;
        
        enemyTeleported = false; // Сбрасываем флаг для принудительной телепортации
        DoTeleportEnemy(enemy);
    }
    
    private void DoTeleportEnemy(Transform enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        Door targetDoorComponent = targetDoor.GetComponent<Door>();
        
        Vector2 oldPos = enemy.position;
        
        if (enemyAI != null && targetDoorComponent != null && targetDoorComponent.currentRoom != null)
        {
            // Обновляем комнату врага ПЕРЕД телепортацией, чтобы избежать проблем с целью
            enemyAI.UpdateCurrentRoom(targetDoorComponent.currentRoom);
        }
        
        // Телепортируем врага после обновления комнаты
        // Используем Rigidbody2D.position напрямую для более надежной телепортации
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        Vector2 newPos = (Vector2)targetDoor.position + (Vector2)safeOffset;
        
        if (enemyRb != null)
        {
            enemyRb.position = newPos;
            enemyRb.linearVelocity = Vector2.zero; // Останавливаем движение после телепортации
        }
        else
        {
            enemy.position = newPos;
        }
        
        enemyTeleported = true; // Устанавливаем флаг после телепортации
        
        Debug.Log($"[Door] Враг телепортирован! Из {oldPos} в {newPos}. Целевая комната: {(targetDoorComponent != null && targetDoorComponent.currentRoom != null ? targetDoorComponent.currentRoom.name : "null")}");
    }

    // --- Телепорт игрока ---
    private IEnumerator TeleportPlayer(Transform player)
    {
        if (targetDoor == null) yield break;

        // Запоминаем, через какую дверь вошёл игрок
        LastPlayerDoor = this;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMovement(false);

        yield return new WaitForSeconds(0.05f);

        player.position = targetDoor.position + safeOffset;

        yield return new WaitForSeconds(0.05f);

        if (controller != null)
            controller.SetMovement(true);
    }
}
