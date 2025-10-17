﻿using UnityEngine;
using System.Collections;

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
    public LayerMask doorMask;

    [Header("Патруль")]
    public float patrolWaitTime = 2f;

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

    private bool recentlyUsedDoor = false;
    private float doorCooldown = 1.2f;

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

    public void Init(Room spawnRoom, Transform playerTransform, Vector3 spawnPos)
    {
        currentRoom = spawnRoom;
        player = playerTransform;
        transform.position = spawnPos;
        ChoosePatrolTarget();
    }

    void Update()
    {
        DetectPlayer();
    }

    void FixedUpdate()
    {
        PatrolAndChase();
    }

    private void PatrolAndChase()
    {
        if (player == null) return;

        Vector2 moveTarget;

        Vector2 playerPos2D = player.position; // убираем Z

        if (hasSeenPlayer)
        {
            Room playerRoom = player.GetComponentInParent<Room>();

            // Если игрок в другой комнате — идём к двери
            if (playerRoom != null && playerRoom != currentRoom)
            {
                Door doorToUse = FindDoorToRoom(playerRoom);
                if (doorToUse != null)
                    moveTarget = doorToUse.transform.position;
                else
                    moveTarget = GetNearestDoorPosition(); // fallback
            }
            else
            {
                moveTarget = playerPos2D;
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

        float dist = Vector2.Distance(rb.position, moveTarget);
        if (dist < 0.15f)
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

        // Проверка на двери
        RaycastHit2D doorHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, doorMask);
        if (doorHit.collider != null)
        {
            Door door = doorHit.collider.GetComponent<Door>();
            if (door != null && !recentlyUsedDoor)
            {
                StartCoroutine(PassThroughDoor(door));
                return;
            }
        }

        // Проверка на стены
        RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
        if (wallHit.collider != null)
            dir = Vector2.zero;

        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        currentDir = Vector2.Lerp(currentDir, dir, 0.15f);
        HandleAnimation(currentDir, dir.sqrMagnitude > 0.001f);
    }

    private IEnumerator PassThroughDoor(Door door)
    {
        if (door == null || recentlyUsedDoor)
            yield break;

        recentlyUsedDoor = true;

        yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

        door.TeleportEnemyToTarget(transform);

        yield return new WaitForSeconds(doorCooldown);
        recentlyUsedDoor = false;
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

    private void DetectPlayer()
    {
        if (player == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);
        if (hit != null)
        {
            Room playerRoom = hit.GetComponentInParent<Room>();
            bool sameRoom = (playerRoom == currentRoom);

            Vector2 dir = (Vector2)(hit.transform.position - transform.position);
            bool clearLine = !Physics2D.Raycast(transform.position, dir.normalized, detectionRadius, wallMask);

            if (clearLine || !sameRoom)
                hasSeenPlayer = true;
        }
    }

    private void ChoosePatrolTarget()
    {
        if (currentRoom == null) return;

        Bounds b = currentRoom.GetRoomBounds();
        patrolTarget = new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );
        hasPatrolTarget = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Room r = other.GetComponentInParent<Room>();
        if (r != null)
            currentRoom = r;
    }

    private Door FindDoorToRoom(Room targetRoom)
    {
        if (currentRoom == null || currentRoom.doors == null) return null;

        Door bestDoor = null;
        float minDist = float.MaxValue;

        foreach (Door d in currentRoom.doors)
        {
            if (d != null && d.targetDoor != null)
            {
                Door target = d.targetDoor.GetComponent<Door>();
                if (target != null && target.currentRoom == targetRoom)
                {
                    float dist = Vector2.Distance(transform.position, d.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestDoor = d;
                    }
                }
            }
        }

        return bestDoor;
    }

    private Vector2 GetNearestDoorPosition()
    {
        if (currentRoom == null || currentRoom.doors == null || currentRoom.doors.Length == 0)
            return transform.position;

        Door nearest = currentRoom.doors[0];
        float minDist = Vector2.Distance(transform.position, nearest.transform.position);

        foreach (Door d in currentRoom.doors)
        {
            if (d == null) continue;
            float dist = Vector2.Distance(transform.position, d.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = d;
            }
        }
        return nearest.transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void SetCurrentRoom(Room newRoom)
    {
        currentRoom = newRoom;
    }
}
