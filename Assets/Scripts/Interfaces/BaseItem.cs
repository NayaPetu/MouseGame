using UnityEngine;

// Базовый класс для всех предметов
public class BaseItem : MonoBehaviour, IInteractable, IUsable
{
    [Header("Настройки предмета")]
    public string itemName = "Предмет";
    public bool isConsumable = false; // если предмет съедается сразу (сыр, зелья)

    protected PlayerController player;

    // ===== IInteractable =====
    public virtual void Interact(PlayerController playerController)
    {
        player = playerController;
        player.PickUpItem(gameObject);

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
