using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;
    public Vector2 lastFacingDirection = Vector2.right;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isRunning;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (Gamepad.current != null)
        {
            moveInput = Gamepad.current.leftStick.ReadValue();
            if (moveInput.magnitude < 0.1f) moveInput = Gamepad.current.dpad.ReadValue();
        }
        else if (Keyboard.current != null)
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1;
            moveInput = input.normalized;
        }

        if (moveInput.sqrMagnitude > 0.01f) lastFacingDirection = moveInput.normalized;
        isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
    }

    private void FixedUpdate()
    {
        // Se estiver em estado de Knockback, ignora o input do jogador para não anular a força
        if (playerHealth != null && playerHealth.isKnockbacked) return;

        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;
        rb.linearVelocity = moveInput * currentSpeed;
    }
}