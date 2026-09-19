using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;
        public Sprite icon;
        public ItemPickup2D.ItemType type;
        public int amount;
    }

    [Header("Configurações da Hotbar")]
    public int maxSlots = 5;
    public List<InventorySlotData> slots = new List<InventorySlotData>();

    // A arma agora começa BLOQUEADA (false) até ser coletada no mapa
    public bool hasGun = false;

    public int ammoCount => GetTotalAmount(ItemPickup2D.ItemType.Ammo);
    public int decontamKitsCount => GetTotalAmount(ItemPickup2D.ItemType.DecontamKit);

    public bool AddItem(string name, Sprite icon, ItemPickup2D.ItemType type, int amount)
    {
        // Se o item coletado for a Arma, habilita o uso das armas de fogo
        if (type == ItemPickup2D.ItemType.Gun)
        {
            hasGun = true;
        }

        // 1. Agrupa se já existir no inventário
        foreach (var slot in slots)
        {
            if (slot.type == type)
            {
                slot.amount += amount;
                return true;
            }
        }

        // 2. Adiciona em um slot livre
        if (slots.Count < maxSlots)
        {
            InventorySlotData newSlot = new InventorySlotData
            {
                itemName = name,
                icon = icon,
                type = type,
                amount = amount
            };
            slots.Add(newSlot);
            return true;
        }

        return false;
    }

    private int GetTotalAmount(ItemPickup2D.ItemType type)
    {
        foreach (var slot in slots)
        {
            if (slot.type == type) return slot.amount;
        }
        return 0;
    }
}