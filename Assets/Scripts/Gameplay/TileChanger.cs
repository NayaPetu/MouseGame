using UnityEngine;
using UnityEngine.Tilemaps;

// ������� ���������, ������� ������ ���� � ��������� Tilemap �� newTile
public class TileChanger : MonoBehaviour
{
    public Tilemap targetTilemap; // ��������� � ����������
    public TileBase newTile;      // ����, ������� ��������

    // worldPoint � ����� ���� �������� (� ������� �����������)
    public void ChangeTile(Vector3 worldPoint)
    {
        if (targetTilemap == null || newTile == null) return;
        Vector3Int cell = targetTilemap.WorldToCell(worldPoint);
        targetTilemap.SetTile(cell, newTile);
    }
}
