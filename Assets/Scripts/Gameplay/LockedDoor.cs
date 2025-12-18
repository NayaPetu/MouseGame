using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Состояние двери")]
    public bool IsOpen = false;

    [Header("Спрайты")]
    public Sprite closedSprite;
    public Sprite openSprite;

    private SpriteRenderer sr;
    private Collider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sr.sprite = closedSprite; // по умолчанию закрыта
    }

    public void TryOpen()
    {
        if (IsOpen) return;

        if (PlayerInventory.Instance.HasKey())
        {
            PlayerInventory.Instance.UseKey();
            OpenDoor();
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

        // ✅ МЕНЯЕМ СПРАЙТ
        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        // ✅ ОТКЛЮЧАЕМ КОЛЛАЙДЕР
        if (col != null)
            col.enabled = false;
    }
}

