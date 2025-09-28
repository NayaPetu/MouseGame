using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class BSPGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 64;
    public int mapHeight = 64;

    [Header("Leaf Settings")]
    public int minLeafSize = 12;
    public int maxLeafSize = 20;

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase arcTile;

    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Enemy")]
    public GameObject enemyCatPrefab;

    [Header("Camera")]
    public CinemachineCamera virtualCamera;

    private System.Random rng;
    private List<Leaf> leaves = new List<Leaf>();
    private GameObject playerInstance;

    void Start()
    {
        rng = new System.Random();
        Generate();
    }

    public void Generate()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        leaves.Clear();

        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leaves.Add(root);

        SplitLeaves(root);
        root.CreateRooms(rng);

        foreach (Leaf l in leaves)
            if (l.room != RectInt.zero)
                DrawRoom(l.room);

        root.CreateArcs(this);
        BuildWalls();
        HideAllRooms();

        SpawnPlayer();
        StartCoroutine(UpdateVisibleRooms());
    }

    private void SplitLeaves(Leaf root)
    {
        bool didSplit = true;
        while (didSplit)
        {
            didSplit = false;
            List<Leaf> newLeaves = new List<Leaf>();
            foreach (Leaf l in leaves)
            {
                if (l.left == null && l.right == null)
                {
                    if (l.width > maxLeafSize || l.height > maxLeafSize || rng.NextDouble() > 0.8)
                    {
                        if (l.Split(minLeafSize, rng))
                        {
                            newLeaves.Add(l.left);
                            newLeaves.Add(l.right);
                            didSplit = true;
                        }
                    }
                }
            }
            leaves.AddRange(newLeaves);
        }
    }

    private void DrawRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
    }

    public void BuildWalls()
    {
        BoundsInt bounds = floorTilemap.cellBounds;
        for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
        {
            for (int y = bounds.yMin - 1; y <= bounds.yMax + 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(pos) != null) continue; // пол или арка

                Vector3Int[] neighbors = {
                    new Vector3Int(x+1, y, 0),
                    new Vector3Int(x-1, y, 0),
                    new Vector3Int(x, y+1, 0),
                    new Vector3Int(x, y-1, 0)
                };

                foreach (var n in neighbors)
                {
                    TileBase t = floorTilemap.GetTile(n);
                    if (t != null) 
                    {
                        wallTilemap.SetTile(pos, wallTile);
                        wallTilemap.SetColor(pos, Color.white);
                        break;
                    }
                }
            }
        }
    }

    private void HideAllRooms()
    {
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(pos) != null)
                    floorTilemap.SetColor(pos, Color.clear);
                if (wallTilemap.GetTile(pos) != null)
                    wallTilemap.SetColor(pos, Color.clear);
            }
    }

    private void SpawnPlayer()
    {
        Leaf startLeaf = FindTopCenterLeaf();
        Vector3 spawnPos = new Vector3(
            startLeaf.room.x + startLeaf.room.width / 2f,
            startLeaf.room.y + startLeaf.room.height / 2f,
            0
        );
        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        if (virtualCamera != null)
            virtualCamera.Follow = playerInstance.transform;

        RevealRoom(startLeaf.room);
        SpawnEnemyInSameRoom();
    }

    private void SpawnEnemyInSameRoom()
    {
        if (enemyCatPrefab == null || playerInstance == null) return;
        Leaf room = FindLeafContainingPosition(playerInstance.transform.position);
        Vector3 enemyPos = new Vector3(
            room.room.x + room.room.width / 2f,
            room.room.y + room.room.height / 2f,
            0
        );
        GameObject enemy = Instantiate(enemyCatPrefab, enemyPos, Quaternion.identity);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
            ai.Init(GenerateWalkableMap());
    }

    private Leaf FindLeafContainingPosition(Vector3 pos)
    {
        foreach (Leaf leaf in leaves)
        {
            if (leaf.room == RectInt.zero) continue;
            Rect r = new Rect(leaf.room.x, leaf.room.y, leaf.room.width, leaf.room.height);
            if (r.Contains(new Vector2(pos.x, pos.y))) return leaf;
        }
        return null;
    }

    private bool[,] GenerateWalkableMap()
    {
        bool[,] map = new bool[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase t = floorTilemap.GetTile(pos);
                map[x, y] = t == floorTile || t == arcTile; // арки тоже проходимы
            }
        return map;
    }

    private Leaf FindTopCenterLeaf()
    {
        Vector2 topCenter = new Vector2(mapWidth / 2f, mapHeight - 2);
        Leaf closest = null;
        float minDist = float.MaxValue;
        foreach (Leaf l in leaves)
        {
            if (l.room == RectInt.zero) continue;
            Vector2 roomCenter = new Vector2(l.room.x + l.room.width / 2f, l.room.y + l.room.height / 2f);
            float dist = Vector2.Distance(topCenter, roomCenter);
            if (dist < minDist)
            {
                minDist = dist;
                closest = l;
            }
        }
        return closest;
    }

    private void RevealRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(pos) != null) floorTilemap.SetColor(pos, Color.white);
                if (wallTilemap.GetTile(pos) != null) wallTilemap.SetColor(pos, Color.white);
            }
    }

    public void CreateArc(Vector2Int pos)
    {
        floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), arcTile);
        floorTilemap.SetColor(new Vector3Int(pos.x, pos.y, 0), Color.white);
        wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
    }

    private IEnumerator UpdateVisibleRooms()
    {
        while (true)
        {
            Vector2 playerPos = playerInstance.transform.position;
            foreach (Leaf leaf in leaves)
            {
                if (leaf.room == RectInt.zero) continue;
                Rect roomRect = new Rect(leaf.room.x, leaf.room.y, leaf.room.width, leaf.room.height);
                if (roomRect.Contains(playerPos))
                {
                    RevealRoom(leaf.room);
                    foreach (Vector2Int arc in leaf.arcs)
                        CreateArc(arc);
                }
            }
            yield return null;
        }
    }
}

// ===== Leaf =====
public class Leaf
{
    public int x, y, width, height;
    public Leaf left, right;
    public RectInt room;
    public List<Vector2Int> arcs = new List<Vector2Int>();

    public Leaf(int x, int y, int w, int h)
    {
        this.x = x; this.y = y; this.width = w; this.height = h;
    }

    public bool Split(int minSize, System.Random rng)
    {
        bool splitH = rng.NextDouble() > 0.5;
        if (width > height && (float)width / height >= 1.25f) splitH = false;
        else if (height > width && (float)height / width >= 1.25f) splitH = true;

        int max = (splitH ? height : width) - minSize;
        if (max <= minSize) return false;

        int split = rng.Next(minSize, max);
        if (splitH)
        {
            left = new Leaf(x, y, width, split);
            right = new Leaf(x, y + split, width, height - split);
        }
        else
        {
            left = new Leaf(x, y, split, height);
            right = new Leaf(x + split, y, width - split, height);
        }
        return true;
    }

    public void CreateRooms(System.Random rng)
    {
        if (left != null || right != null)
        {
            left?.CreateRooms(rng);
            right?.CreateRooms(rng);
        }
        else
        {
            int rw = rng.Next(6, width - 2);
            int rh = rng.Next(6, height - 2);
            int rx = rng.Next(x + 1, x + width - rw - 1);
            int ry = rng.Next(y + 1, y + height - rh - 1);
            room = new RectInt(rx, ry, rw, rh);
        }
    }

    public void CreateArcs(BSPGenerator gen)
    {
        if (left != null && right != null)
        {
            Vector2Int c1 = left.GetRoomCenter();
            Vector2Int c2 = right.GetRoomCenter();

            Vector2Int arcPos;
            if (c1.x != c2.x)
                arcPos = new Vector2Int((c1.x + c2.x) / 2, Random.Range(
                    Mathf.Max(left.room.y, right.room.y),
                    Mathf.Min(left.room.y + left.room.height - 1, right.room.y + right.room.height - 1)
                ));
            else
                arcPos = new Vector2Int(Random.Range(
                    Mathf.Max(left.room.x, right.room.x),
                    Mathf.Min(left.room.x + left.room.width - 1, right.room.x + right.room.width - 1)
                ), (c1.y + c2.y) / 2);

            arcs.Add(arcPos);
            gen.CreateArc(arcPos);

            left.CreateArcs(gen);
            right.CreateArcs(gen);
        }
    }

    public Vector2Int GetRoomCenter()
    {
        if (room == RectInt.zero)
            return new Vector2Int(x + width / 2, y + height / 2);
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }
}
