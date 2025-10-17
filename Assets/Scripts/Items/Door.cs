using UnityEngine;
using Unity.Cinemachine;

public class Door : MonoBehaviour
{
    [Header("Целевая дверь/комната")]
    public Transform targetDoor;
    public Collider2D roomCollider;

    [Header("Комната, в которой находится эта дверь")]
    public Room currentRoom;

    [Header("Телепорт смещение")]
    public Vector3 safeOffset = new Vector3(0.5f, 0f, 0f);

    private bool playerTeleported = false;
    private bool enemyTeleported = false;

    public void TeleportEnemyToTarget(Transform enemy)
    {
        if (targetDoor == null) return;
        enemy.position = targetDoor.position + safeOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playerTeleported)
        {
            TeleportEntity(other.transform, true);
            playerTeleported = true;
        }
        else if (other.CompareTag("Enemy") && !enemyTeleported)
        {
            TeleportEntity(other.transform, false);
            enemyTeleported = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerTeleported = false;
        if (other.CompareTag("Enemy")) enemyTeleported = false;
    }

    private void TeleportEntity(Transform entity, bool isPlayer)
    {
        if (targetDoor == null) return;
        entity.position = targetDoor.position + safeOffset;

        if (isPlayer && roomCollider != null)
        {
            CinemachineConfiner2D confiner = Camera.main.GetComponent<CinemachineConfiner2D>();
            if (confiner != null)
                confiner.BoundingShape2D = roomCollider;
        }
    }
}
