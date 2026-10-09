using UnityEngine;
using TMPro;

public class ItemPickup2D : MonoBehaviour
{
    public enum ItemType
    {
        Ammo,
        DecontamKit,
        Gun
    }

    [Header("Configurações do item")]
    public ItemType itemType;
    public int amount = 1;
    public string itemName = "Munição";
    public Sprite itemIcon;

    [Header("Dados carregados pelo item")]
    public int carriedAmmo;

    [Header("UI do item")]
    public GameObject promptCanvas;
    public TextMeshProUGUI promptText;

    private bool playerInRange;

    private void Start()
    {
        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        if (promptText != null)
            promptText.text = $"{itemName}\n[E / Y] Coletar";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (promptCanvas != null)
            promptCanvas.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void CollectItem(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError($"PlayerInventory ausente ao coletar {itemName}.");
            return;
        }

        Vector3 pickupWorldPosition = transform.position;

        bool success = inventory.AddItem(
            itemName,
            itemIcon,
            itemType,
            amount,
            carriedAmmo
        );

        if (!success)
        {
            Debug.LogWarning("Inventário cheio.");
            return;
        }

        if (InventoryUIManager.Instance != null)
        {
            if (itemType == ItemType.Ammo)
                InventoryUIManager.Instance.ShowAmmoPickupNotice(pickupWorldPosition, amount);
            else
                InventoryUIManager.Instance.ShowCollectionNotice(itemName, amount);

            InventoryUIManager.Instance.UpdateHotbarUI();
        }

        Destroy(gameObject);
    }

    public static void TryCollectNearby(
        Vector2 playerPosition,
        PlayerInventory inventory)
    {
        if (inventory == null)
            return;

        ItemPickup2D[] pickups = FindObjectsOfType<ItemPickup2D>();
        ItemPickup2D nearestPickup = null;
        float nearestDistance = float.MaxValue;

        foreach (ItemPickup2D pickup in pickups)
        {
            if (!pickup.playerInRange)
                continue;

            float distance = Vector2.Distance(
                playerPosition,
                pickup.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPickup = pickup;
            }
        }

        if (nearestPickup != null)
            nearestPickup.CollectItem(inventory);
    }
}
