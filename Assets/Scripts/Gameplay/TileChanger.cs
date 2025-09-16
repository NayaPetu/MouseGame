using UnityEngine;
using UnityEngine.Tilemaps;

// Простой компонент, который меняет тайл в указанной Tilemap на newTile
public class TileChanger : MonoBehaviour
{
    public Tilemap targetTilemap; // назначить в инспекторе
    public TileBase newTile;      // тайл, которым заменять

    // worldPoint — место куда кликнули (в мировых координатах)
    public void ChangeTile(Vector3 worldPoint)
    {
        if (targetTilemap == null || newTile == null) return;
        Vector3Int cell = targetTilemap.WorldToCell(worldPoint);
        targetTilemap.SetTile(cell, newTile);
    }
}
