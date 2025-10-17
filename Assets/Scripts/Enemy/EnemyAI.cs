using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 2f;
    public float reachThreshold = 0.1f;

    [Header("Зрение")]
    public float detectionRadius = 8f;
    public LayerMask playerMask;
    public LayerMask wallMask;

    [Header("Патруль")]
    public float patrolWaitTime = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private Room currentRoom;
    private Room targetRoom;
    private Door targetDoor;

    private Vector2 patrolTarget;
    private bool hasPatrolTarget = false;
    private float waitTimer = 0f;
    private bool hasSeenPlayer = false;

    private Vector2 lastMoveDir;
    private Vector2 currentDir;

    private Door[] allDoors;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        animator = GetComponent<Animator>();
    }

    void Start()
    {
        allDoors = Object.FindObjectsByType<Door>(FindObjectsSortMode.None);
        FindCurrentRoom();
        ChooseNextTarget();
    }

    public void Init(Room startRoom, Transform playerTransform)
    {
        currentRoom = startRoom;
        player = playerTransform;
        ChooseNextTarget();
    }

    void Update()
    {
        DetectPlayer();
    }

    void FixedUpdate()
    {
        if (currentRoom == null) FindCurrentRoom();
        if (!hasPatrolTarget) ChooseNextTarget();

        Vector2 targetPos = patrolTarget;

        // Если игрок замечен и в другой комнате
        if (hasSeenPlayer && player != null)
        {
            Room playerRoom = player.GetComponentInParent<Room>();
            if (playerRoom != null && playerRoom != currentRoom)
            {
                // Цель — ближайшая дверь в комнату игрока
                targetDoor = FindDoorToRoom(playerRoom);
                if (targetDoor != null) targetPos = targetDoor.transform.position;
            }
            else
            {
                targetPos = player.position;
            }
        }

        MoveTowards(targetPos);

        // Если дошли до патрульной точки
        if (!hasSeenPlayer && Vector2.Distance(rb.position, patrolTarget) < reachThreshold)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;
                ChooseNextTarget();
            }
        }

        // Если дошли до двери — меняем комнату
        if (targetDoor != null && Vector2.Distance(rb.position, targetDoor.transform.position) < reachThreshold)
        {
            currentRoom = targetDoor.targetDoor.GetComponentInParent<Room>();
            patrolTarget = GetRandomPointInRoom(currentRoom);
            targetDoor = null;
        }
    }

    private void MoveTowards(Vector2 moveTarget)
    {
        Vector2 delta = moveTarget - rb.position;
        if (delta.magnitude < 0.01f)
        {
            HandleAnimation(Vector2.zero, false);
            return;
        }

        Vector2 dir = delta.normalized;
        Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;

        // Проверка столкновений со стенами
        RaycastHit2D hit = Physics2D.CircleCast(rb.position, 0.1f, dir, delta.magnitude, wallMask);
        if (hit.collider == null)
        {
            rb.MovePosition(nextPos);
            currentDir = Vector2.Lerp(currentDir, dir, 0.2f);
            HandleAnimation(currentDir, true);
        }
        else
        {
            HandleAnimation(Vector2.zero, false);
        }
    }

    private void HandleAnimation(Vector2 dir, bool isMoving)
    {
        animator.SetBool(IsMoving, isMoving);
        if (isMoving && dir.sqrMagnitude > 0.001f)
        {
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);
            lastMoveDir = dir;
        }
        else
        {
            Vector2 snapped = new Vector2(
                Mathf.Abs(lastMoveDir.x) > Mathf.Abs(lastMoveDir.y) ? Mathf.Sign(lastMoveDir.x) : 0,
                Mathf.Abs(lastMoveDir.y) >= Mathf.Abs(lastMoveDir.x) ? Mathf.Sign(lastMoveDir.y) : 0
            );
            animator.SetFloat(MoveX, snapped.x);
            animator.SetFloat(MoveY, snapped.y);
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);
        if (hit != null)
        {
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            if (!Physics2D.Raycast(transform.position, dir, detectionRadius, wallMask))
            {
                hasSeenPlayer = true;
                targetRoom = player.GetComponentInParent<Room>();
            }
        }
    }

    private void ChooseNextTarget()
    {
        // Случайная комната
        Room[] rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);
        if (rooms.Length == 0) return;

        targetRoom = rooms[Random.Range(0, rooms.Length)];
        if (targetRoom == null) return;

        // Если цель в другой комнате, идём к двери
        if (targetRoom != currentRoom)
        {
            targetDoor = FindDoorToRoom(targetRoom);
            if (targetDoor != null)
            {
                patrolTarget = targetDoor.transform.position;
            }
        }
        else
        {
            patrolTarget = GetRandomPointInRoom(targetRoom);
        }

        hasPatrolTarget = true;
    }

    private Door FindDoorToRoom(Room room)
    {
        foreach (Door d in allDoors)
        {
            if (d.currentRoom == currentRoom)
            {
                Room dest = d.targetDoor.GetComponentInParent<Room>();
                if (dest == room) return d;
            }
        }
        return null;
    }

    private Vector2 GetRandomPointInRoom(Room room)
    {
        Bounds b = room.GetRoomBounds();
        return new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
    }

    private void FindCurrentRoom()
    {
        Room[] rooms = Object.FindObjectsByType<Room>(FindObjectsSortMode.None);
        foreach (Room r in rooms)
        {
            if (r.ContainsPoint(transform.position))
            {
                currentRoom = r;
                return;
            }
        }
        Debug.LogWarning($"⚠️ Враг {name} не нашёл комнату при старте! Позиция: {transform.position}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Room r = other.GetComponentInParent<Room>();
        if (r != null)
            currentRoom = r;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolTarget, 0.2f);
    }
}
