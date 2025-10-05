using UnityEngine;
using Unity.Cinemachine;


public class Door : MonoBehaviour
{
    public Transform targetDoor; // �����, ���� ��������� �����
    public Collider2D roomCollider; // ��������� �������, ��� ������

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ���������� ������ � ������� �����
            other.transform.position = targetDoor.position;
            
            // ����������� ������ �� ����� �������
            Camera.main.GetComponent<CinemachineConfiner2D>().BoundingShape2D = roomCollider;
        }
    }
}
