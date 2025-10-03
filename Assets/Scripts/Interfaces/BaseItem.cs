using UnityEngine;

// ������� ����� ��� ���� ���������
public class BaseItem : MonoBehaviour, IInteractable, IUsable
{
    [Header("��������� ��������")]
    public string itemName = "�������";
    public bool isConsumable = false; // ���� ������� ��������� ����� (���, �����)

    protected PlayerController player;

    // ===== IInteractable =====
    public virtual void Interact(PlayerController playerController)
    {
        player = playerController;
        player.PickUpItem(gameObject);

        // ���� ������� ���������, ���������� �����
        if (isConsumable)
        {
            Use(playerController);
        }
    }

    // ===== IUsable =====
    public virtual void Use(PlayerController playerController)
    {
        Debug.Log($"{itemName} �����������!");
        if (isConsumable)
        {
            Destroy(gameObject);
        }
    }
}
