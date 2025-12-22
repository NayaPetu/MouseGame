using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image keySlot;
    public Image recipeSlot;
    public Image catnipSlot;

    void Update()
    {
        keySlot.gameObject.SetActive(PlayerInventory.Instance.HasKey());
        recipeSlot.gameObject.SetActive(PlayerInventory.Instance.HasRecipe());
        catnipSlot.gameObject.SetActive(PlayerInventory.Instance.HasCatnip());
    }
}
