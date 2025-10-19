using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private BaseItem item;

    private void Awake()
    {
        item = GetComponent<BaseItem>();
        if (item == null)
            Debug.LogWarning("На этом объекте нет компонента BaseItem!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && item != null)
        {
            // Вызываем эффект предмета
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                item.Use(player);
            }

            // Удаляем предмет после подбора
            Destroy(gameObject);
        }
    }
}
