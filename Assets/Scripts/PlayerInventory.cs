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
    public bool isCamera;
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

    [Header("Câmera fixa")]
    public Sprite cameraIcon;

    [Header("Itens que podem ser descartados")]
    public GameObject ammoPickupPrefab;
    public GameObject decontamKitPickupPrefab;
    public GameObject gunPickupPrefab;
    public float dropDistance = 0.8f;
    public float wrathOnDrop = 5f;

    private FlashlightController flashlightController;
    private Sprite generatedCameraIcon;
    private Texture2D generatedCameraIconTexture;

    public FlashlightController FlashlightController
    {
        get
        {
            if (flashlightController == null)
                flashlightController = GetComponentInChildren<FlashlightController>(true);
            return flashlightController;
        }
    }

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
        SetupDefaultCamera();
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

    private void SetupDefaultCamera()
    {
        if (slots.Exists(slot => slot.isCamera))
            return;

        if (slots.Count >= maxSlots)
        {
            Debug.LogWarning("Não há espaço para adicionar a câmera fixa ao inventário.");
            return;
        }

        if (cameraIcon == null)
            cameraIcon = CreateCameraPlaceholderIcon();

        InventorySlotData cameraSlot = new InventorySlotData
        {
            itemName = "Câmera",
            icon = cameraIcon,
            amount = 1,
            type = ItemPickup2D.ItemType.PhotoCamera,
            isCamera = true
        };

        // A câmera fica logo depois da lanterna para ser fácil de encontrar no teste.
        slots.Insert(Mathf.Min(1, slots.Count), cameraSlot);
    }

    private Sprite CreateCameraPlaceholderIcon()
    {
        const int size = 32;
        generatedCameraIconTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        generatedCameraIconTexture.name = "CameraPlaceholderIconTexture";
        generatedCameraIconTexture.filterMode = FilterMode.Point;
        generatedCameraIconTexture.wrapMode = TextureWrapMode.Clamp;

        Color transparent = new Color(0f, 0f, 0f, 0f);
        Color body = new Color(0.82f, 0.82f, 0.76f, 1f);
        Color dark = new Color(0.12f, 0.14f, 0.13f, 1f);
        Color lens = new Color(0.24f, 0.55f, 0.58f, 1f);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = transparent;

        for (int y = 7; y <= 24; y++)
        {
            for (int x = 3; x <= 28; x++)
                pixels[y * size + x] = body;
        }

        for (int y = 22; y <= 27; y++)
        {
            for (int x = 9; x <= 17; x++)
                pixels[y * size + x] = body;
        }

        for (int y = 10; y <= 21; y++)
        {
            for (int x = 12; x <= 23; x++)
            {
                int dx = x - 17;
                int dy = y - 15;
                if (dx * dx + dy * dy <= 36)
                    pixels[y * size + x] = dark;
                if (dx * dx + dy * dy <= 16)
                    pixels[y * size + x] = lens;
            }
        }

        for (int y = 18; y <= 21; y++)
        {
            for (int x = 6; x <= 9; x++)
                pixels[y * size + x] = dark;
        }

        generatedCameraIconTexture.SetPixels(pixels);
        generatedCameraIconTexture.Apply(false, true);

        generatedCameraIcon = Sprite.Create(
            generatedCameraIconTexture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            32f
        );
        return generatedCameraIcon;
    }

    public bool AddItem(
        string name,
        Sprite icon,
        ItemPickup2D.ItemType type,
        int amount,
        int carriedAmmo = 0)
    {
        amount = Mathf.Max(1, amount);

        // A câmera faz parte do equipamento inicial e não é coletável nem empilhável.
        if (type == ItemPickup2D.ItemType.PhotoCamera)
        {
            Debug.LogWarning("A câmera é um item fixo e não pode ser coletada como pickup.");
            return false;
        }

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

        if (slot == null || slot.isFlashlight || slot.isCamera)
        {
            Debug.Log("Não é possível descartar a lanterna ou a câmera.");
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

    private void OnDestroy()
    {
        if (generatedCameraIcon != null)
            Destroy(generatedCameraIcon);
        if (generatedCameraIconTexture != null)
            Destroy(generatedCameraIconTexture);
    }
}
