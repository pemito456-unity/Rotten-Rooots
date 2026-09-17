using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ItemPickup2D : MonoBehaviour
{
    public enum ItemType { Ammo, DecontamKit }
    
    [Header("Configurações do Item")]
    public ItemType itemType;
    public int amount = 1;
    public string itemName = "Munição";

    [Header("UI Pixel HUD")]
    public GameObject promptCanvas;
    public TextMeshProUGUI promptText;

    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    private void Start()
    {
        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        if (promptText != null)
            promptText.text = $"{itemName}\n[E] Coletar";
    }

    private void Update()
    {
        // Pressiona E para efetuar a coleta
        if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CollectItem();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInventory = other.GetComponent<PlayerInventory>();
            playerInRange = true;

            if (promptCanvas != null)
                promptCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInventory = null;

            if (promptCanvas != null)
                promptCanvas.SetActive(false);
        }
    }

    private void CollectItem()
    {
        if (playerInventory != null)
        {
            // Adiciona o recurso específico no inventário do Player
            switch (itemType)
            {
                case ItemType.Ammo:
                    playerInventory.AddAmmo(amount);
                    break;

                case ItemType.DecontamKit:
                    playerInventory.AddDecontamKit(amount);
                    break;
            }

            // Avisa a UI sobre o item coletado
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.ShowCollectionNotice(itemName, amount);
            InventoryUIManager.Instance.UpdateGridData();
        }
            Destroy(gameObject);
        }
    }
}