using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 15f;

    [Header("Interaction Settings")]
    public float interactionRadius = 1f;
    public LayerMask interactableLayer;
    public LayerMask tileChangeLayer;

    [Header("References")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform itemHoldPosition; // пустой объект над головой игрока

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private GameObject heldItem;
    private bool canMove = true;

    [Header("Animation Settings")]
    public float animationVelocityThreshold = 0.08f;
    private Vector2 lastMoveDir = Vector2.down;

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
        HandleTileChanging();
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

        rb.velocity = currentVelocity;
    }

    private void HandleAnimation()
    {
        bool isMoving = movementInput.magnitude > 0.1f;
        animator.SetBool(IsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(MoveX, movementInput.x);
            animator.SetFloat(MoveY, movementInput.y);
            lastMoveDir = movementInput;
        }
        else
        {
            animator.SetFloat(MoveX, lastMoveDir.x);
            animator.SetFloat(MoveY, lastMoveDir.y);
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem != null)
                DropItem();
            else
                TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
            UseHeldItem();
    }

    private void HandleTileChanging()
    {
        if (Input.GetMouseButtonDown(0) && heldItem != null)
            TryChangeTile();
    }

    private void TryInteract()
    {
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

        foreach (Collider2D collider in interactables)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // Берём предмет игроком
                PickUpItem(collider.gameObject);

                // Вызываем взаимодействие предмета
                interactable.Interact(this);
                break;
            }
        }
    }

    private Vector3 SnapToPixelGrid(Vector3 worldPos, int pixelsPerUnit = 16)
    {
        float unitsPerPixel = 1f / pixelsPerUnit;
        worldPos.x = Mathf.Round(worldPos.x / unitsPerPixel) * unitsPerPixel;
        worldPos.y = Mathf.Round(worldPos.y / unitsPerPixel) * unitsPerPixel;
        return worldPos;
    }

    private void TryChangeTile()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos = SnapToPixelGrid(mouseWorldPos);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, tileChangeLayer);
        if (hit.collider != null)
        {
            TileChanger tileChanger = heldItem.GetComponent<TileChanger>();
            if (tileChanger != null)
                tileChanger.ChangeTile(SnapToPixelGrid(hit.point));
        }
    }

    // ------------------ Подбор предмета ------------------
    public void PickUpItem(GameObject item)
    {
        if (heldItem != null) return;

        heldItem = item;

        // Сделать предмет дочерним объектом над головой игрока
        heldItem.transform.SetParent(itemHoldPosition);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.transform.localScale = Vector3.one;

        // Отображение поверх игрока
        SpriteRenderer sr = heldItem.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
            sr.sortingLayerName = "Default";
        }

        // Выключаем физику и коллайдер
        Rigidbody2D rbItem = heldItem.GetComponent<Rigidbody2D>();
        if (rbItem != null) rbItem.simulated = false;

        Collider2D colItem = heldItem.GetComponent<Collider2D>();
        if (colItem != null) colItem.enabled = false;
    }

    // ------------------ Выброс предмета ------------------
    public void DropItem()
    {
        if (heldItem == null) return;

        Rigidbody2D itemRb = heldItem.GetComponent<Rigidbody2D>();
        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();

        if (itemRb != null) itemRb.simulated = true;
        if (itemCollider != null) itemCollider.enabled = true;

        heldItem.transform.SetParent(null);

        if (itemRb != null)
            itemRb.velocity = rb.velocity * 0.5f;

        heldItem = null;
    }

    private void UseHeldItem()
    {
        if (heldItem == null) return;

        IUsable usable = heldItem.GetComponent<IUsable>();
        if (usable != null)
            usable.Use(this);
    }

    public void SetMovement(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            rb.velocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
    }

    public bool IsCarryingItem() => heldItem != null;
    public GameObject GetHeldItem() => heldItem;

    // ------------------ Направление движения для броска ------------------
    public Vector2 GetCurrentMoveDirection()
    {
        if (movementInput.sqrMagnitude > 0.01f)
            return movementInput.normalized;
        else
            return Vector2.up;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
