using System.Collections.Generic;
using UnityEngine;

// Certifique-se de que a estrutura do Slot tenha algo parecido com isso:
[System.Serializable]
public class InventorySlotData
{
    public string itemName;
    public Sprite icon;
    public int amount;
    public bool isFlashlight; // Identificador simples
}

public class PlayerInventory : MonoBehaviour
{
    public List<InventorySlotData> slots = new List<InventorySlotData>();

    [Header("Configuração da Lanterna Inicial")]
    public Sprite flashlightIcon; // Arraste a arte/ícone da lanterna no Inspector

    private void Awake()
    {
        SetupDefaultFlashlight();
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

        // Se a lista estiver vazia, adiciona no índice 0
        if (slots.Count == 0)
        {
            slots.Add(flashlightSlot);
        }
        else
        {
            // Se já tiver itens, força o Slot 1 (índice 0) a ser a Lanterna
            slots[0] = flashlightSlot;
        }
    }
}