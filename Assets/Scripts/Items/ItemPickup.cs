using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(this.gameObject);
        }

        gameObject.SetActive(false);
        Debug.Log($"{gameObject.name} подобран!");
    }
}
