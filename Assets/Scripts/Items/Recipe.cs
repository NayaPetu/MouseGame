using UnityEngine;

public class Recipe : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerInventory.Instance.PickRecipe(); // добавляем в инвентарь
        Debug.Log("Рецепт подобран!");

        Destroy(gameObject); // убираем с карты
    }
}
