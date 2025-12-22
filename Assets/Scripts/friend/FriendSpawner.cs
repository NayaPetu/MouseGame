using UnityEngine;

public class FriendSpawner : MonoBehaviour
{
    public GameObject friendPrefab;

    private bool spawned = false;

    public void TrySpawnFriend(GameObject floor)
    {
        if (spawned || friendPrefab == null) return;

        Transform spawnPoint = floor.transform.Find("FriendSpawnPoint");
        if (spawnPoint == null)
        {
            Debug.Log("FriendSpawnPoint не найден на этаже");
            return;
        }

        Instantiate(friendPrefab, spawnPoint.position, Quaternion.identity);
        spawned = true;

        Debug.Log("🐭 Подруга заспавнена по FriendSpawnPoint");
    }
}
