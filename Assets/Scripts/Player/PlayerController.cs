using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;       // скорость движения
    private Rigidbody2D rb;            // ссылка на Rigidbody2D игрока
    private Vector2 moveInput;         // куда идём

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // находим Rigidbody2D у объекта
    }

    void Update()
    {
        // читаем ввод (стрелки / WASD)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        // двигаем через Rigidbody2D (чтобы работала физика и коллизии)
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
