using UnityEngine;

public class Key : MonoBehaviour, IUsable
{
    public Sprite icon;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Добавляем ключ в инвентарь
        InventoryManager.Instance.AddItem(this);
        gameObject.SetActive(false); // скрываем с карты
        Debug.Log("Ключ подобран и добавлен в инвентарь!");
    }

    public void Use(PlayerController player)
    {
        // Находим двери рядом
        Door[] doors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        foreach (var door in doors)
        {
            if (Vector3.Distance(player.transform.position, door.transform.position) < 1.5f)
            {
                if (door.lockedDoor != null && !door.lockedDoor.IsOpen)
                {
                    door.UnlockDoor();
                    Debug.Log("Ключ использован для открытия двери!");
                    InventoryManager.Instance.RemoveItem(this);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
