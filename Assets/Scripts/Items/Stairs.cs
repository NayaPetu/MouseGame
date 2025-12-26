using UnityEngine;
using System.Collections;

public class Stair : MonoBehaviour
{
    public FloorManager.FloorCategory targetFloor;
    public string spawnPointName = "StairSpawnPoint";
    public float enemyTeleportDelay = 0.5f;

    private bool playerTeleported = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerTeleported) return;

        playerTeleported = true;
        StartCoroutine(TeleportPlayer(other.transform));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerTeleported = false;
    }

    private IEnumerator TeleportPlayer(Transform player)
    {
        FloorManager.Instance.LoadFloor(targetFloor, spawnPointName, player);

        // Ждём конец кадра, чтобы этаж точно отобразился
        yield return null;

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.SetMovement(true);
    }
}
