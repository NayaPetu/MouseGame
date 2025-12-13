using UnityEngine;

public class CatCatcher : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player caught!");
            GameManager.Instance.OnPlayerCaught();
        }
    }
}
