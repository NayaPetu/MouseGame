using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Настройки спавнера")]
    public GameObject[] itemPrefabs; // все предметы
    public int itemsToSpawn = 5;     // сколько предметов заспавнить
    public Vector2 roomMin;          // нижний левый угол комнаты
    public Vector2 roomMax;          // верхний правый угол комнаты

    [Header("Опционально")]
    public bool randomRotation = false;
    public LayerMask floorLayerMask; // слой пола
    public LayerMask wallLayerMask;  // слой стен, чтобы предметы не застревали

    public void SpawnItems()
    {
        if (itemPrefabs.Length == 0) return;

        for (int i = 0; i < itemsToSpawn; i++)
        {
            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            Vector3 spawnPos = FindSafePosition(0.3f); // radius безопасного спавна

            if (spawnPos != Vector3.zero)
            {
                GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

                if (randomRotation)
                {
                    float zRot = Random.Range(0f, 360f);
                    item.transform.Rotate(0f, 0f, zRot);
                }
            }
        }
    }

    private Vector3 FindSafePosition(float radius)
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            float x = Random.Range(roomMin.x, roomMax.x);
            float y = Random.Range(roomMin.y, roomMax.y);
            Vector2 rayOrigin = new Vector2(x, y + 5f);

            // проверяем пол
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 10f, floorLayerMask);
            if (hit.collider != null)
            {
                Vector3 pos = (Vector3)hit.point + Vector3.up * 0.3f;

                // проверяем стены, чтобы предмет не оказался внутри
                Collider2D overlap = Physics2D.OverlapCircle(pos, radius, wallLayerMask);
                if (overlap == null)
                    return pos;
            }
        }

        Debug.LogWarning("⚠️ Не удалось найти безопасную позицию для предмета");
        return Vector3.zero;
    }
}
