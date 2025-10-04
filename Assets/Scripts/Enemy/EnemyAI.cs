using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRadius = 5f;
    public LayerMask playerMask, wallMask;
    public float attackDistance = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform target;
    private bool[,] walkable;
    private Vector2Int patrolTarget;
    private bool hasPatrolTarget = false;

    // ’эши параметров аниматора
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    // ѕоследнее направление (дл€ idle)
    private Vector2 lastMoveDir;
    private Vector2 currentDir; // сглаженное направление

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        if (target != null) // преследуем игрока
        {
            moveTarget = target.position;
        }
        else if (hasPatrolTarget) // патрулируем комнату
        {
            moveTarget = new Vector2(patrolTarget.x + 0.5f, patrolTarget.y + 0.5f);
            if (Vector2.Distance(rb.position, moveTarget) < 0.05f)
                ChoosePatrolTarget();
        }
        else
        {
            // нет цели Ч стоим на месте
            HandleAnimation(Vector2.zero, false);
            return;
        }

        Vector2 delta = moveTarget - rb.position;
        bool isMoving = delta.magnitude > 0.001f;

        if (isMoving)
        {
            // если близко к цели, фиксируем позицию и считаем, что остановились
            if (delta.magnitude <= speed * Time.fixedDeltaTime)
            {
                rb.position = moveTarget;
                isMoving = false;
            }
            else
            {
                Vector2 dir = delta.normalized;
                rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
                // сглаживаем направление дл€ анимации
                currentDir = Vector2.Lerp(currentDir, dir, 0.2f);
            }
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
            animator.SetFloat(MoveX, lastMoveDir.x);
            animator.SetFloat(MoveY, lastMoveDir.y);
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

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log(" от атакует игрока!");
        }
    }
}
