using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("��������� ��������")]
    public GameObject[] itemPrefabs; // ��� ��������
    public int itemsToSpawn = 5;     // ������� ��������� ����������
    public Vector2 roomMin;          // ������ ����� ���� �������
    public Vector2 roomMax;          // ������� ������ ���� �������

    [Header("�����������")]
    public bool randomRotation = false;

    public void SpawnItems()
    {
        for (int i = 0; i < itemsToSpawn; i++)
        {
            if (itemPrefabs.Length == 0) return;

            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            Vector2 spawnPos = new Vector2(
                Random.Range(roomMin.x, roomMax.x),
                Random.Range(roomMin.y, roomMax.y)
            );

            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

            if (randomRotation)
            {
                float zRot = Random.Range(0f, 360f);
                item.transform.Rotate(0f, 0f, zRot);
            }
        }
    }
}
