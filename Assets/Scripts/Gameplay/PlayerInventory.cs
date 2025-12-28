using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private bool hasKey = false;
    private bool hasRecipe = false;
    private bool hasCatnip = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Сохраняем инвентарь между сценами
    }

    // ===== Ключ =====
    public void PickKey()
    {
        hasKey = true;
        Debug.Log("[PlayerInventory] Ключ подобран!");
    }
    
    public bool HasKey() => hasKey;
    
    public void UseKey()
    {
        hasKey = false;
        Debug.Log("[PlayerInventory] Ключ использован!");
    }

    // ===== Рецепт =====
    public void PickRecipe()
    {
        hasRecipe = true;
        Debug.Log("Рецепт добавлен в инвентарь!");
    }

    public void UseRecipe() => hasRecipe = false;
    public bool HasRecipe() => hasRecipe;

    // ===== Мята =====
    public void PickCatnip()
    {
        hasCatnip = true;
        Debug.Log("Мята подобрана!");
    }

    public bool HasCatnip() => hasCatnip;
    public void UseCatnip() => hasCatnip = false;
}
