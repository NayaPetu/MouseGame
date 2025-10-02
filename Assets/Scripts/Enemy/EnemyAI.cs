using UnityEngine;
using System.Collections.Generic;

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

    // ѕоследнее направление (чтобы кот "смотрел" в сторону движени€ даже на месте)
    private Vector2 lastMoveDir;

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
            if (Vector2.Distance(rb.position, moveTarget) < 0.2f)
                ChoosePatrolTarget();
        }
        else return;

        Vector2 dir = (moveTarget - rb.position).normalized;

        // ƒвижение
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);

        // јнимаци€
        HandleAnimation(dir);
    }

    private void HandleAnimation(Vector2 dir)
    {
        bool isMoving = dir.magnitude > 0.1f;
        animator.SetBool(IsMoving, isMoving);

        if (isMoving)
        {
            animator.SetFloat(MoveX, dir.x);
            animator.SetFloat(MoveY, dir.y);
            lastMoveDir = dir;
        }
        else
        {
            // фиксируем последнее направление (дл€ idle-анимации)
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
                target = hit.transform; // нашли игрока
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

        // случайна€ проходима€ позици€
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
            // здесь можно уменьшать здоровье игрока
        }
    }
}
