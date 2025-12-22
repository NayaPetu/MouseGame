using UnityEngine;
using System.Collections;

public class Catnip : BaseItem
{
    [Header("Настройки мяты")]
    public float calmDuration = 5f;
    public GameObject attractor; // зона действия мяты
    public float pickupRange = 1.5f;

    private bool isPickedUp = false;
    private bool isUsed = false;

    private void Awake()
    {
        itemName = "Catnip";
        isConsumable = true; // можно использовать
    }
    public bool IsUsed() => isUsed;  // для врагов
public bool IsEaten() => false;  // можно пока отключить или реализовать, если кот ест мяту

public void EatenByCat() { /* если хотите логику для врага */ }




    public override void Interact(PlayerController player)
    {
        if (isPickedUp) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > pickupRange) return;

        isPickedUp = true;

        // Скрываем объект
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Добавляем в инвентарь
        InventoryManager.Instance.AddItem(this);
    }

    public override void Use(PlayerController player)
    {
        if (!isPickedUp || isUsed) return;

        isUsed = true;

        if (attractor != null)
            attractor.SetActive(true);

        // Сбрасываем рядом с игроком
        transform.position = player.transform.position + Vector3.up * 0.5f;
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        InventoryManager.Instance.RemoveItem(this); // убираем из слота после использования
    }
}
