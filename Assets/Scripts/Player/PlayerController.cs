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
    public Transform itemHoldPosition;
    
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private GameObject heldItem;
    private bool canMove = true;

    [Header("Animation Settings")]
    [Tooltip("Если скорость (м/с) меньше этого — считаем, что персонаж стоит")]
    public float animationVelocityThreshold = 0.08f;
    private Vector2 lastMoveDir = Vector2.down; // направление по-умолчанию вниз

    // Animation parameters (хэши)
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
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void HandleMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            // ускорение к целевой скорости
            currentVelocity = Vector2.MoveTowards(
                currentVelocity, 
                movementInput * moveSpeed, 
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // торможение
            currentVelocity = Vector2.MoveTowards(
                currentVelocity, 
                Vector2.zero, 
                deceleration * Time.fixedDeltaTime
            );
        }
        
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


  


    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem != null)
            {
                DropItem();
            }
            else
            {
                TryInteract();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && heldItem != null)
        {
            UseHeldItem();
        }
    }

    private void HandleTileChanging()
    {
        if (Input.GetMouseButtonDown(0) && heldItem != null)
        {
            TryChangeTile();
        }
    }

    private void TryInteract()
    {
        Collider2D[] interactables = Physics2D.OverlapCircleAll(
            transform.position, 
            interactionRadius, 
            interactableLayer
        );

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
        mouseWorldPos = SnapToPixelGrid(mouseWorldPos); // округляем

        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 0f, tileChangeLayer);
        if (hit.collider != null)
        {
            TileChanger tileChanger = heldItem.GetComponent<TileChanger>();
            if (tileChanger != null)
            {
                tileChanger.ChangeTile(SnapToPixelGrid(hit.point)); // округляем точку изменения
            }
        }
    }


    public void PickUpItem(GameObject item)
    {
        if (heldItem != null) return;
        
        heldItem = item;
        
     
        heldItem.transform.SetParent(itemHoldPosition);
        heldItem.transform.localPosition = Vector3.zero;
        
   
        Rigidbody2D itemRb = heldItem.GetComponent<Rigidbody2D>();
        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
        
        if (itemRb != null) itemRb.simulated = false;
        if (itemCollider != null) itemCollider.enabled = false;
    }

    public void DropItem()
    {
        if (heldItem == null) return;
        
     
        Rigidbody2D itemRb = heldItem.GetComponent<Rigidbody2D>();
        Collider2D itemCollider = heldItem.GetComponent<Collider2D>();
        
        if (itemRb != null) itemRb.simulated = true;
        if (itemCollider != null) itemCollider.enabled = true;
        
   
        heldItem.transform.SetParent(null);
        
     
        if (itemRb != null)
        {
            itemRb.linearVelocity = rb.linearVelocity * 0.5f;
        }
        
        heldItem = null;
    }

    private void UseHeldItem()
    {
        IUsable usable = heldItem.GetComponent<IUsable>();
        if (usable != null)
        {
            usable.Use(this);
        }
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

    public bool IsCarryingItem() => heldItem != null;
    public GameObject GetHeldItem() => heldItem;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
