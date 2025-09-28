using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public TileBase[] floorTiles;
    public TileBase wallTile;

    [Header("Characters")]
    public GameObject playerPrefab; // только игрок

    public List<Leaf> leaves = new List<Leaf>();
    private System.Random rng;

    void Start()
    {
        rng = new System.Random();
        Generate();
    }

    public List<Leaf> GetRoomLeaves()
    {
        List<Leaf> rooms = new List<Leaf>();
        foreach (Leaf l in leaves)
        {
            if (l.room != RectInt.zero)
                rooms.Add(l);
        }
        return rooms;
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
        {
            if (l.room != RectInt.zero)
                DrawRoom(l.room);
        }

        root.CreateDoors(this);
        BuildWalls();

        // —павн игрока в верхней центральной комнате
        SpawnPlayerAtTopCenterRoom();
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
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTiles[rng.Next(floorTiles.Length)]);
    }

    public void DrawDoor(Vector2Int pos)
    {
        floorTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), floorTiles[rng.Next(floorTiles.Length)]);
        wallTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
    }

    private void BuildWalls()
    {
        BoundsInt bounds = floorTilemap.cellBounds;
        for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
            for (int y = bounds.yMin - 1; y <= bounds.yMax + 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(pos) != null)
                {
                    Vector3Int[] neighbors = {
                        new Vector3Int(x+1, y, 0),
                        new Vector3Int(x-1, y, 0),
                        new Vector3Int(x, y+1, 0),
                        new Vector3Int(x, y-1, 0)
                    };
                    foreach (var n in neighbors)
                        if (floorTilemap.GetTile(n) == null)
                            wallTilemap.SetTile(n, wallTile);
                }
            }

        for (int x = -1; x <= mapWidth; x++)
        {
            wallTilemap.SetTile(new Vector3Int(x, -1, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(x, mapHeight, 0), wallTile);
        }
        for (int y = -1; y <= mapHeight; y++)
        {
            wallTilemap.SetTile(new Vector3Int(-1, y, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(mapWidth, y, 0), wallTile);
        }

        wallTilemap.RefreshAllTiles();
        StartCoroutine(UpdateColliderNextFrame());
    }

    private IEnumerator UpdateColliderNextFrame()
    {
        yield return null;
        var tilemapCollider = wallTilemap.GetComponent<TilemapCollider2D>();
        var composite = wallTilemap.GetComponent<CompositeCollider2D>();
        var rb = wallTilemap.GetComponent<Rigidbody2D>();

        tilemapCollider.compositeOperation = TilemapCollider2D.CompositeOperation.Merge;
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        rb.bodyType = RigidbodyType2D.Static;

        tilemapCollider.enabled = false;
        tilemapCollider.enabled = true;
        composite.enabled = false;
        composite.enabled = true;
    }

    private void SpawnPlayerAtTopCenterRoom()
    {
        Leaf topCenterRoom = FindTopCenterRoom();
        if (topCenterRoom == null) return;

        Vector3 spawnPos = new Vector3(
            topCenterRoom.room.x + topCenterRoom.room.width / 2f,
            topCenterRoom.room.y + topCenterRoom.room.height / 2f,
            0
        );

        Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // ѕоказываем только эту комнату
        RevealRoom(topCenterRoom.room);
    }

    private Leaf FindTopCenterRoom()
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

    public void RevealRoom(RectInt room)
    {
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                floorTilemap.SetColor(new Vector3Int(x, y, 0), Color.clear);
                wallTilemap.SetColor(new Vector3Int(x, y, 0), Color.clear);
            }

        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTilemap.GetTile(pos) != null) floorTilemap.SetColor(pos, Color.white);
                if (wallTilemap.GetTile(pos) != null) wallTilemap.SetColor(pos, Color.white);
            }
    }
}

// ===== CLASS LEAF =====
public class Leaf
{
    public int x, y, width, height;
    public Leaf left, right;
    public RectInt room;

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
            int rw = rng.Next(6, Mathf.Min(12, width - 2));
            int rh = rng.Next(6, Mathf.Min(12, height - 2));
            int rx = rng.Next(x + 1, x + width - rw - 1);
            int ry = rng.Next(y + 1, y + height - rh - 1);
            room = new RectInt(rx, ry, rw, rh);
        }
    }

    public void CreateDoors(BSPGenerator gen)
    {
        if (left != null && right != null)
        {
            Vector2Int c1 = left.GetRoomCenter();
            Vector2Int c2 = right.GetRoomCenter();

            if (c1.x != c2.x)
            {
                int doorX = (c1.x + c2.x) / 2;
                int doorY = c1.y;
                gen.DrawDoor(new Vector2Int(doorX, doorY));
            }
            else
            {
                int doorY = (c1.y + c2.y) / 2;
                int doorX = c1.x;
                gen.DrawDoor(new Vector2Int(doorX, doorY));
            }

            left.CreateDoors(gen);
            right.CreateDoors(gen);
        }
    }

    public Vector2Int GetRoomCenter()
    {
        if (room == RectInt.zero)
            return new Vector2Int(x + width / 2, y + height / 2);
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }
}
