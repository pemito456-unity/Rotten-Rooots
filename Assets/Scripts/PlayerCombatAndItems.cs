using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatAndItems : MonoBehaviour
{
    [Header("Referências principais")]
    public PlayerInventory inventory;

    [Header("Configurações da pistola")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float wrathPerShot = 10f;

    [Header("Configurações do kit de descontaminação")]
    public float wrathReductionPerKit = 25f;

    private PlayerController2D playerController;

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        playerController = GetComponent<PlayerController2D>();
    }

    // Pistola selecionada: dispara.
    // Kit selecionado: usa o kit.
    public void OnPrimary(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot == null || selectedSlot.isFlashlight)
            return;

        switch (selectedSlot.type)
        {
            case ItemPickup2D.ItemType.Gun:
                TryShoot(selectedSlot);
                break;

            case ItemPickup2D.ItemType.DecontamKit:
                TryUseDecontamKit(selectedSlot);
                break;

            default:
                Debug.Log("O item selecionado não tem ação no botão principal.");
                break;
        }
    }

    // A pistola não recarrega.
    // Este callback fica disponível para a recarga futura da lanterna.
    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot != null && selectedSlot.isFlashlight)
        {
            Debug.Log("A recarga da lanterna ainda precisa ser implementada.");
            return;
        }

        Debug.Log("A pistola não é recarregável.");
    }

    private void TryShoot(InventorySlotData pistolSlot)
    {
        if (!inventory.hasGun)
        {
            Debug.Log("Você ainda não possui uma pistola.");
            return;
        }

        if (pistolSlot.ammoCount <= 0)
        {
            Debug.Log("A pistola está sem munição.");
            return;
        }

        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogWarning(
                "Configure Fire Point e Bullet Prefab no PlayerCombatAndItems."
            );
            return;
        }

        Vector2 direction = playerController != null
            ? playerController.lastFacingDirection
            : Vector2.right;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.right;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        pistolSlot.ammoCount--;
        inventory.RefreshInventoryUI();

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathPerShot);

        Debug.Log($"Tiro disparado. Munição restante: {pistolSlot.ammoCount}.");
    }

    private void TryUseDecontamKit(InventorySlotData kitSlot)
    {
        if (kitSlot == null || kitSlot.amount <= 0)
            return;

        kitSlot.amount--;

        if (kitSlot.amount <= 0)
            inventory.slots.Remove(kitSlot);

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.ReduceWrath(wrathReductionPerKit);

        inventory.RefreshInventoryUI();

        Debug.Log("Kit usado. A ira da floresta diminuiu.");
    }
}