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

    [Header("Player")]
    public GameObject playerPrefab;

    private List<Leaf> leaves = new List<Leaf>();
    private System.Random rng;
    private RectInt startRoom;
    private Vector2Int startCorridorEnd;

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

        // 1. Создаём стартовую комнату
        CreateStartRoom();

        // 2. Создаём коридор вправо (3 тайла в высоту)
        CreateRightCorridorFromStart();

        // 3. BSP генерация от конца коридора
        Leaf root = new Leaf(
            startCorridorEnd.x + 1,       // начало нового листа сразу после коридора
            startCorridorEnd.y - 1,
            mapWidth,
            mapHeight
        );
        leaves.Add(root);

        SplitLeaves();
        root.CreateRooms(rng);

        foreach (Leaf l in leaves)
            if (l.room != RectInt.zero)
                DrawRoomConnectedToCorridor(l.room);

        root.CreateCorridors(this);

        // 4. Строим стены
        BuildWalls();

        // 5. Спавн игрока
        SpawnPlayer();
    }

    private void CreateStartRoom()
    {
        int roomSize = 12;
        int x = -roomSize / 2;
        int y = -roomSize / 2;
        startRoom = new RectInt(x, y, roomSize, roomSize);

        for (int i = startRoom.xMin; i < startRoom.xMax; i++)
            for (int j = startRoom.yMin; j < startRoom.yMax; j++)
                floorTilemap.SetTile(new Vector3Int(i, j, 0), floorTiles[rng.Next(floorTiles.Length)]);
    }

    private void CreateRightCorridorFromStart()
    {
        int corridorLength = Mathf.Max(3, startRoom.width / 4);
        int yCenter = (int)startRoom.center.y;

        int yMin = yCenter - 1;
        int yMax = yCenter + 1;

        startCorridorEnd = new Vector2Int(startRoom.xMax + corridorLength, yCenter);

        for (int x = startRoom.xMax; x <= startCorridorEnd.x; x++)
            for (int y = yMin; y <= yMax; y++)
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTiles[rng.Next(floorTiles.Length)]);
    }

    private void SplitLeaves()
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

    private void DrawRoomConnectedToCorridor(RectInt room)
    {
        // Сдвигаем комнату так, чтобы соединить с коридором
        int corridorX = startCorridorEnd.x;
        int corridorYCenter = startCorridorEnd.y;

        int offsetX = corridorX - room.xMin;
        room.position += new Vector2Int(offsetX, 0);

        // Рисуем пол комнаты
        for (int x = room.xMin; x < room.xMax; x++)
            for (int y = room.yMin; y < room.yMax; y++)
                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTiles[rng.Next(floorTiles.Length)]);

        // Прорезаем проход в 3 тайла к коридору
        for (int y = corridorYCenter - 1; y <= corridorYCenter + 1; y++)
            floorTilemap.SetTile(new Vector3Int(corridorX, y, 0), floorTiles[rng.Next(floorTiles.Length)]);
    }

    public void DrawCorridor(Vector2Int from, Vector2Int to)
    {
        int x = from.x;
        int y = from.y;
        int yMin = y - 1;
        int yMax = y + 1;

        while (x != to.x)
        {
            for (int yi = yMin; yi <= yMax; yi++)
                floorTilemap.SetTile(new Vector3Int(x, yi, 0), floorTiles[rng.Next(floorTiles.Length)]);
            x += (to.x > x) ? 1 : -1;
        }

        while (y != to.y)
        {
            for (int yi = y - 1; yi <= y + 1; yi++)
                floorTilemap.SetTile(new Vector3Int(x, yi, 0), floorTiles[rng.Next(floorTiles.Length)]);
            y += (to.y > y) ? 1 : -1;
        }
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

    private void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            int centerX = (int)startRoom.center.x;
            int centerY = (int)startRoom.center.y;
            Vector3 spawnPos = new Vector3(centerX + 0.5f, centerY + 0.5f, 0);
            Instantiate(playerPrefab, spawnPos, Quaternion.identity);
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
