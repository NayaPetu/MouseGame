using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Целевая дверь")]
    public Transform targetDoor; // 👈 не трогаем!

    [Header("Комната этой двери")]
    public Room currentRoom; // 👈 не трогаем!

    [Header("Телепорт смещение")]
    public Vector3 safeOffset = new Vector3(0.5f, 0f, 0f); // 👈 не трогаем!

    private bool enemyTeleported = false;
    private bool playerTeleported = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- Враг ---
        if (other.CompareTag("Enemy") && !enemyTeleported)
        {
            TeleportEnemyToTarget(other.transform);
            enemyTeleported = true;
        }

        // --- Игрок ---
        if (other.CompareTag("Player") && !playerTeleported)
        {
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

        // Блокируем движение, чтобы не дергался
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMovement(false);

        yield return new WaitForSeconds(0.05f);

        // Перемещаем
        player.position = targetDoor.position + safeOffset;

        yield return new WaitForSeconds(0.05f);

        if (controller != null)
            controller.SetMovement(true);
    }
}
