using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Painel Principal do Asset")]
    public GameObject inventoryPanel;       // Arraste o InventoryPanel (com o Frame e a Grid Overlay)
    public GameObject notificationBanner;   // Arraste o NotificationBanner
    public TextMeshProUGUI notificationText;// Texto do banner de notificação

    [Header("Textos dos Slots do Grid")]
    public TextMeshProUGUI ammoSlotText;     // Texto dentro do slot de munição
    public TextMeshProUGUI decontamSlotText; // Texto dentro do slot de kit eco

    [Header("Referência do Jogador")]
    public PlayerInventory playerInventory;

    private bool isInventoryOpen = false;
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (notificationBanner != null) notificationBanner.SetActive(false);
    }

    private void Update()
    {
        // Tecla F para abrir/fechar o inventário
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            if (isInventoryOpen) UpdateGridData();
        }
    }

    public void UpdateGridData()
    {
        if (playerInventory == null) return;

        if (ammoSlotText != null) 
            ammoSlotText.text = playerInventory.ammoCount > 0 ? $"x{playerInventory.ammoCount}" : "";

        if (decontamSlotText != null) 
            decontamSlotText.text = playerInventory.decontamKitsCount > 0 ? $"x{playerInventory.decontamKitsCount}" : "";
    }

    public void ShowCollectionNotice(string itemName, int amount)
    {
        if (notificationBanner == null || notificationText == null) return;

        notificationText.text = $"+{amount} {itemName}";

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(NotificationSequence());
    }

    private IEnumerator NotificationSequence()
    {
        notificationBanner.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        notificationBanner.SetActive(false);
    }
}