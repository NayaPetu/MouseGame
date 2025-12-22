using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FriendMouse : MonoBehaviour
{
    public float followSpeed = 3f;
    public float followDistance = 0.6f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isFollowing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (!isFollowing || player == null) return;

        float dist = Vector2.Distance(rb.position, player.position);
        if (dist > followDistance)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * followSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFollowing) return;

        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isFollowing = true;

            GameManager.Instance.friendRescued = true;
            Debug.Log("🐭 Подруга спасена!");
        }
    }
}
