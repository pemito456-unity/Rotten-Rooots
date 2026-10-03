using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimentação")]
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

    // Conecte esta função à ação Move no componente PlayerInput.
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)

    {
        Vector2 input = context.ReadValue<Vector2>();

        // Mantém apenas o eixo predominante para impedir diagonais.
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        moveInput = new Vector2(Mathf.Sign(input.x), 0f);
        else if (Mathf.Abs(input.y) > 0.01f)
        moveInput = new Vector2(0f, Mathf.Sign(input.y));
        else
        moveInput = Vector2.zero;

        if (moveInput != Vector2.zero)
        lastFacingDirection = moveInput;
    }

    public void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed)
        return;

        ItemPickup2D.TryCollectNearby(
        transform.position,
        GetComponent<PlayerInventory>()
        );
    }

    // Conecte esta função à ação Run no componente PlayerInput.
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Mantém o controle do jogador bloqueado durante o knockback.
        if (playerHealth != null && playerHealth.isKnockbacked)
            return;

        float currentSpeed = isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        rb.linearVelocity = moveInput * currentSpeed;
    }
}