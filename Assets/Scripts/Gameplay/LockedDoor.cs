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

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        sr.sprite = closedSprite;
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
