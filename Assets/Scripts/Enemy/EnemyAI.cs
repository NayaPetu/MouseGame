using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float obstacleCheckDistance = 0.4f;

    [Header("Vision")]
    public float detectionRadius = 8f;
    public LayerMask playerMask;
    public LayerMask wallMask;

    [Header("Catnip")]
    public LayerMask catnipMask;
    public float catnipPeaceTime = 5f;
    private bool pacifiedByCatnip = false;
    private Catnip targetCatnip;
    private bool isResting = false;

    [Header("Patrol")]
    public float patrolWaitTime = 2f;

    [Header("PowerCheese")]
    public float powerCheeseScaleThreshold = 1.2f;
    public float fleeSpeed = 3f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private Room currentRoom;

    private Vector2 patrolTarget;
    private bool hasPatrolTarget = false;
    private float waitTimer = 0f;
    private bool hasSeenPlayer = false;

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
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        animator = GetComponent<Animator>();
    }

    // Инициализация врага
    public void Init(Room room, Transform playerTransform, Vector3 spawnPos)
    {
        player = playerTransform;
        currentRoom = room;
        transform.position = spawnPos;

        // Сброс состояния
        hasSeenPlayer = false;
        pacifiedByCatnip = false;
        targetCatnip = null;
        isResting = false;

        ChoosePatrolTarget();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (isResting) return;

        DetectPlayer();
        DetectCatnip();
    }

    void FixedUpdate()
    {
        if (isResting)
        {
            rb.linearVelocity = Vector2.zero;
            HandleAnimation(Vector2.zero, false);
            return;
        }

        if (pacifiedByCatnip && targetCatnip != null)
            MoveToCatnip();
        else
            PatrolAndChase();

        CheckPlayerCatch();
    }

    // ------------------ CATNIP -----------------------
    private void DetectCatnip()
    {
        if (pacifiedByCatnip || isResting) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, catnipMask);
        foreach (var hit in hits)
        {
            Catnip catnip = hit.GetComponent<Catnip>();
            if (catnip != null && catnip.IsUsed() && !catnip.IsEaten())
            {
                targetCatnip = catnip;
                pacifiedByCatnip = true;
                break;
            }
        }
    }

    private void MoveToCatnip()
    {
        if (targetCatnip == null)
        {
            pacifiedByCatnip = false;
            return;
        }

        Vector2 dir = ((Vector2)targetCatnip.transform.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        HandleAnimation(dir, true);

        if (Vector2.Distance(rb.position, targetCatnip.transform.position) < 0.2f)
        {
            targetCatnip.EatenByCat();
            targetCatnip = null;
            StartCoroutine(CatnipPeace());
        }
    }

    private IEnumerator CatnipPeace()
    {
        isResting = true;
        pacifiedByCatnip = false;
        targetCatnip = null;
        HandleAnimation(Vector2.zero, false);

        yield return new WaitForSeconds(catnipPeaceTime);

        isResting = false;
        ChoosePatrolTarget();
    }

    // ------------------ PATROL & CHASE -------------------------
    private void PatrolAndChase()
    {
        if (player == null) return;

        // Проверяем, увеличен ли игрок
        bool isPlayerEnlarged = IsPlayerEnlarged();
        
        if (isPlayerEnlarged)
        {
            FleeFromPlayer();
            return;
        }

        Vector2 moveTarget;

        if (hasSeenPlayer)
        {
            // Если есть прямой обзор на игрока — гонимся напрямую
            if (HasLineOfSightToPlayer())
            {
                moveTarget = (Vector2)player.position;
            }
            // Если игрок спрятался за стеной и есть информация о двери,
            // через которую он прошёл — сначала идём к этой двери
            else if (Door.LastPlayerDoor != null)
            {
                moveTarget = (Vector2)Door.LastPlayerDoor.transform.position;
            }
            else
            {
                moveTarget = (Vector2)player.position;
            }
        }
        else if (hasPatrolTarget)
        {
            moveTarget = patrolTarget;
        }
        else
        {
            ChoosePatrolTarget();
            return;
        }

        float distanceToTarget = Vector2.Distance(rb.position, moveTarget);

        // Если цель — дверь, через которую прошёл игрок, не останавливаемся "рядом"
        bool isDoorTarget = Door.LastPlayerDoor != null &&
                            Vector2.Distance(moveTarget, (Vector2)Door.LastPlayerDoor.transform.position) < 0.05f;

        if (!isDoorTarget && distanceToTarget < 0.15f)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;
                ChoosePatrolTarget();
            }
            HandleAnimation(Vector2.zero, false);
            return;
        }

        Vector2 dir = (moveTarget - rb.position).normalized;

        // Всегда проверяем стены — кот не должен идти сквозь них
        RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
        if (wallHit.collider != null)
        {
            // Если целимся прямо в стену перед собой, останавливаемся
            dir = Vector2.zero;
        }

        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        currentDir = Vector2.Lerp(currentDir, dir, 0.15f);
        HandleAnimation(currentDir, dir.sqrMagnitude > 0.001f);
    }

    // ------------------ PLAYER DETECTION -----------------------
    private void DetectPlayer()
    {
        if (player == null || isResting) return;

        // Не обнаруживаем увеличенного игрока для преследования
        if (IsPlayerEnlarged())
        {
            hasSeenPlayer = false;
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);
        if (hit != null)
        {
            // Считаем, что враг "увидел" игрока только если между ними нет стены
            if (HasLineOfSightToPlayer())
                hasSeenPlayer = true;
        }
    }

    /// <summary>
    /// Проверка прямой видимости игрока (нет стены между котом и мышью).
    /// </summary>
    private bool HasLineOfSightToPlayer()
    {
        if (player == null) return false;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, wallMask);
        // Если луч не попал в стену — значит стена не блокирует обзор
        return hit.collider == null;
    }

    private void CheckPlayerCatch()
    {
        // Кот не может ловить игрока, если он отдыхает после мяты или если игрок увеличен
        if (player == null || isResting || IsPlayerEnlarged()) return;

        float catchDistance = 0.5f;
        if (Vector2.Distance(transform.position, player.position) <= catchDistance)
        {
            GameManager.Instance.OnPlayerCaught();
        }
    }

    // ------------------ PATROL HELPERS -----------------------
    private void ChoosePatrolTarget()
    {
        // Патрулируем в пределах текущей комнаты
        if (currentRoom != null)
        {
            Bounds roomBounds = currentRoom.GetRoomBounds();
            patrolTarget = new Vector2(
                Random.Range(roomBounds.min.x + 1f, roomBounds.max.x - 1f),
                Random.Range(roomBounds.min.y + 1f, roomBounds.max.y - 1f)
            );
        }
        else
        {
            // Если комнаты нет - патрулируем вокруг текущей позиции
            patrolTarget = (Vector2)transform.position + Random.insideUnitCircle * 3f;
        }
        hasPatrolTarget = true;
        waitTimer = 0f;
    }

    /// <summary>
    /// Проверяет, увеличен ли игрок (эффект PowerCheese)
    /// </summary>
    private bool IsPlayerEnlarged()
    {
        if (player == null) return false;
        return player.localScale.magnitude > powerCheeseScaleThreshold;
    }

    /// <summary>
    /// Убегание от увеличенного игрока
    /// </summary>
    private void FleeFromPlayer()
    {
        if (player == null) return;

        Vector2 dirFromPlayer = ((Vector2)transform.position - (Vector2)player.position).normalized;
        
        // Проверяем стены при убегании
        RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dirFromPlayer, obstacleCheckDistance, wallMask);
        if (wallHit.collider != null)
        {
            dirFromPlayer = Vector2.zero;
        }

        rb.MovePosition(rb.position + dirFromPlayer * fleeSpeed * Time.fixedDeltaTime);
        HandleAnimation(dirFromPlayer, dirFromPlayer.sqrMagnitude > 0.001f);
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
            animator.SetFloat(MoveX, lastMoveDir.x);
            animator.SetFloat(MoveY, lastMoveDir.y);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
