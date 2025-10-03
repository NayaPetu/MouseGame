using UnityEngine;

public class ShieldButton : MonoBehaviour, IUsable
{
    public int durability = 2; // ����������� 2 ����� ����

    public void Use(PlayerController player)
    {
        // ���� ������� ������ ��������, ������������� ������� �� �����
        Debug.Log("���-������� �������. ������ �� ����!");
    }

    public bool AbsorbHit()
    {
        durability--;
        if (durability <= 0)
        {
            Destroy(gameObject);
            return false; // ������ �� ��������
        }
        return true;
    }
}
