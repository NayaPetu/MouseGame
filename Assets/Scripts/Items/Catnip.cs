using UnityEngine;
using System.Collections;

public class Catnip : MonoBehaviour
{
    [Header("Настройки мяты")]
    public float calmDuration = 5f;      // сколько времени враг будет спокойным
    public GameObject attractor;         // зона действия

    private bool isUsed = false;
    private bool isEaten = false;

    // ================== Для врага ==================
    public bool IsUsed() => isUsed;
    public bool IsEaten() => isEaten;

    // ================== Подбор игроком ==================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Добавляем в инвентарь
        InventoryManager.Instance?.AddItem(this.gameObject);

        Debug.Log("Catnip подобран!");
        gameObject.SetActive(false); // спрятать визуально, игрок держит в инвентаре
    }

    // ================== Использование игроком ==================
    public void Use()
    {
        if (isUsed) return;

        isUsed = true;

        if (attractor != null)
            attractor.SetActive(true);

        transform.SetParent(null);
        gameObject.SetActive(true);
    }

    // ================== Когда кот съел ==================
    public void EatenByCat()
    {
        if (isEaten) return;
        isEaten = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DisableAfterTime());
    }

    private IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(calmDuration);

        if (attractor != null)
            attractor.SetActive(false);

        Destroy(gameObject);
    }
}
