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

    [Header("Configurações do Item")]
    public ItemType itemType;
    public int amount = 1;
    public string itemName = "Munição";
    public Sprite itemIcon;

    [Header("UI do Item")]
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

        Debug.Log($"[Coleta] Jogador entrou no alcance de: {itemName}.");

        if (promptCanvas != null)
            promptCanvas.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        Debug.Log($"[Coleta] Jogador saiu do alcance de: {itemName}.");

        if (promptCanvas != null)
            promptCanvas.SetActive(false);
    }

    private void CollectItem(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogError($"[Coleta] PlayerInventory nulo ao tentar coletar {itemName}.");
            return;
        }

        bool success = inventory.AddItem(
            itemName,
            itemIcon,
            itemType,
            amount
        );

        if (!success)
        {
            Debug.LogWarning($"[Coleta] Inventário cheio. Não foi possível coletar {itemName}.");
            return;
        }

        Debug.Log($"[Coleta] Item coletado: {itemName} x{amount}.");

        if (InventoryUIManager.Instance != null)
        {
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
        {
            Debug.LogError("[Coleta] PlayerInventory nulo em TryCollectNearby.");
            return;
        }

        // Busca todos os itens ativos; o trigger informa quais estão ao alcance.
        ItemPickup2D[] pickups = FindObjectsOfType<ItemPickup2D>();

        Debug.Log($"[Coleta] Interação procurando entre {pickups.Length} itens ativos.");

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

            Debug.Log(
                $"[Coleta] {pickup.itemName} está no alcance. " +
                $"Distância entre centros: {distance:F2}."
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPickup = pickup;
            }
        }

        if (nearestPickup == null)
        {
            Debug.LogWarning(
                "[Coleta] Nenhum ItemPickup2D marcou playerInRange, " +
                "apesar de o jogador estar vendo o aviso."
            );
            return;
        }

        nearestPickup.CollectItem(inventory);
    }
}