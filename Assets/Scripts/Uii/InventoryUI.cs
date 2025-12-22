using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Слоты UI")]
    public Image[] slots; // 2 слота
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    // =================== Обновление UI ===================
    public void RefreshUI(List<GameObject> items, int selectedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                BaseItem item = items[i].GetComponent<BaseItem>();
                if (item != null && item.icon != null)
                {
                    slots[i].sprite = item.icon;
                    slots[i].color = (i == selectedIndex) ? selectedColor : normalColor;
                    slots[i].gameObject.SetActive(true);
                }
                else
                {
                    slots[i].gameObject.SetActive(false);
                }
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }
}
