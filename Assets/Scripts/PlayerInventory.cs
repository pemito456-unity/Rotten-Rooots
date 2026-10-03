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
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Estado do Jogador")]
    public bool hasGun;

    [Header("Configurações do Inventário")]
    public int maxSlots = 8;
    public List<InventorySlotData> slots = new List<InventorySlotData>();
    public int selectedSlotIndex;

    [Header("Lanterna Inicial")]
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
        hasGun = slots.Exists(slot => slot.type == ItemPickup2D.ItemType.Gun);
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

    public bool AddItem(string name, Sprite icon, ItemPickup2D.ItemType type, int amount)
    {
        InventorySlotData existingSlot = slots.Find(
            slot => slot.type == type && slot.itemName == name
        );

        if (existingSlot != null)
        {
            existingSlot.amount += amount;
            hasGun = slots.Exists(slot => slot.type == ItemPickup2D.ItemType.Gun);
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
        hasGun = slots.Exists(slot => slot.type == ItemPickup2D.ItemType.Gun);

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

        // LB/RB alternam circularmente apenas entre os slots ocupados.
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
            Debug.LogWarning($"Configure um prefab de coleta para o item: {slot.itemName}.");
            return;
        }

        Vector2 dropDirection = GetDropDirection();
        Vector3 dropPosition = transform.position + (Vector3)(dropDirection * dropDistance);

        GameObject droppedObject = Instantiate(
            pickupPrefab,
            dropPosition,
            Quaternion.identity
        );

        ItemPickup2D droppedPickup = droppedObject.GetComponent<ItemPickup2D>();

        if (droppedPickup == null)
        {
            Debug.LogError("O prefab descartado precisa ter o componente ItemPickup2D.");
            Destroy(droppedObject);
            return;
        }

        droppedPickup.itemName = slot.itemName;
        droppedPickup.itemIcon = slot.icon;
        droppedPickup.itemType = slot.type;
        droppedPickup.amount = 1;

        slot.amount--;

        if (slot.amount <= 0)
            slots.Remove(slot);

        selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, Mathf.Max(0, slots.Count - 1));
        hasGun = slots.Exists(item => item.type == ItemPickup2D.ItemType.Gun);

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

        if (controller != null && controller.lastFacingDirection.sqrMagnitude > 0.01f)
            return controller.lastFacingDirection.normalized;

        return Vector2.right;
    }

    public void RefreshInventoryUI()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.UpdateHotbarUI();
    }
}