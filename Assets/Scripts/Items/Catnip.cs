using UnityEngine;
using System.Collections;

public class Catnip : MonoBehaviour, IInteractable, IUsable
{
    [Header("Настройки мяты")]
    public float calmDuration = 5f;      // сколько времени враг будет спокойным
    public GameObject attractor;         // зона действия мяты
    public float pickupRange = 1.5f;     // дистанция для подбора

    private bool isPickedUp = false;
    private bool isEaten = false;
    private bool isUsed = false;

    // ------------------ Для игрока ------------------
    public void Interact(PlayerController player)
    {
        if (isEaten) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > pickupRange) return;

        if (!isPickedUp)
        {
            isPickedUp = true;
            PlayerInventory.Instance.PickCatnip();

            // Скрываем объект визуально, отключаем физику
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            // Назначаем объект как heldItem, чтобы его можно было дропать
            player.PickUpItem(this.gameObject);
        }
    }

    public void Use(PlayerController player)
    {
        if (!isPickedUp) return;

        PlayerInventory.Instance.UseCatnip();

        // Сбрасываем объект на сцену рядом с игроком
        transform.SetParent(null);
        transform.position = player.transform.position + Vector3.up * 0.5f;

        // Включаем визуализацию и физику
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        isPickedUp = false;
        isUsed = true;

        if (attractor != null)
            attractor.SetActive(true);
    }

    // ------------------ Для врага ------------------
    public bool IsPickedUp() => isPickedUp;
    public bool IsEaten() => isEaten;
    public bool IsUsed() => isUsed;

    public void EatenByCat()
    {
        if (isEaten) return;

        isEaten = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        isPickedUp = false;
        isUsed = true;

        if (attractor != null)
            StartCoroutine(DisableAttractorAfterTime());
    }

    private IEnumerator DisableAttractorAfterTime()
    {
        yield return new WaitForSeconds(calmDuration);

        if (attractor != null)
            attractor.SetActive(false);

        Destroy(gameObject);
    }
}
