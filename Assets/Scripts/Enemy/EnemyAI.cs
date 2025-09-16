using UnityEngine;
using System.Collections.Generic;



[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {
public float speed = 2f;
public float detectionRadius = 5f;
public LayerMask playerMask, wallMask;


Rigidbody2D rb;
Transform target;
List<Vector2Int> path;
int pathIndex;
bool[,] walkable;


void Awake() { rb = GetComponent<Rigidbody2D>(); }


public void Init(bool[,] mapWalkable) { walkable = mapWalkable; }


void Update() {
if (target == null) FindTarget();
if (target != null) UpdatePath();
}


void FixedUpdate() {
if (path != null && pathIndex < path.Count) {
Vector2 targetPos = new Vector2(path[pathIndex].x+0.5f, path[pathIndex].y+0.5f);
Vector2 dir = (targetPos - rb.position).normalized;
rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);


if (Vector2.Distance(rb.position, targetPos) < 0.1f) pathIndex++;
}
}


void FindTarget() {
Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);
if (hit != null) {
Vector2 dir = (hit.transform.position - transform.position).normalized;
if (!Physics2D.Raycast(transform.position, dir, detectionRadius, wallMask)) {
target = hit.transform;
}
}
}


void UpdatePath() {
Vector2Int start = new Vector2Int(Mathf.RoundToInt(rb.position.x), Mathf.RoundToInt(rb.position.y));
Vector2Int goal = new Vector2Int(Mathf.RoundToInt(target.position.x), Mathf.RoundToInt(target.position.y));
path = AStar.FindPath(walkable, start, goal);
pathIndex = 0;
}
}