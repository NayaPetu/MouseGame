using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private bool hasKey = false;
    private bool hasRecipe = false;
    private bool hasÑatnip = false;

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
    public void PickRecipe()
    {
        hasRecipe = true;
        Debug.Log("Ğåöåïò äîáàâëåí â èíâåíòàğü!");
    }

    public void UseRecipe() => hasRecipe = false;
    public bool HasRecipe() => hasRecipe;

    // ===== ÌßÒÀ =====
    public void PickCatnip()
    {
        hasÑatnip = true;
        Debug.Log("Ìÿòà ïîäîáğàíà!");
    }

    public bool HasCatnip() => hasÑatnip;
    public void UseCatnip() => hasÑatnip = false;
}
