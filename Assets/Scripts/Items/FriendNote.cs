using UnityEngine;

public class FriendNote : BaseItem
{
    [TextArea] public string noteText = "���� � �������!";

    private void Awake()
    {
        itemName = "������� �����";
        isConsumable = false;
    }

    public override void Use(PlayerController playerController)
    {
        Debug.Log($"�������: {noteText}");
        // ����� ����� �������� ����� UI ��� ����������� ������
    }
}
