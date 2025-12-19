using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Image slot1;
    public Image slot2;
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    private void Awake()
    {
        Instance = this;
    }

    public void Refresh(List<GameObject> items, int selectedIndex)
    {
        slot1.enabled = slot2.enabled = false;

        if (items.Count > 0 && items[0] != null)
        {
            slot1.enabled = true;
            slot1.sprite = items[0].GetComponent<SpriteRenderer>().sprite;
            slot1.color = (selectedIndex == 0) ? selectedColor : normalColor;
        }

        if (items.Count > 1 && items[1] != null)
        {
            slot2.enabled = true;
            slot2.sprite = items[1].GetComponent<SpriteRenderer>().sprite;
            slot2.color = (selectedIndex == 1) ? selectedColor : normalColor;
        }
    }
}
