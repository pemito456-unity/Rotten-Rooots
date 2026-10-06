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

    // Conecte à ação Move no PlayerInput.
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // Mantém apenas o eixo predominante para bloquear diagonais.
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            moveInput = new Vector2(Mathf.Sign(input.x), 0f);
        else if (Mathf.Abs(input.y) > 0.01f)
            moveInput = new Vector2(0f, Mathf.Sign(input.y));
        else
            moveInput = Vector2.zero;

        if (moveInput != Vector2.zero)
            lastFacingDirection = moveInput;
    }

    // Conecte à ação Interact no PlayerInput.
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Debug.Log("[Coleta] E/interação recebido pelo PlayerController2D.");

        PlayerInventory inventory = GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogError("[Coleta] PlayerInventory não foi encontrado no Player.");
            return;
        }

        ItemPickup2D.TryCollectNearby(transform.position, inventory);
    }

    // Opcional: só será usado se houver uma ação Run no Input Actions.
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Mantém o controle bloqueado durante o knockback.
        if (playerHealth != null && playerHealth.isKnockbacked)
            return;

        float currentSpeed = isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        rb.linearVelocity = moveInput * currentSpeed;
    }
}