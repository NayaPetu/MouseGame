using UnityEngine;
using System.Collections;

public class PowerCheese : BaseItem
{
    public Vector3 sizeMultiplier = new Vector3(1.5f, 1.5f, 1f);
    public float duration = 5f;

    private void Awake()
    {
        itemName = "��� ����";
        isConsumable = true;
    }

    public override void Use(PlayerController playerController)
    {
        base.Use(playerController); // ������� Debug

        // �������� ������: ��������� ����
        playerController.StartCoroutine(ApplyEffect(playerController));
    }

    private IEnumerator ApplyEffect(PlayerController playerController)
    {
        Vector3 originalSize = playerController.transform.localScale;
        playerController.transform.localScale = originalSize + sizeMultiplier;

        yield return new WaitForSeconds(duration);

        playerController.transform.localScale = originalSize;
    }
}
