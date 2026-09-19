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
        
        // Aplica a velocidade diretamente na física do Rigidbody2D
        // Isso impede totalmente o jogador de atravessar colisão de paredes
        rb.linearVelocity = moveInput.normalized * currentSpeed; 
        // Nota: Se usar versão mais antiga da Unity (anterior a 2023), use 'rb.velocity' no lugar de 'rb.linearVelocity'
    }
}