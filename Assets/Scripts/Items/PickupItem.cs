using UnityEngine;

public class PickupItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.AddItem(gameObject))
        {
            gameObject.SetActive(false);
        }
    }
}
