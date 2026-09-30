using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public string itemName;
    public Sprite icon;
    public int amount;
    public ItemPickup2D.ItemType type; // Tipo de item (Ammo, DecontamKit, Gun)
    public bool isFlashlight;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Estado do Jogador")]
    public bool hasGun = false;

    [Header("Configurações do Inventário")]
    public int maxSlots = 8;
    public List<InventorySlotData> slots = new List<InventorySlotData>();

    [Header("Configuração da Lanterna Inicial")]
    public Sprite flashlightIcon;

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

        if (slots.Count == 0)
        {
            slots.Add(flashlightSlot);
        }
        else if (!slots[0].isFlashlight)
        {
            slots[0] = flashlightSlot;
        }
    }

    // Método chamado pelo ItemPickup2D para coletar itens
    public bool AddItem(string name, Sprite icon, ItemPickup2D.ItemType type, int amount)
    {
        // Se o item coletado for uma arma, ativa a posse da arma no player
        if (type == ItemPickup2D.ItemType.Gun)
        {
            hasGun = true;
        }

        // 1. Procura se já existe um slot com esse mesmo item para empilhar
        InventorySlotData existingSlot = slots.Find(s => s.type == type && s.itemName == name);
        if (existingSlot != null)
        {
            existingSlot.amount += amount;
            return true;
        }

        // 2. Se não existir e ainda houver espaço no limite de slots, cria um novo slot
        if (slots.Count < maxSlots)
        {
            InventorySlotData newSlot = new InventorySlotData
            {
                itemName = name,
                icon = icon,
                type = type,
                amount = amount,
                isFlashlight = false
            };

            slots.Add(newSlot);
            return true;
        }

        // Inventário cheio
        return false;
    }
}