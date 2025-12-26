using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FriendMouse : MonoBehaviour
{
    [Header("Movement")]
    public float followSpeed = 3f;
    public float followDistance = 0.6f;
    public float obstacleCheckDistance = 0.4f;

    [Header("Layers")]
    public LayerMask wallMask;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isFollowing = false;
    private Room currentRoom;

    private Vector2 lastMoveDir;
    private Vector2 currentDir;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Находим текущую комнату
        UpdateCurrentRoom();
    }

    void FixedUpdate()
    {
        if (!isFollowing || player == null)
        {
            HandleAnimation(Vector2.zero, false);
            return;
        }

        float dist = Vector2.Distance(rb.position, player.position);
        Vector2 dir = Vector2.zero;
        bool isMoving = false;

        if (dist > followDistance)
        {
            dir = ((Vector2)player.position - rb.position).normalized;

            // Проверяем стены - Friend не должен летать сквозь них
            RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
            if (wallHit.collider != null)
            {
                // Путь заблокирован стеной - ищем дверь
                Door nearestDoor = FindNearestDoor();
                if (nearestDoor != null)
                {
                    // Идём к двери
                    dir = ((Vector2)nearestDoor.transform.position - rb.position).normalized;
                }
                else
                {
                    // Дверей нет - останавливаемся
                    dir = Vector2.zero;
                }
            }

            if (dir.sqrMagnitude > 0.001f)
            {
                rb.MovePosition(rb.position + dir * followSpeed * Time.fixedDeltaTime);
                isMoving = true;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;
        }

        // Обновляем анимацию
        currentDir = Vector2.Lerp(currentDir, dir, 0.2f);
        HandleAnimation(currentDir, isMoving);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFollowing)
        {
            player = other.transform;
            isFollowing = true;

            GameManager.Instance.friendRescued = true;
            Debug.Log("🐭 Подруга спасена!");
        }

        // Friend проходит через двери так же, как игрок
        if (other.CompareTag("Doors"))
        {
            Door door = other.GetComponent<Door>();
            if (door != null && door.targetDoor != null)
            {
                // Телепортируем Friend через дверь
                rb.position = door.targetDoor.position + door.safeOffset;
                UpdateCurrentRoom();
            }
        }
    }

    private void UpdateCurrentRoom()
    {
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (var room in rooms)
        {
            if (room != null && room.ContainsPoint(rb.position))
            {
                currentRoom = room;
                break;
            }
        }
    }

    private Door FindNearestDoor()
    {
        // Сначала проверяем дверь игрока
        if (Door.LastPlayerDoor != null && currentRoom != null)
        {
            if (Door.LastPlayerDoor.currentRoom == currentRoom)
            {
                return Door.LastPlayerDoor;
            }
        }

        // Ищем все двери в текущей комнате
        if (currentRoom != null && currentRoom.doors != null)
        {
            Door nearestDoor = null;
            float nearestDistance = float.MaxValue;

            foreach (Door door in currentRoom.doors)
            {
                if (door == null) continue;
                
                // Пропускаем закрытые двери
                if (door.lockedDoor != null && !door.lockedDoor.IsOpen)
                    continue;

                float dist = Vector2.Distance(rb.position, door.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestDoor = door;
                }
            }

            return nearestDoor;
        }

        return null;
    }

    private void HandleAnimation(Vector2 dir, bool isMoving)
    {
        animator.SetBool(IsMoving, isMoving);
        
        if (isMoving && dir.sqrMagnitude > 0.001f)
        {
            // Обновляем направление движения
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);
            lastMoveDir = dir;
        }
        else
        {
            // Сохраняем последнее направление для idle анимации
            animator.SetFloat(MoveX, lastMoveDir.x);
            animator.SetFloat(MoveY, lastMoveDir.y);
        }
    }
}
