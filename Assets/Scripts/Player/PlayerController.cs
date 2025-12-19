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
    public Transform itemHoldPosition; // оставили, но не используем

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 currentVelocity;
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

        // Использование предмета из мини-инвентаря
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseInventoryItem();
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        HandleMovement();
    }

    private void GetInput()
    {
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                movementInput * moveSpeed,
                acceleration * Time.fixedDeltaTime
            );
        else
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                Vector2.zero,
                deceleration * Time.fixedDeltaTime
            );

        rb.linearVelocity = currentVelocity;
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

    private void UseInventoryItem()
    {
        if (InventoryManager.Instance == null) return;

        GameObject item = InventoryManager.Instance.GetSelectedItem();
        if (item == null) return;

        // Используем предмет
        item.SendMessage(
            "Use",
            this,
            SendMessageOptions.DontRequireReceiver
        );

        // Удаляем использованный предмет
        InventoryManager.Instance.RemoveSelectedItem();
    }

    public void SetMovement(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
    }

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
