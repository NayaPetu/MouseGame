using UnityEngine;
using System.Collections;

public class PowerCheese : MonoBehaviour
{
    public Vector3 sizeMultiplier = new Vector3(1.5f, 1.5f, 1f);
    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        player.StartCoroutine(ApplyEffect(player));
        Debug.Log("Сыр съеден! Мышь увеличена.");
        Destroy(gameObject); // мгновенно исчезает после использования
    }

    private IEnumerator ApplyEffect(PlayerController player)
    {
        Vector3 originalSize = player.transform.localScale;
        player.transform.localScale = originalSize + sizeMultiplier;

        yield return new WaitForSeconds(duration);

        player.transform.localScale = originalSize;
    }
}
