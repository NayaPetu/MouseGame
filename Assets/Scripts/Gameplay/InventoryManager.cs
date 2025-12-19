using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public GameObject[] items = new GameObject[2];
    private int selectedIndex = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddItem(GameObject item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                return true;
            }
        }
        return false;
    }

    public GameObject GetSelectedItem()
    {
        return items[selectedIndex];
    }

    public void RemoveSelectedItem()
    {
        items[selectedIndex] = null;
    }

    public void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % items.Length;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }
}
