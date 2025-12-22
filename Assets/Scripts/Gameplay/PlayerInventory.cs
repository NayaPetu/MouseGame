using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private bool hasKey = false;
    private bool hasRecipe = false;
    private bool hasCatnip = false;

    private int selectedIndex = 0;
    private BaseItem[] inventorySlots = new BaseItem[2]; // всего 2 слота

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ------------------- Методы для ключа -------------------
    public void PickKey()
    {
        hasKey = true;
        Debug.Log("Ключ подобран!");
    }

    public bool HasKey() => hasKey;

    public void UseKey()
    {
        if (hasKey)
        {
            hasKey = false;
            Debug.Log("Ключ использован!");
        }
    }

    // ------------------- Методы для рецепта -------------------
    public void PickRecipe()
    {
        hasRecipe = true;
        Debug.Log("Рецепт подобран!");
    }

    public bool HasRecipe() => hasRecipe;

    public void UseRecipe()
    {
        if (hasRecipe)
        {
            hasRecipe = false;
            Debug.Log("Рецепт использован!");
        }
    }

    // ------------------- Методы для мяты -------------------
    public void PickCatnip()
    {
        hasCatnip = true;
        Debug.Log("Мята подобрана!");
    }

    public bool HasCatnip() => hasCatnip;

    public void UseCatnip()
    {
        if (hasCatnip)
        {
            hasCatnip = false;
            Debug.Log("Мята использована!");
        }
    }

    // ------------------- Инвентарь слотов -------------------
    public void AddItem(BaseItem item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = item;
                selectedIndex = 0; // первый слот выбран автоматически
                return;
            }
        }
        Debug.Log("Инвентарь заполнен!");
    }

    public void NextItem()
    {
        if (inventorySlots.Length == 0) return;
        selectedIndex = (selectedIndex + 1) % inventorySlots.Length;
    }

    public void PreviousItem()
    {
        if (inventorySlots.Length == 0) return;
        selectedIndex--;
        if (selectedIndex < 0) selectedIndex = inventorySlots.Length - 1;
    }

    public void UseSelectedItem(PlayerController player)
    {
        BaseItem item = inventorySlots[selectedIndex];
        if (item != null)
        {
            item.Use(player);
            if (item.isConsumable)
                inventorySlots[selectedIndex] = null;
        }
    }
}
