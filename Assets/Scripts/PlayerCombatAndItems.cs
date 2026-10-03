using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatAndItems : MonoBehaviour
{
    [Header("Referências Principais")]
    public PlayerInventory inventory;

    [Header("Configurações da Arma")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float wrathPerShot = 10f;
    public int magazineCapacity = 8;
    public int currentMagazineAmmo = 8;

    [Header("Configurações do Kit Eco")]
    public float wrathReductionPerKit = 25f;

    private PlayerController2D playerController;

    private void Start()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        playerController = GetComponent<PlayerController2D>();
        currentMagazineAmmo = Mathf.Clamp(currentMagazineAmmo, 0, magazineCapacity);
    }

    // Ação Primary: dispara com a arma selecionada ou usa o kit selecionado.
    public void OnPrimary(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot == null)
            return;

        if (selectedSlot.isFlashlight)
            return;

        switch (selectedSlot.type)
        {
            case ItemPickup2D.ItemType.Gun:
                TryShoot();
                break;

            case ItemPickup2D.ItemType.DecontamKit:
                TryUseDecontamKit(selectedSlot);
                break;

            default:
                Debug.Log("O item selecionado não tem ação atribuída ao botão principal.");
                break;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed || inventory == null)
            return;

        InventorySlotData selectedSlot = inventory.SelectedSlot;

        if (selectedSlot == null ||
            selectedSlot.type != ItemPickup2D.ItemType.Gun ||
            !inventory.hasGun)
        {
            Debug.Log("Selecione a arma para recarregar.");
            return;
        }

        if (currentMagazineAmmo >= magazineCapacity)
        {
            Debug.Log("O pente já está cheio.");
            return;
        }

        InventorySlotData ammoSlot = inventory.slots.Find(
            slot => slot.type == ItemPickup2D.ItemType.Ammo && slot.amount > 0
        );

        if (ammoSlot == null)
        {
            Debug.Log("Você não possui munição reserva.");
            return;
        }

        int spaceInMagazine = magazineCapacity - currentMagazineAmmo;
        int ammoToLoad = Mathf.Min(spaceInMagazine, ammoSlot.amount);

        currentMagazineAmmo += ammoToLoad;
        ammoSlot.amount -= ammoToLoad;

        if (ammoSlot.amount <= 0)
            inventory.slots.Remove(ammoSlot);

        inventory.RefreshInventoryUI();

        Debug.Log($"Arma recarregada: {currentMagazineAmmo}/{magazineCapacity}");
    }

    private void TryShoot()
    {
        if (!inventory.hasGun)
        {
            Debug.Log("Você ainda não possui uma arma.");
            return;
        }

        if (currentMagazineAmmo <= 0)
        {
            Debug.Log("Sem munição no pente. Recarregue.");
            return;
        }

        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogWarning("Configure Fire Point e Bullet Prefab no PlayerCombatAndItems.");
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
        currentMagazineAmmo--;

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathPerShot);

        Debug.Log($"Tiro disparado. Munição no pente: {currentMagazineAmmo}/{magazineCapacity}");
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