using UnityEngine;
using System.Collections;

public class Catnip : MonoBehaviour, IInteractable, IUsable
{
    [Header("Настройки мяты")]
    public float calmDuration = 5f;      // сколько времени враг будет спокойным
    public GameObject attractor;         // дочерний объект зоны действия мяты

    private bool isPickedUp = false;
    private bool isEaten = false;
    private bool isUsed = false;

    // ------------------ Для игрока ------------------
    public void Interact(PlayerController player)
    {
        if (isPickedUp) return;

        player.PickUpItem(this.gameObject);
        isPickedUp = true;
    }

    public void Use(PlayerController player)
    {
        if (!isPickedUp) return;

        transform.SetParent(null);
        transform.position = player.itemHoldPosition.position; // оставляем там, где игрок стоит
        isPickedUp = false;
        isUsed = true;

        // Включаем зону действия
        if (attractor != null)
            attractor.SetActive(true);

        // Включаем Collider и Rigidbody, чтобы враг мог обнаружить мяту
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;
    }

    // ------------------ Для врага ------------------
    public bool IsPickedUp() => isPickedUp;
    public bool IsEaten() => isEaten;
    public bool IsUsed() => isUsed;

    public void EatenByCat()
    {
        if (isEaten) return;

        isEaten = true;

        // Делаем мяту невидимой и отключаем коллайдер
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Отключаем зону действия через время
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
