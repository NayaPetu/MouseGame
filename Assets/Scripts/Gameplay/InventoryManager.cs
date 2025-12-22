using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [System.Serializable]
    public class ItemSlot
    {
        public BaseItem item;
        public Image slotImage;
    }

    public List<ItemSlot> slots = new List<ItemSlot>(); // слоты UI

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AddItem(BaseItem item)
    {
        // ищем первый пустой слот
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                if (slot.slotImage != null)
                {
                    slot.slotImage.sprite = item.icon;
                    slot.slotImage.enabled = true;
                }
                return;
            }
        }
        Debug.LogWarning("Инвентарь полон!");
    }

    public void RemoveItem(BaseItem item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.item = null;
                if (slot.slotImage != null)
                    slot.slotImage.enabled = false;
                return;
            }
        }
    }
}
