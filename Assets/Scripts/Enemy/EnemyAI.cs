using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Параметры движения")]
    public float speed = 2f;

    [Header("Зрение")]
    public float detectionRadius = 12f; // 👈 увеличен радиус обнаружения
    public LayerMask playerMask;
    public LayerMask wallMask;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform target;
    private bool[,] walkable;
    private Vector2Int patrolTarget;
    private bool hasPatrolTarget = false;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private Vector2 lastMoveDir;
    private Vector2 currentDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        animator = GetComponent<Animator>();
    }

    public void Init(bool[,] mapWalkable)
    {
        walkable = mapWalkable;
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
            moveTarget = new Vector2(patrolTarget.x + 0.5f, patrolTarget.y + 0.5f);
            if (Vector2.Distance(rb.position, moveTarget) < 0.05f)
                ChoosePatrolTarget();
        }
        else
        {
            HandleAnimation(Vector2.zero, false);
            return;
        }

        Vector2 delta = moveTarget - rb.position;
        bool isMoving = delta.magnitude > 0.05f; // 👈 увеличен порог остановки

        if (isMoving)
        {
            Vector2 dir = delta.normalized;
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
            currentDir = Vector2.Lerp(currentDir, dir, 0.25f);
        }
        else
        {
            currentDir = Vector2.zero;
        }

        HandleAnimation(currentDir, isMoving);
    }

    private void HandleAnimation(Vector2 dir, bool isMoving)
    {
        animator.SetBool(IsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);
            lastMoveDir = dir;
        }
        else
        {
            Vector2 lookDir = lastMoveDir;

            // 👁 если игрок близко — поворачиваемся лицом к нему даже в idle
            if (target != null)
            {
                Vector2 toPlayer = (target.position - transform.position).normalized;
                if (toPlayer.magnitude > 0.1f)
                    lookDir = toPlayer;
            }

            // 👇 фиксируем idle-направление по 4 сторонам
            Vector2 snapped = new Vector2(
                Mathf.Abs(lookDir.x) > Mathf.Abs(lookDir.y) ? Mathf.Sign(lookDir.x) : 0,
                Mathf.Abs(lookDir.y) >= Mathf.Abs(lookDir.x) ? Mathf.Sign(lookDir.y) : 0
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
            {
                target = hit.transform;
            }
        }
        else
        {
            target = null;
        }
    }

    void ChoosePatrolTarget()
    {
        if (walkable == null) return;

        int w = walkable.GetLength(0);
        int h = walkable.GetLength(1);

        for (int attempt = 0; attempt < 100; attempt++)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            if (walkable[x, y])
            {
                patrolTarget = new Vector2Int(x, y);
                hasPatrolTarget = true;
                return;
            }
        }
    }

    // Визуализация радиуса зрения в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
