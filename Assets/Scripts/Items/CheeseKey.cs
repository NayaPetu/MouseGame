using UnityEngine;

public class CheeseKey : MonoBehaviour, IUsable
{
    public void Use(PlayerController player)
    {
        // ������ �������������� ������� ��� ��������������
        Debug.Log("�������� ���������� ���������!");
    }
}
