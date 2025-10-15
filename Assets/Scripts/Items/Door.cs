using UnityEngine;
using Unity.Cinemachine;

public class Door : MonoBehaviour
{
    [Header("Целевая дверь/комната")]
    public Transform targetDoor;
    public Collider2D roomCollider;

    [Header("Телепорт смещение")]
    public Vector3 safeOffset = new Vector3(0.5f, 0f, 0f);

    private bool playerTeleported = false;
    private bool enemyTeleported = false;
    // внутри класса Door
    public void TeleportEnemyToTarget(Transform enemy)
    {
    // используем ту же логику что и TeleportEntity, но публично
        if (targetDoor == null) { Debug.LogWarning($"targetDoor не назначен на двери {name}!"); return; }
        enemy.position = targetDoor.position + safeOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- Если вошёл игрок ---
        if (other.CompareTag("Player") && !playerTeleported)
        {
            TeleportEntity(other.transform, true);
            playerTeleported = true;
        }

        // --- Если вошёл враг ---
        else if (other.CompareTag("Enemy") && !enemyTeleported)
        {
            TeleportEntity(other.transform, false);
            enemyTeleported = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerTeleported = false;

        if (other.CompareTag("Enemy"))
            enemyTeleported = false;
    }

    private void TeleportEntity(Transform entity, bool isPlayer)
    {
        if (targetDoor == null)
        {
            Debug.LogWarning($"targetDoor не назначен на двери {name}!");
            return;
        }

        // Смещение — чтобы не застревали
        entity.position = targetDoor.position + safeOffset;

        // Камеру обновляем только если это игрок
        if (isPlayer && roomCollider != null)
        {
            CinemachineConfiner2D confiner = Camera.main.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
                confiner.BoundingShape2D = roomCollider;
        }
    }
}
