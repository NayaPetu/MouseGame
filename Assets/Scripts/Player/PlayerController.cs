using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Interaction Settings")]
    public float interactionRadius = 1f;
    public LayerMask interactableLayer;

    [Header("References")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform itemHoldPosition;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private GameObject heldItem;
    private bool canMove = true;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
{
    if (!canMove) return;

    GetInput();
    HandleAnimation();
    HandleInteraction();

    // ------------------- Новый функционал инвентаря -------------------
    if (Input.GetKeyDown(KeyCode.RightArrow))
        PlayerInventory.Instance.NextItem();

    if (Input.GetKeyDown(KeyCode.LeftArrow))
        PlayerInventory.Instance.PreviousItem();

    if (Input.GetKeyDown(KeyCode.E))
        PlayerInventory.Instance.UseSelectedItem(this);
}


    void FixedUpdate()
    {
        if (!canMove) return;
        HandleMovement();
    }

    private void GetInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
            currentVelocity = Vector2.MoveTowards(currentVelocity, movementInput * moveSpeed, acceleration * Time.fixedDeltaTime);
        else
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);

        rb.linearVelocity = currentVelocity; // 🔹 исправлено
    }

    private void HandleAnimation()
    {
        bool isMoving = movementInput.magnitude > 0.1f;
        animator.SetBool(IsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(MoveX, movementInput.x);
            animator.SetFloat(MoveY, movementInput.y);
        }
    }

    private void HandleInteraction()
    {
        // -------- E: только взаимодействие, мята не дропается
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);
            foreach (Collider2D collider in interactables)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                    break;
                }
            }
        }

        // -------- Q: использовать или дропнуть
        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            IUsable usable = heldItem.GetComponent<IUsable>();

            if (usable != null)
            {
                usable.Use(this); // Используем предмет
            }
            else
            {
                DropItem(); // Дропаем обычные предметы
            }
        }
    }

    // ------------------ Подбор предмета ------------------
    public void PickUpItem(GameObject item)
    {
        heldItem = item;

        if (item.GetComponent<Catnip>() != null)
        {
            // Для мяты — делаем объект невидимым и отключаем физику
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            Collider2D col = item.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rbItem = item.GetComponent<Rigidbody2D>();
            if (rbItem != null) rbItem.simulated = false;
        }
        else
        {
            // Стандартный предмет — переносим в руки
            item.transform.SetParent(itemHoldPosition);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;

            Rigidbody2D rbItem = item.GetComponent<Rigidbody2D>();
            if (rbItem != null) rbItem.simulated = false;

            Collider2D colItem = item.GetComponent<Collider2D>();
            if (colItem != null) colItem.enabled = false;
        }
    }

    // ------------------ Дроп предмета ------------------
    public void DropItem()
    {
        if (heldItem == null) return;

        if (heldItem.GetComponent<Catnip>() != null)
        {
            // Мята — возвращаем на сцену рядом с игроком
            heldItem.transform.SetParent(null);
            heldItem.transform.position = transform.position + Vector3.up * 0.5f;

            SpriteRenderer sr = heldItem.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = true;

            Collider2D col = heldItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            Rigidbody2D rbItem = heldItem.GetComponent<Rigidbody2D>();
            if (rbItem != null) rbItem.simulated = true;
        }
        else
        {
            Rigidbody2D itemRb = heldItem.GetComponent<Rigidbody2D>();
            Collider2D itemCollider = heldItem.GetComponent<Collider2D>();

            if (itemRb != null) itemRb.simulated = true;
            if (itemCollider != null) itemCollider.enabled = true;

            heldItem.transform.SetParent(null);
            if (itemRb != null)
                itemRb.linearVelocity = rb.linearVelocity * 0.5f;
        }

        heldItem = null;
    }

    private void UseHeldItem()
    {
        if (heldItem == null) return;

        IUsable usable = heldItem.GetComponent<IUsable>();
        if (usable != null)
            usable.Use(this);
    }

    public GameObject GetHeldItem() => heldItem;

    public void SetMovement(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero; // 🔹 исправлено
            currentVelocity = Vector2.zero;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
   

}
