using UnityEngine;

using System.Collections.Generic;


public class AStar {
public static List<Vector2Int> FindPath(bool[,] walkable, Vector2Int start, Vector2Int goal) {
var openSet = new PriorityQueue<Vector2Int>();
var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
var gScore = new Dictionary<Vector2Int, int>();
var fScore = new Dictionary<Vector2Int, int>();


openSet.Enqueue(start, 0);
gScore[start] = 0;
fScore[start] = Heuristic(start, goal);


while (openSet.Count > 0) {
var current = openSet.Dequeue();
if (current == goal) return ReconstructPath(cameFrom, current);


foreach (var neighbor in GetNeighbors(current, walkable)) {
int tentativeG = gScore[current] + 1;
if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor]) {
cameFrom[neighbor] = current;
gScore[neighbor] = tentativeG;
fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
if (!openSet.Contains(neighbor)) openSet.Enqueue(neighbor, fScore[neighbor]);
}
}
}
return null;
}


static int Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);


static List<Vector2Int> GetNeighbors(Vector2Int pos, bool[,] walkable) {
var dirs = new Vector2Int[]{ Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
var result = new List<Vector2Int>();
foreach (var d in dirs) {
var np = pos + d;
if (np.x >= 0 && np.y >= 0 && np.x < walkable.GetLength(0) && np.y < walkable.GetLength(1) && walkable[np.x,np.y])
result.Add(np);
}
return result;
}


static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
var path = new List<Vector2Int> { current };
while (cameFrom.ContainsKey(current)) {
current = cameFrom[current];
path.Insert(0, current);
}
return path;
}
}


public class PriorityQueue<T> {
private List<(T item, int priority)> elements = new();
public int Count => elements.Count;


public void Enqueue(T item, int priority) {
elements.Add((item, priority));
}


public T Dequeue() {
int bestIndex = 0;
for (int i = 0; i < elements.Count; i++)
if (elements[i].priority < elements[bestIndex].priority)
bestIndex = i;
var item = elements[bestIndex].item;
elements.RemoveAt(bestIndex);
return item;
}


public bool Contains(T item) => elements.Exists(e => EqualityComparer<T>.Default.Equals(e.item, item));
}