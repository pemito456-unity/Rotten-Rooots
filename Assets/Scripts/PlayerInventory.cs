using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class InventorySlotData
{
    public string itemName;
    public Sprite icon;
    public int amount;
    public ItemPickup2D.ItemType type;
    public bool isFlashlight;

    // Usado pelo slot da pistola.
    public int ammoCount;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Estado do jogador")]
    public bool hasGun;

    [Header("Configurações do inventário")]
    public int maxSlots = 8;
    public List<InventorySlotData> slots = new List<InventorySlotData>();
    public int selectedSlotIndex;

    [Header("Munição coletada antes da pistola")]
    public int pendingPistolAmmo;

    [Header("Lanterna inicial")]
    public Sprite flashlightIcon;

    [Header("Itens que podem ser descartados")]
    public GameObject ammoPickupPrefab;
    public GameObject decontamKitPickupPrefab;
    public GameObject gunPickupPrefab;
    public float dropDistance = 0.8f;
    public float wrathOnDrop = 5f;

    private FlashlightController flashlightController;

    public InventorySlotData SelectedSlot
    {
        get
        {
            if (slots == null || slots.Count == 0)
                return null;

            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, slots.Count - 1);
            return slots[selectedSlotIndex];
        }
    }

    private void Awake()
    {
        SetupDefaultFlashlight();
        UpdateGunState();
    }

    private void Start()
    {
        flashlightController = GetComponentInChildren<FlashlightController>(true);
        ApplySelectedItem();
        RefreshInventoryUI();
    }

    private void SetupDefaultFlashlight()
    {
        InventorySlotData flashlightSlot = new InventorySlotData
        {
            itemName = "Lanterna",
            icon = flashlightIcon,
            amount = 1,
            isFlashlight = true
        };

        if (slots.Count == 0)
        {
            slots.Add(flashlightSlot);
        }
        else if (!slots[0].isFlashlight)
        {
            slots[0] = flashlightSlot;
        }
    }

    public bool AddItem(
        string name,
        Sprite icon,
        ItemPickup2D.ItemType type,
        int amount,
        int carriedAmmo = 0)
    {
        amount = Mathf.Max(1, amount);

        // Munição não ocupa um slot próprio.
        if (type == ItemPickup2D.ItemType.Ammo)
        {
            InventorySlotData pistolSlot = slots.Find(
                slot => slot.type == ItemPickup2D.ItemType.Gun
            );

            if (pistolSlot != null)
            {
                pistolSlot.ammoCount += amount;
                Debug.Log($"Munição adicionada à pistola: {pistolSlot.ammoCount}.");
            }
            else
            {
                pendingPistolAmmo += amount;
                Debug.Log(
                    $"Munição guardada até encontrar a pistola: {pendingPistolAmmo}."
                );
            }

            RefreshInventoryUI();
            return true;
        }

        // Mantém uma única pistola no inventário.
        if (type == ItemPickup2D.ItemType.Gun)
        {
            InventorySlotData existingPistol = slots.Find(
                slot => slot.type == ItemPickup2D.ItemType.Gun
            );

            if (existingPistol != null)
            {
                existingPistol.ammoCount += carriedAmmo;
                RefreshInventoryUI();
                return true;
            }

            if (slots.Count >= maxSlots)
                return false;

            InventorySlotData newPistolSlot = new InventorySlotData
            {
                itemName = name,
                icon = icon,
                type = type,
                amount = 1,
                isFlashlight = false,
                ammoCount = pendingPistolAmmo + Mathf.Max(0, carriedAmmo)
            };

            slots.Add(newPistolSlot);
            pendingPistolAmmo = 0;
            UpdateGunState();
            RefreshInventoryUI();

            Debug.Log($"Pistola coletada com {newPistolSlot.ammoCount} munições.");
            return true;
        }

        // Kits e outros itens empilháveis mantêm seu comportamento atual.
        InventorySlotData existingSlot = slots.Find(
            slot => slot.type == type && slot.itemName == name
        );

        if (existingSlot != null)
        {
            existingSlot.amount += amount;
            RefreshInventoryUI();
            return true;
        }

        if (slots.Count >= maxSlots)
            return false;

        InventorySlotData newSlot = new InventorySlotData
        {
            itemName = name,
            icon = icon,
            type = type,
            amount = amount,
            isFlashlight = false
        };

        slots.Add(newSlot);
        UpdateGunState();
        RefreshInventoryUI();
        return true;
    }

    public void OnNextItem(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectSlot(selectedSlotIndex + 1);
    }

    public void OnPreviousItem(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectSlot(selectedSlotIndex - 1);
    }

    public void OnSelectSlot1(InputAction.CallbackContext context) { if (context.performed) SelectSlot(0); }
    public void OnSelectSlot2(InputAction.CallbackContext context) { if (context.performed) SelectSlot(1); }
    public void OnSelectSlot3(InputAction.CallbackContext context) { if (context.performed) SelectSlot(2); }
    public void OnSelectSlot4(InputAction.CallbackContext context) { if (context.performed) SelectSlot(3); }
    public void OnSelectSlot5(InputAction.CallbackContext context) { if (context.performed) SelectSlot(4); }
    public void OnSelectSlot6(InputAction.CallbackContext context) { if (context.performed) SelectSlot(5); }
    public void OnSelectSlot7(InputAction.CallbackContext context) { if (context.performed) SelectSlot(6); }
    public void OnSelectSlot8(InputAction.CallbackContext context) { if (context.performed) SelectSlot(7); }

    private void SelectSlot(int index)
    {
        if (slots == null || slots.Count == 0)
            return;

        if (index < 0)
            index = slots.Count - 1;
        else if (index >= slots.Count)
            index = 0;

        selectedSlotIndex = index;
        ApplySelectedItem();
        RefreshInventoryUI();
    }

    private void ApplySelectedItem()
    {
        if (flashlightController == null)
            flashlightController = GetComponentInChildren<FlashlightController>(true);

        if (flashlightController != null)
            flashlightController.isOn = SelectedSlot != null && SelectedSlot.isFlashlight;
    }

    public void OnDropItem(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        InventorySlotData slot = SelectedSlot;

        if (slot == null || slot.isFlashlight)
        {
            Debug.Log("Não é possível descartar a lanterna.");
            return;
        }

        GameObject pickupPrefab = GetPickupPrefab(slot.type);

        if (pickupPrefab == null)
        {
            Debug.LogWarning($"Configure o prefab de descarte de {slot.itemName}.");
            return;
        }

        Vector2 direction = GetDropDirection();
        Vector3 dropPosition = transform.position + (Vector3)(direction * dropDistance);

        GameObject droppedObject = Instantiate(
            pickupPrefab,
            dropPosition,
            Quaternion.identity
        );

        ItemPickup2D droppedPickup = droppedObject.GetComponent<ItemPickup2D>();

        if (droppedPickup == null)
        {
            Debug.LogError("O prefab precisa ter o componente ItemPickup2D.");
            Destroy(droppedObject);
            return;
        }

        droppedPickup.itemName = slot.itemName;
        droppedPickup.itemIcon = slot.icon;
        droppedPickup.itemType = slot.type;
        droppedPickup.amount = 1;

        // A munição acompanha a pistola quando ela é descartada.
        droppedPickup.carriedAmmo = slot.type == ItemPickup2D.ItemType.Gun
            ? slot.ammoCount
            : 0;

        slot.amount--;

        if (slot.amount <= 0)
            slots.Remove(slot);

        selectedSlotIndex = Mathf.Clamp(
            selectedSlotIndex,
            0,
            Mathf.Max(0, slots.Count - 1)
        );

        UpdateGunState();
        ApplySelectedItem();
        RefreshInventoryUI();

        if (ForestWrathManager.Instance != null)
            ForestWrathManager.Instance.AddWrath(wrathOnDrop);
    }

    private GameObject GetPickupPrefab(ItemPickup2D.ItemType type)
    {
        switch (type)
        {
            case ItemPickup2D.ItemType.Ammo:
                return ammoPickupPrefab;
            case ItemPickup2D.ItemType.DecontamKit:
                return decontamKitPickupPrefab;
            case ItemPickup2D.ItemType.Gun:
                return gunPickupPrefab;
            default:
                return null;
        }
    }

    private Vector2 GetDropDirection()
    {
        PlayerController2D controller = GetComponent<PlayerController2D>();

        if (controller != null &&
            controller.lastFacingDirection.sqrMagnitude > 0.01f)
        {
            return controller.lastFacingDirection.normalized;
        }

        return Vector2.right;
    }

    private void UpdateGunState()
    {
        hasGun = slots.Exists(slot => slot.type == ItemPickup2D.ItemType.Gun);
    }

    public void RefreshInventoryUI()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.UpdateHotbarUI();
    }
}