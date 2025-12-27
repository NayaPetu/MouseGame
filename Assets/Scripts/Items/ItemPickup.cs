using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private BaseItem item;

    private void Awake()
    {
        item = GetComponent<BaseItem>();
        if (item == null)
            Debug.LogWarning("На объекте нет компонента, наследующего BaseItem!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && item != null)
        {
            // Вызываем метод использования предмета
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                item.Use(player);
            }

            // Уничтожаем предмет только если это не письмо друга
            // Письма не уничтожаются, чтобы их можно было читать повторно
            FriendNote friendNote = GetComponent<FriendNote>();
            if (friendNote == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
