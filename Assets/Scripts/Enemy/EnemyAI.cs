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

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

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

        // Проверяем, что цель действительно является дверью, через которую игрок только что прошёл
        bool hasDoorInfo = Door.LastPlayerDoor != null;
        bool isDoorTarget = hasDoorInfo &&
                            Vector2.Distance(moveTarget, (Vector2)Door.LastPlayerDoor.transform.position) < 0.05f;

        // Если цель — дверь и мы уже достаточно близко, "насильно" проталкиваем кота через дверь,
        // чтобы он не застревал на коллайдерах и триггерах
        if (isDoorTarget && distanceToTarget < 0.2f)
        {
            Door door = Door.LastPlayerDoor;
            if (door != null && door.targetDoor != null)
            {
                Vector3 targetPos = door.targetDoor.position + door.safeOffset;

                // Жёстко телепортируем кота к целевой двери на другой стороне
                rb.position = targetPos;
                transform.position = targetPos;

                // Сбрасываем информацию о двери — дальше кот снова просто гонится за игроком
                Door.LastPlayerDoor = null;
                hasSeenPlayer = true;
            }

            waitTimer = 0f;
            HandleAnimation(Vector2.zero, false);
            return;
        }

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

        // Когда идём к двери за игроком, разрешаем "подходить вплотную",
        // чтобы луч не блокировал движение возле проёма.
        if (!isDoorTarget)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
            if (wallHit.collider != null) dir = Vector2.zero;
        }

        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        currentDir = Vector2.Lerp(currentDir, dir, 0.15f);
        HandleAnimation(currentDir, dir.sqrMagnitude > 0.001f);
    }

    // ------------------ PLAYER DETECTION -----------------------
    private void DetectPlayer()
    {
        if (player == null) return;

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
        if (player == null || isResting) return;

        float catchDistance = 0.5f;
        if (Vector2.Distance(transform.position, player.position) <= catchDistance)
        {
            GameManager.Instance.OnPlayerCaught();
        }
    }

    // ------------------ PATROL HELPERS -----------------------
    private void ChoosePatrolTarget()
    {
        if (player == null) return;
        patrolTarget = (Vector2)player.position + Random.insideUnitCircle * 2f;
        hasPatrolTarget = true;
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
