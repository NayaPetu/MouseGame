using UnityEngine;

public class Recipe : MonoBehaviour, IUsable
{
    public Sprite icon;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Добавляем рецепт в инвентарь
        InventoryManager.Instance.AddItem(this);
        gameObject.SetActive(false);
        Debug.Log("Рецепт подобран и добавлен в инвентарь!");
    }

    public void Use(PlayerController player)
    {
        CheeseFactory[] factories = FindObjectsByType<CheeseFactory>(FindObjectsSortMode.None);
        foreach (var factory in factories)
        {
            if (Vector3.Distance(player.transform.position, factory.transform.position) < 1.5f)
            {
                factory.ProduceCheese();
                Debug.Log("Рецепт использован на машине сыра!");
                InventoryManager.Instance.RemoveItem(this);
                Destroy(gameObject);
                return;
            }
        }
    }
}
