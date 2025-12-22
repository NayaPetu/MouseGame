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
        if (targetDoor == null) return;
        enemy.position = targetDoor.position + safeOffset;
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
