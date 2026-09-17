using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public bool isRunning;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    void FixedUpdate()
    {
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);
    }
}