using UnityEngine;

public class FriendNote : MonoBehaviour
{
    [TextArea]
    public string noteText = "Друг в подвале!";

    private bool playerInside = false;

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            ReadNote();
        }
    }

    private void ReadNote()
    {
        Debug.Log("Записка: " + noteText);
        // позже сюда можно подключить UI
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}
