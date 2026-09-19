using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ItemPickup2D : MonoBehaviour
{
    // Adicionado 'Gun' ao Enum de tipos de item
    public enum ItemType { Ammo, DecontamKit, Gun }
    
    [Header("Configurações do Item")]
    public ItemType itemType;
    public int amount = 1;
    public string itemName = "Munição";
    public Sprite itemIcon;

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
            bool success = playerInventory.AddItem(itemName, itemIcon, itemType, amount);

            if (success)
            {
                if (InventoryUIManager.Instance != null)
                {
                    InventoryUIManager.Instance.ShowCollectionNotice(itemName, amount);
                    InventoryUIManager.Instance.UpdateHotbarUI();
                }

                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventário Cheio!");
            }
        }
    }
}