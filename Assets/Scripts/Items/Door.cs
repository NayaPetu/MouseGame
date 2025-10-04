using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject roomA;
    public GameObject roomB;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomB != null)
                roomB.SetActive(true);

            if (animator != null)
                animator.SetTrigger("Open");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (animator != null)
                animator.SetTrigger("Close");
        }
    }
}
