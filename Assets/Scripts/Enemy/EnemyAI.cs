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
    private Door patrolDoorTarget = null; // Дверь для патрулирования между комнатами
    private float waitTimer = 0f;
    private bool hasSeenPlayer = false;
    private float doorStuckTimer = 0f; // Таймер для обнаружения застревания у двери
    private Vector2 lastDoorPosition = Vector2.zero; // Последняя позиция двери, к которой шли
    private float lastPatrolTargetChangeTime = 0f;
    private const float MIN_PATROL_TARGET_CHANGE_INTERVAL = 0.5f; // Минимальный интервал между сменами цели
    private int patrolPointsInCurrentRoom = 0; // Счетчик патрульных точек в текущей комнате
    private const int MAX_PATROL_POINTS_PER_ROOM = 2; // Максимум патрульных точек в комнате перед переходом в другую

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
        patrolDoorTarget = null;
        hasPatrolTarget = false;
        lastPatrolTargetChangeTime = 0f;
        patrolPointsInCurrentRoom = 0;

        ChoosePatrolTarget();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Обновляет текущую комнату врага после телепортации через дверь
    /// </summary>
    public void UpdateCurrentRoom(Room newRoom)
    {
        if (newRoom != null)
        {
            currentRoom = newRoom;
            
            // Сбрасываем цель патрулирования через дверь - враг уже прошел через нее
            patrolDoorTarget = null;
            doorStuckTimer = 0f; // Сбрасываем таймер застревания
            lastDoorPosition = Vector2.zero;
            patrolPointsInCurrentRoom = 0; // Сбрасываем счетчик при переходе в новую комнату
            
            // Если патрулировали, выбираем новую цель
            if (!hasSeenPlayer)
            {
                hasPatrolTarget = false;
                ChoosePatrolTarget();
            }
            // Если преследуем игрока, проверяем видимость в новой комнате
            else if (hasSeenPlayer)
            {
                // Если игрок виден в новой комнате - продолжаем преследование напрямую
                // Если не виден - логика в PatrolAndChase обновит цель
                hasPatrolTarget = false; // Сбрасываем патруль, чтобы начать преследование
            }
        }
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
        bool isChasingThroughDoor = false;

        if (hasSeenPlayer)
        {
            // Если есть прямой обзор на игрока — гонимся напрямую
            if (HasLineOfSightToPlayer())
            {
                moveTarget = (Vector2)player.position;
                isChasingThroughDoor = false;
            }
            // Если игрок спрятался за стеной и есть информация о двери,
            // через которую он прошёл — идём к этой двери
            else if (Door.LastPlayerDoor != null)
            {
                moveTarget = (Vector2)Door.LastPlayerDoor.transform.position;
                isChasingThroughDoor = true;
            }
            else
            {
                moveTarget = (Vector2)player.position;
                isChasingThroughDoor = false;
            }
        }
        else if (hasPatrolTarget)
        {
            // Если есть дверь для патрулирования - идем к ней, иначе к обычной точке
            if (patrolDoorTarget != null)
            {
                moveTarget = (Vector2)patrolDoorTarget.transform.position;
                isChasingThroughDoor = true; // При патрулировании через двери тоже проходим сквозь
            }
            else
            {
                moveTarget = patrolTarget;
                isChasingThroughDoor = false;
            }
        }
        else
        {
            ChoosePatrolTarget();
            return;
        }

        float distanceToTarget = Vector2.Distance(rb.position, moveTarget);

        // Если преследуем через дверь, проверяем, не находимся ли мы уже в другой комнате от цели двери
        if (isChasingThroughDoor && currentRoom != null)
        {
            Door targetDoor = null;
            if (hasSeenPlayer && Door.LastPlayerDoor != null)
            {
                targetDoor = Door.LastPlayerDoor;
            }
            else if (patrolDoorTarget != null)
            {
                targetDoor = patrolDoorTarget;
            }
            
            // Если цель двери находится в другой комнате - значит уже прошли через нее, выбираем новую цель
            if (targetDoor != null && targetDoor.currentRoom != null && targetDoor.currentRoom != currentRoom)
            {
                // Уже прошли через дверь, сбрасываем цель и выбираем новую
                patrolDoorTarget = null;
                hasPatrolTarget = false;
                doorStuckTimer = 0f;
                if (!hasSeenPlayer)
                {
                    ChoosePatrolTarget();
                }
                return;
            }
            
            // Проверка и принудительная телепортация: если очень близко к двери
            if (targetDoor != null && distanceToTarget < 0.4f)
            {
                // Проверяем, находится ли враг в триггере двери
                Collider2D doorCollider = targetDoor.GetComponent<Collider2D>();
                bool isInTrigger = false;
                if (doorCollider != null && doorCollider.isTrigger)
                {
                    isInTrigger = doorCollider.OverlapPoint(rb.position);
                }
                
                // Если враг очень близко к двери (< 0.25) или в триггере - принудительно телепортируем СРАЗУ
                if (distanceToTarget < 0.25f || isInTrigger)
                {
                    Door doorScript = targetDoor.GetComponent<Door>();
                    if (doorScript != null)
                    {
                        // Debug.Log($"[EnemyAI Door Debug] Немедленная телепортация: расстояние={distanceToTarget:F2}, в триггере={isInTrigger}!");
                        doorScript.ForceTeleportEnemy(transform);
                        return;
                    }
                }
                
                // Если позиция двери изменилась - сбрасываем таймер (новая дверь)
                if (Vector2.Distance(lastDoorPosition, moveTarget) > 0.1f)
                {
                    doorStuckTimer = 0f;
                    lastDoorPosition = moveTarget;
                }
                else
                {
                    doorStuckTimer += Time.fixedDeltaTime;
                    
                    // Отладка застревания у двери
                    if (doorStuckTimer > 0.5f && doorStuckTimer < 0.6f) // Логируем один раз при достижении 0.5 сек
                    {
                        Vector2 doorPos = (Vector2)targetDoor.transform.position;
                        Vector2 enemyPos = rb.position;
                        bool isInDoorRoom = targetDoor.currentRoom != null && targetDoor.currentRoom == currentRoom;
                        
                        // Debug.LogWarning(
                        //     $"[EnemyAI Door Debug] Застревание у двери!\n" +
                        //     $"  Расстояние до двери: {distanceToTarget:F2}\n" +
                        //     $"  Позиция врага: {enemyPos}\n" +
                        //     $"  Позиция двери: {doorPos}\n" +
                        //     $"  Враг в комнате двери: {isInDoorRoom}\n" +
                        //     $"  Враг в триггере двери: {isInTrigger}\n" +
                        //     $"  Текущая комната: {(currentRoom != null ? currentRoom.name : "null")}\n" +
                        //     $"  Комната двери: {(targetDoor.currentRoom != null ? targetDoor.currentRoom.name : "null")}\n" +
                        //     $"  targetDoor != null: {targetDoor != null}\n" +
                        //     $"  hasSeenPlayer: {hasSeenPlayer}"
                        // );
                    }
                    
                    // Если застряли более 0.3 секунды - принудительно телепортируем
                    if (doorStuckTimer > 0.3f && isInTrigger)
                    {
                        Door doorScript = targetDoor.GetComponent<Door>();
                        if (doorScript != null)
                        {
                            // Debug.LogWarning($"[EnemyAI Door Debug] Принудительная телепортация после застревания 0.3 сек!");
                            doorScript.ForceTeleportEnemy(transform);
                            return;
                        }
                    }
                    
                    // Если застряли более 1 секунды - выбираем новую цель
                    if (doorStuckTimer > 1f)
                    {
                        // Debug.LogWarning($"[EnemyAI Door Debug] Враг застрял у двери более 1 сек, выбираю новую цель");
                        patrolDoorTarget = null;
                        hasPatrolTarget = false;
                        doorStuckTimer = 0f;
                        if (!hasSeenPlayer)
                        {
                            ChoosePatrolTarget();
                        }
                        return;
                    }
                }
            }
            else
            {
                doorStuckTimer = 0f;
            }
        }
        else
        {
            doorStuckTimer = 0f;
        }

        // Если преследуем через дверь - не останавливаемся, продолжаем движение к двери
        // Для обычного патрулирования - останавливаемся при достижении цели
        if (!isChasingThroughDoor && distanceToTarget < 0.15f)
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
        bool isBlockedByWall = false;

        // Если идем к двери - НЕ проверяем стены, чтобы враг мог пройти через триггер двери
        // Триггер двери позволит врагу пройти, даже если дверь находится рядом со стеной
        if (isChasingThroughDoor)
        {
            // Полностью отключаем проверку стен при движении к двери
            // Враг должен пройти через триггер коллайдер двери
        }
        else
        {
            // При обычном патрулировании или прямом преследовании проверяем стены стандартно
            RaycastHit2D wallHit = Physics2D.Raycast(rb.position, dir, obstacleCheckDistance, wallMask);
            if (wallHit.collider != null && !wallHit.collider.isTrigger)
            {
                isBlockedByWall = true;
                // Если упираемся в стену во время патрулирования - выбираем новую цель (но не слишком часто)
                if (!hasSeenPlayer && Time.time - lastPatrolTargetChangeTime > MIN_PATROL_TARGET_CHANGE_INTERVAL)
                {
                    lastPatrolTargetChangeTime = Time.time;
                    ChoosePatrolTarget();
                    return;
                }
                // При преследовании останавливаемся
                dir = Vector2.zero;
            }
        }

        // Двигаемся к цели (к двери или игроку)
        if (!isBlockedByWall && dir.sqrMagnitude > 0.001f)
        {
            // При движении к двери используем прямое изменение позиции для более надежного прохождения через триггер
            if (isChasingThroughDoor)
            {
                // Используем прямое изменение позиции для прохождения через триггер
                Vector2 newPos = rb.position + dir * speed * Time.fixedDeltaTime;
                
                // Отладка движения к двери
                if (distanceToTarget < 0.5f && Time.frameCount % 30 == 0) // Логируем каждые 30 кадров
                {
                    Door debugDoor = hasSeenPlayer && Door.LastPlayerDoor != null 
                        ? Door.LastPlayerDoor 
                        : patrolDoorTarget;
                    if (debugDoor != null)
                    {
                        Collider2D doorCol = debugDoor.GetComponent<Collider2D>();
                        bool willBeInTrigger = doorCol != null && doorCol.isTrigger && doorCol.OverlapPoint(newPos);
                        // Debug.Log(
                        //     $"[EnemyAI Door Debug] Движение к двери:\n" +
                        //     $"  Дистанция: {distanceToTarget:F2}\n" +
                        //     $"  Текущая позиция: {rb.position}\n" +
                        //     $"  Новая позиция: {newPos}\n" +
                        //     $"  Направление: {dir}\n" +
                        //     $"  Будет в триггере: {willBeInTrigger}"
                        // );
                    }
                }
                
                rb.position = newPos;
            }
            else
            {
                rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Плавное изменение направления для анимации
        if (dir.sqrMagnitude > 0.001f)
        {
            currentDir = Vector2.Lerp(currentDir, dir, 0.2f);
            HandleAnimation(currentDir, true);
        }
        else
        {
            // Когда останавливаемся, плавно уменьшаем направление
            currentDir = Vector2.Lerp(currentDir, Vector2.zero, 0.3f);
            HandleAnimation(currentDir, currentDir.sqrMagnitude > 0.01f);
        }
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

        // Проверяем, что GameManager существует
        if (GameManager.Instance == null)
        {
            Debug.LogError("[EnemyAI] GameManager.Instance is null! Cannot trigger game over.");
            return;
        }

        float catchDistance = 0.5f;
        // Используем rb.position для более точного расчета, так как используем Rigidbody2D
        Vector2 enemyPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 playerPos = player.position;
        float distanceToPlayer = Vector2.Distance(enemyPos, playerPos);
        
        if (distanceToPlayer <= catchDistance)
        {
            // Debug.Log($"[EnemyAI] Player caught! Distance: {distanceToPlayer:F2}, EnemyPos: {enemyPos}, PlayerPos: {playerPos}");
            GameManager.Instance.OnPlayerCaught();
        }
    }

    // ------------------ PATROL HELPERS -----------------------
    private void ChoosePatrolTarget()
    {
        patrolDoorTarget = null; // Сбрасываем дверь для патрулирования
        lastPatrolTargetChangeTime = Time.time;
        
        // Проверяем, нужно ли перейти в другую комнату (после нескольких патрульных точек)
        bool shouldGoToDoor = patrolPointsInCurrentRoom >= MAX_PATROL_POINTS_PER_ROOM;
        
        // Определяем вероятность выбора двери: увеличиваем ее, если прошли несколько точек в комнате
        float doorChance = shouldGoToDoor ? 0.9f : 0.7f; // 70% базовая вероятность, 90% если прошли несколько точек
        
        // Выбираем дверь для патрулирования между комнатами
        if (currentRoom != null && currentRoom.doors != null && currentRoom.doors.Length > 0 && Random.value < doorChance)
        {
            // Выбираем случайную открытую дверь
            Door[] availableDoors = System.Array.FindAll(currentRoom.doors, door => 
                door != null && 
                (door.lockedDoor == null || door.lockedDoor.IsOpen) &&
                door.targetDoor != null
            );
            
            if (availableDoors.Length > 0)
            {
                patrolDoorTarget = availableDoors[Random.Range(0, availableDoors.Length)];
                hasPatrolTarget = true;
                waitTimer = 0f;
                patrolPointsInCurrentRoom = 0; // Сбрасываем счетчик при переходе в другую комнату
                return;
            }
        }
        
        // Патрулируем в пределах текущей комнаты
        if (currentRoom != null)
        {
            Bounds roomBounds = currentRoom.GetRoomBounds();
            // Выбираем точку подальше от краев комнаты, чтобы избежать столкновений со стенами
            float margin = 2f; // Больший отступ от краев
            patrolTarget = new Vector2(
                Random.Range(roomBounds.min.x + margin, roomBounds.max.x - margin),
                Random.Range(roomBounds.min.y + margin, roomBounds.max.y - margin)
            );
            patrolPointsInCurrentRoom++; // Увеличиваем счетчик патрульных точек в комнате
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

        PlayerController pc = player.GetComponent<PlayerController>();
        return pc != null && pc.IsPoweredUp;
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
    void OnDrawGizmos()
    {
        // Радиус обнаружения игрока
        Gizmos.color = hasSeenPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Линия до игрока
        if (player != null)
        {
            Gizmos.color = HasLineOfSightToPlayer() ? Color.green : Color.magenta;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Цель патруля / погони
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(patrolTarget, 0.1f);
    }

}