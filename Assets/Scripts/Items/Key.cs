using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory.Instance.PickKey();
            Destroy(gameObject); // убираем ключ с карты
        }
    }
}
