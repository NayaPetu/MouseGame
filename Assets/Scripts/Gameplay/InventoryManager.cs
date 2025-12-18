using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public string itemName;
        public Sprite icon;
    }

    public List<Item> items = new List<Item>();
    public Image[] slots; // массив Image слотов UI

    void Start()
    {
        UpdateUI();
    }

    // Добавление предмета
    public void AddItem(Item newItem)
    {
        if (items.Count < slots.Length)
        {
            items.Add(newItem);
            UpdateUI();
        }
    }

    // Использование предмета
    public void UseItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            // Логика эффекта предмета
            Debug.Log("Использован " + items[index].itemName);

            // Удаляем из инвентаря
            items.RemoveAt(index);
            UpdateUI();
        }
    }

    // Обновление UI
    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count)
            {
                slots[i].gameObject.SetActive(true); // включаем слот
                slots[i].sprite = items[i].icon;     // ставим иконку предмета
                slots[i].color = Color.white;
            }
            else
            {
                slots[i].gameObject.SetActive(false); // выключаем пустой слот
            }
        }
    }
}
