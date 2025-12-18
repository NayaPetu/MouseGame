using UnityEngine;
using UnityEngine.UI;

public class KeySlotUI : MonoBehaviour
{
    public Image keySlotImage; // присвоить через инспектор

    private void Update()
    {
        if (PlayerInventory.Instance.HasKey())
        {
            keySlotImage.gameObject.SetActive(true);
        }
        else
        {
            keySlotImage.gameObject.SetActive(false);
        }
    }
}
