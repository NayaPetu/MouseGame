using UnityEngine;
using System.Collections.Generic;



[System.Serializable]
public class Room {
public RectInt rect;
public int themeId;
public Vector2Int center => new Vector2Int(rect.x + rect.width/2, rect.y + rect.height/2);


public Room(RectInt r, int theme) { rect = r; themeId = theme; }
}


public class MapData {
public int width, height;
public bool[,] walkable;
public List<Room> rooms = new List<Room>();


public MapData(int w, int h) { width = w; height = h; walkable = new bool[w,h]; }
}