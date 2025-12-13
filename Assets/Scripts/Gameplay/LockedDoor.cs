using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Состояние двери")]
    public bool IsOpen = false;

    public void TryOpen()
    {
        if (IsOpen) return; // уже открыта

        if (PlayerInventory.Instance.HasKey())
        {
            OpenDoor();
            PlayerInventory.Instance.UseKey();
        }
        else
        {
            Debug.Log("Нет ключа");
        }
    }

    public void OpenDoor()
    {
        IsOpen = true;
        Debug.Log("Дверь открыта ключом");

        // здесь можно сменить спрайт на открытую дверь
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f); // пример: прозрачная
        }

        // отключаем коллайдер, который блокирует движение
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
