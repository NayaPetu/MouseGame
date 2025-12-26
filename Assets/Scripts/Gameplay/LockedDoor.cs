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

    private bool playerNearby = false;

    private bool awakeCalled = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        // При первом создании объекта устанавливаем закрытый спрайт
        if (closedSprite != null && sr != null)
        {
            sr.sprite = closedSprite;
        }
        awakeCalled = true;
    }

    private void OnEnable()
    {
        // Инициализируем компоненты если Awake еще не вызван
        if (!awakeCalled)
        {
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            if (closedSprite != null && sr != null && !IsOpen)
            {
                sr.sprite = closedSprite;
            }
        }
        
        // При активации объекта обновляем спрайт в соответствии с текущим состоянием
        if (sr != null)
        {
            if (IsOpen && openSprite != null)
            {
                sr.sprite = openSprite;
            }
            else if (!IsOpen && closedSprite != null)
            {
                sr.sprite = closedSprite;
            }
        }
        
        // Обновляем состояние коллайдера
        if (col != null)
        {
            col.enabled = !IsOpen;
        }
    }

    private void Update()
    {
        if (!playerNearby || IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpen();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
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

    private void OpenDoor()
    {
        IsOpen = true;
        Debug.Log("Дверь открыта ключом");

        if (sr != null && openSprite != null)
            sr.sprite = openSprite;

        if (col != null)
            col.enabled = false;
    }
}
