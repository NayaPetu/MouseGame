using UnityEngine;
using Unity.Cinemachine;

public class FloorGenerator : MonoBehaviour
{
    [Header("Floor Prefabs")]
    public GameObject[] floorPrefabs;

    [Header("Player / Enemy")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera; // исправлено с CinemachineCamera

    [Header("Layers")]
    public LayerMask floorLayerMask;

    private GameObject playerInstance;

    // Убираем Start() чтобы генератор не вызывал SpawnFloor автоматически
    // void Start() { SpawnFloor(); }

    public void SpawnFloor()
    {
        if (floorPrefabs.Length == 0) return;

        // 1️⃣ Выбираем случайный префаб этажа
        GameObject prefab = floorPrefabs[Random.Range(0, floorPrefabs.Length)];
        GameObject floor = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 2️⃣ Спавн игрока
        Vector3 playerPos = GetRandomPositionOnFloor(floor);
        playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);

        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        // 3️⃣ Спавн врага (например в другой комнате)
        if (enemyPrefab != null)
        {
            Vector3 enemyPos = GetRandomPositionOnFloor(floor);
            Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }
    }

    Vector3 GetRandomPositionOnFloor(GameObject floor)
    {
        Transform floorTransform = floor.transform.Find("Floor");
        if (floorTransform == null) return floor.transform.position;

        Collider2D floorCollider = floorTransform.GetComponent<Collider2D>();
        if (floorCollider == null) return floor.transform.position;

        Bounds b = floorCollider.bounds;

        // случайная позиция внутри пола
        for (int attempt = 0; attempt < 20; attempt++)
        {
            float x = Random.Range(b.min.x + 0.1f, b.max.x - 0.1f);
            float y = Random.Range(b.min.y + 0.1f, b.max.y - 0.1f);
            Vector3 pos = new Vector3(x, y, 0f);

            if (Physics2D.OverlapCircle(pos, 0.2f, floorLayerMask) != null)
                return pos;
        }

        return b.center;
    }
}
