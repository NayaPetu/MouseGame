using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryManager.Instance?.AddItem(this.gameObject);
        Debug.Log("Ключ подобран!");
        gameObject.SetActive(false);
    }
}
