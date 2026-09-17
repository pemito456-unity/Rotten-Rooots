using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatAndItems : MonoBehaviour
{
    [Header("Referências Principais")]
    public PlayerInventory inventory;
    public Camera mainCamera;               // Referência para a câmera do jogo

    [Header("Configurações do Tiro e Mira")]
    public Transform firePoint;             // Ponto de onde sai a bala
    public GameObject bulletPrefab;         // Prefab da bala
    public float wrathPerShot = 10f;        // Quanto aumenta a Ira por tiro

    [Header("Configurações da Faca")]
    public float wrathPerKnifeAttack = 5f;  // Quanto aumenta a Ira por golpe
    public float knifeRange = 1.2f;         // Alcance do ataque de faca
    public LayerMask attackableLayers;       // Camada de inimigos/arbustos

    [Header("Configurações do Kit Eco")]
    public float wrathReductionPerKit = 25f; // Quanto reduz a Ira por kit

    private Vector3 mouseWorldPosition;

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        // Atualiza a rotação do FirePoint para apontar sempre para o cursor do mouse
        AimTowardsMouse();

        if (Keyboard.current == null || Mouse.current == null) return;

        // 1. Atirar (Botão DIREITO do Mouse)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryShoot();
        }

        // 2. Usar Faca (Tecla V)
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            PerformKnifeAttack();
        }

        // 3. Usar Kit de Descontaminação (Tecla Q)
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            TryUseDecontamKit();
        }
    }

    // --- LÓGICA DE MIRA PARA O CURSOR ---
    private void AimTowardsMouse()
    {
        if (mainCamera == null || firePoint == null) return;

        // Pega a posição do mouse na tela e converte para coordenadas do mundo 2D
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPosition.z = 0f; // Mantém no plano 2D

        // Calcula a direção do FirePoint até o mouse
        Vector2 aimDirection = (mouseWorldPosition - firePoint.position).normalized;

        // Calcula o ângulo em graus
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Aplica a rotação no FirePoint
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // --- LÓGICA DE TIRO ---
    private void TryShoot()
    {
        var ammoSlot = inventory.slots.Find(s => s.type == ItemPickup2D.ItemType.Ammo);

        if (ammoSlot != null && ammoSlot.amount > 0)
        {
            // Consome 1 munição
            ammoSlot.amount--;
            if (ammoSlot.amount <= 0)
            {
                inventory.slots.Remove(ammoSlot);
            }

            // Instancia o projétil com a rotação exata apontando para o mouse
            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            }

            // Aumenta a Ira da Floresta
            if (ForestWrathManager.Instance != null)
            {
                ForestWrathManager.Instance.AddWrath(wrathPerShot);
            }

            // Atualiza a UI da Hotbar
            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.UpdateHotbarUI();
            }

            Debug.Log("Tiro disparado na direção do cursor!");
        }
        else
        {
            Debug.Log("Sem munição suficiente no inventário!");
        }
    }

    // --- LÓGICA DA FACA ---
    private void PerformKnifeAttack()
    {
        Debug.Log("Ataque de Faca executado!");

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, knifeRange, attackableLayers);

        foreach (Collider2D obj in hitObjects)
        {
            Debug.Log($"Faca atingiu: {obj.name}");
        }

        if (ForestWrathManager.Instance != null)
        {
            ForestWrathManager.Instance.AddWrath(wrathPerKnifeAttack);
        }
    }

    // --- LÓGICA DO KIT DE DESCONTAMINAÇÃO ---
    private void TryUseDecontamKit()
    {
        var kitSlot = inventory.slots.Find(s => s.type == ItemPickup2D.ItemType.DecontamKit);

        if (kitSlot != null && kitSlot.amount > 0)
        {
            kitSlot.amount--;
            if (kitSlot.amount <= 0)
            {
                inventory.slots.Remove(kitSlot);
            }

            if (ForestWrathManager.Instance != null)
            {
                ForestWrathManager.Instance.ReduceWrath(wrathReductionPerKit);
            }

            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.UpdateHotbarUI();
            }

            Debug.Log("Kit usado! Floresta purificada.");
        }
        else
        {
            Debug.Log("Você não possui Kit de Descontaminação na Hotbar!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, knifeRange);

        // Desenha uma linha visual na cena mostrando onde a arma está apontando
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(firePoint.position, firePoint.right * 3f);
        }
    }
}