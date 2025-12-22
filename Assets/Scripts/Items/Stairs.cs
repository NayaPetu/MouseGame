using UnityEngine;
using System.Collections;

public class Stair : MonoBehaviour
{
    public FloorManager.FloorCategory targetFloor;  
    public string spawnPointName = "StairSpawnPoint";

    [Header("Переход врага")]
    [Tooltip("Задержка перед тем, как враг перейдёт на новый этаж за игроком")]
    public float enemyTeleportDelay = 1.5f;

    private bool playerTeleported = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerTeleported)
            return;

        playerTeleported = true;
        StartCoroutine(Teleport(other.transform));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerTeleported = false;
    }

    private IEnumerator Teleport(Transform player)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMovement(false);

        yield return new WaitForSeconds(0.05f);

        // ������������� ������ �� ����� ����
        FloorManager.Instance.LoadFloor(targetFloor, spawnPointName, player);

        yield return new WaitForSeconds(0.2f);

        if (controller != null)
            controller.SetMovement(true);

        // ������������� ����� ����� ��������
        StartCoroutine(TeleportEnemyDelayed(enemyTeleportDelay));
    }

    private IEnumerator TeleportEnemyDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ���������� ���������� ��� ������ - TeleportEnemyToFloorPublic()
        FloorManager.Instance.TeleportEnemyToFloorPublic();
    }
}