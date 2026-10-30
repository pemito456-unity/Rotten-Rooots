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
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
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

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        if (notePanel != null && notePanel.IsOpen)
        {
            notePanel.Close();
            return;
        }

        if (CorpseInvestigation.TryOpenNearby(transform.position))
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

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        if (notePanel != null && notePanel.IsOpen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Mantém o controle bloqueado durante o knockback.
        if (playerHealth != null && playerHealth.isKnockbacked)
            return;

        float currentSpeed = isRunning
            ? moveSpeed * runMultiplier
            : moveSpeed;

        rb.linearVelocity = moveInput * currentSpeed;
    }

    private void Update()
    {
        if (animator == null)
            return;

        CorpseNotePanelController notePanel = CorpseNotePanelController.Instance;
        bool movementBlocked = (notePanel != null && notePanel.IsOpen) ||
                               (playerHealth != null && playerHealth.isKnockbacked) ||
                               (playerHealth != null && playerHealth.currentHealth <= 0f);

        Vector2 facing = lastFacingDirection.sqrMagnitude > 0.01f
            ? lastFacingDirection.normalized
            : Vector2.right;

        animator.SetFloat("MoveX", facing.x);
        animator.SetFloat("MoveY", facing.y);
        animator.SetBool("IsMoving", !movementBlocked && moveInput.sqrMagnitude > 0.01f);
    }
}
