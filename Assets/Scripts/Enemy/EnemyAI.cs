using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRadius = 5f;
    public LayerMask playerMask, wallMask;
    public float attackDistance = 0.5f;

    private Rigidbody2D rb;
    private Transform target;
    private bool[,] walkable;
    private Vector2Int patrolTarget;
    private bool hasPatrolTarget = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
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

        // случайная проходимая позиция
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
            Debug.Log("Кот атакует игрока!");
            // здесь можно уменьшать здоровье игрока
        }
    }
}
