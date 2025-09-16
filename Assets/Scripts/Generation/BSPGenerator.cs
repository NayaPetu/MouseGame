using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int mapWidth = 64;
    public int mapHeight = 64;

    [Header("Leaf Settings")]
    public int minLeafSize = 12;
    public int maxLeafSize = 20;

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    [Header("Tiles")]
    public TileBase[] floorTiles; // варианты пола
    public TileBase wallTile;     // стена (обои)

    private List<Leaf> leaves = new List<Leaf>();
    private System.Random rng;

    void Start()
    {
        rng = new System.Random();
        Generate();
    }

    public void Generate()
    {
        // очистка
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        leaves.Clear();

        // создаём корень BSP
        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leaves.Add(root);

        // деление на листья
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

        // создаём комнаты
        root.CreateRooms(rng);

        // рисуем комнаты
        foreach (Leaf l in leaves)
        {
            if (l.room != RectInt.zero)
            {
                DrawRoom(l.room);
            }
        }

        // соединяем коридорами
        root.CreateCorridors(this);

        // ставим стены
        BuildWalls();
    }

    private void DrawRoom(RectInt room)
    {
        for (int x = room.xMin; x < room.xMax; x++)
        {
            for (int y = room.yMin; y < room.yMax; y++)
            {
                TileBase chosen = floorTiles[rng.Next(floorTiles.Length)];
                floorTilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            }
        }
    }

    public void DrawCorridor(Vector2Int from, Vector2Int to)
    {
        int x = from.x, y = from.y;
        while (x != to.x)
        {
            TileBase chosen = floorTiles[rng.Next(floorTiles.Length)];
            floorTilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            x += (to.x > x) ? 1 : -1;
        }
        while (y != to.y)
        {
            TileBase chosen = floorTiles[rng.Next(floorTiles.Length)];
            floorTilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            y += (to.y > y) ? 1 : -1;
        }
    }

    public void BuildWalls()
    {
        BoundsInt bounds = floorTilemap.cellBounds;
        for (int x = bounds.xMin - 1; x <= bounds.xMax + 1; x++)
        {
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
                    {
                        if (floorTilemap.GetTile(n) == null && wallTilemap.GetTile(n) == null)
                        {
                            wallTilemap.SetTile(n, wallTile);
                        }
                    }
                }
            }
        }
    }
}

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

        if (width > height && (float)width / height >= 1.25f)
            splitH = false;
        else if (height > width && (float)height / width >= 1.25f)
            splitH = true;

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
            int rw = rng.Next(4, width - 2);
            int rh = rng.Next(4, height - 2);
            int rx = rng.Next(x + 1, x + width - rw - 1);
            int ry = rng.Next(y + 1, y + height - rh - 1);
            room = new RectInt(rx, ry, rw, rh);
        }
    }

    public void CreateCorridors(BSPGenerator gen)
    {
        if (left != null && right != null)
        {
            Vector2Int c1 = left.GetRoomCenter();
            Vector2Int c2 = right.GetRoomCenter();
            gen.DrawCorridor(c1, c2);

            left.CreateCorridors(gen);
            right.CreateCorridors(gen);
        }
    }

    public Vector2Int GetRoomCenter()
    {
        if (room == RectInt.zero)
            return new Vector2Int(x + width / 2, y + height / 2);
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }
}
