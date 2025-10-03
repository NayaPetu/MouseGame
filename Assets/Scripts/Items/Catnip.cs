using UnityEngine;

public class Catnip : MonoBehaviour, IUsable
{
    public float effectDuration = 5f;

    public void Use(PlayerController player)
    {
        // ������� ������ �� �����
        transform.SetParent(null);
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        Debug.Log("������� ���� ���������!");
    }
}
