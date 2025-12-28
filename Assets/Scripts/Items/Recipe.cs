using UnityEngine;

public class Recipe : BaseItem
{
    private void Awake()
    {
        itemName = "Рецепт";
        isConsumable = true;
    }

    public override void Use(PlayerController playerController)
    {
        base.Use(playerController);
        
        // Добавляем рецепт в инвентарь
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.PickRecipe();
            Debug.Log("Рецепт подобран!");
        }
        
        // Объект будет уничтожен автоматически, так как isConsumable = true
    }
}
