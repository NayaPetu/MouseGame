using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private bool hasKey;
    private bool hasRecipe;
    private bool hasÑatnip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // ===== ÊËŞ× =====
    public void PickKey() => hasKey = true;
    public bool HasKey() => hasKey;
    public void UseKey() => hasKey = false;

    // ===== ĞÅÖÅÏÒ =====
    public void PickRecipe() => hasRecipe = true;
    public bool HasRecipe() => hasRecipe;
    public void UseRecipe() => hasRecipe = false;

    // ===== ÌßÒÀ =====
    public void PickCatnip()
    {
        hasÑatnip = true;
        Debug.Log("Ìÿòà ïîäîáğàíà!");
    }

    public bool HasCatnip()
    {
        return hasÑatnip;
    }

    public void UseCatnip()
    {
        hasÑatnip = false;
    }
}
