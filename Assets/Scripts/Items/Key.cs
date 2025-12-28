using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (PlayerInventory.Instance == null)
            {
                Debug.LogError("[Key] PlayerInventory.Instance равен null! Ключ не может быть подобран.");
                return;
            }
            
            PlayerInventory.Instance.PickKey();
            Destroy(gameObject); // ������� ���� � �����
        }
    }
}
