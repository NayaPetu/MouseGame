using UnityEngine;

public class BaseItem : MonoBehaviour, IInteractable, IUsable
{
    [Header("Настройки предмета")]
    public string itemName = "Предмет";
    public Sprite icon;
    public bool isConsumable = false; // если предмет съедается сразу (сыр, зелья)

    protected PlayerController player;

    // ===== IInteractable =====
    public virtual void Interact(PlayerController playerController)
    {
        player = playerController;

        // Добавляем предмет в новый инвентарь
        InventoryManager.Item newItem = new InventoryManager.Item
        {
            itemName = itemName,
            icon = icon
        };
        InventoryManager.Instance.AddItem(newItem);

        // Если предмет съедаемый, используем сразу
        if (isConsumable)
        {
            Use(playerController);
        }
    }

    // ===== IUsable =====
    public virtual void Use(PlayerController playerController)
    {
        Debug.Log($"{itemName} использован!");
        if (isConsumable)
        {
            Destroy(gameObject);
        }
    }
}
