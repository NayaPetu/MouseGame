using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private BaseItem item;

    private void Awake()
    {
        item = GetComponent<BaseItem>();
        if (item == null)
            Debug.LogWarning("�� ���� ������� ��� ���������� BaseItem!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && item != null)
        {
            // �������� ������ ��������
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                item.Use(player);
            }

            // ������� ������� ����� �������
            Destroy(gameObject);
        }
    }
}
