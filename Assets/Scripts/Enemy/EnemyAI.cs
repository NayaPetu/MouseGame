using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 2f;
    public float obstacleCheckDistance = 0.4f;

    [Header("Зрение")]
    public float detectionRadius = 8f;
    public LayerMask playerMask;
    public LayerMask wallMask;

    [Header("Патруль")]
    public float patrolWaitTime = 2f;
    public float minPatrolDistance = 2f;
    public float maxPatrolDistance = 6f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform target;
    private bool[,] walkable;
    private Vector2 patrolTarget;
    private bool hasPatrolTarget = false;
    private float waitTimer = 0f;

    private Door[] allDoors;
    private Vector2 lastMoveDir;
    private Vector2 currentDir;
    private bool isBlocked = false;

    // Animator hashes
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

        if (walkable == null)
        {
            walkable = new bool[10, 10];
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    walkable[x, y] = true;
        }

        ChoosePatrolTarget();
    }

    void Update()
    {
        DetectPlayer();
    }

    void FixedUpdate()
    {
        Vector2 moveTarget;

        if (target != null)
        {
            moveTarget = target.position;
        }
        else if (hasPatrolTarget)
        {
            moveTarget = patrolTarget;

            float dist = Vector2.Distance(rb.position, moveTarget);
            if (dist < 0.1f)
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
        }
        else
        {
            ChoosePatrolTarget();
            HandleAnimation(Vector2.zero, false);
            return;
        }

        Vector2 delta = moveTarget - rb.position;
        float distance = delta.magnitude;

        if (distance > 0.1f)
        {
            Vector2 dir = delta / distance;

            // Проверяем, не стена ли впереди
            RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
            if (wallHit.collider)
            {
                isBlocked = true;
                // Небольшой обход в сторону
                dir = new Vector2(dir.y, -dir.x); // поворот на 90°
            }
            else
            {
                isBlocked = false;
            }

            Vector2 nextPos = rb.position + dir * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
            currentDir = Vector2.Lerp(currentDir, dir, 0.15f);
            HandleAnimation(currentDir, true);

            Debug.DrawLine(transform.position, moveTarget, target ? Color.red : Color.green);
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

    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);
        if (hit != null)
        {
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            if (!Physics2D.Raycast(transform.position, dir, detectionRadius, wallMask))
                target = hit.transform;
        }
        else
        {
            target = null;
        }
    }

    void ChoosePatrolTarget()
    {
        if (walkable == null)
        {
            patrolTarget = rb.position + Random.insideUnitCircle * Random.Range(minPatrolDistance, maxPatrolDistance);
            hasPatrolTarget = true;
            return;
        }

        int w = walkable.GetLength(0);
        int h = walkable.GetLength(1);

        for (int attempt = 0; attempt < 50; attempt++)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            if (walkable[x, y])
            {
                patrolTarget = TileToWorld(new Vector2Int(x, y));
                hasPatrolTarget = true;
                return;
            }
        }

        patrolTarget = rb.position + Random.insideUnitCircle * Random.Range(minPatrolDistance, maxPatrolDistance);
        hasPatrolTarget = true;
    }

    Vector2 TileToWorld(Vector2Int tile)
    {
        return new Vector2(tile.x + 0.5f, tile.y + 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(patrolTarget, 0.2f);
    }
}
