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

    // Variáveis de contagem mantidas para compatibilidade com outros scripts
    public int ammoCount
    {
        get { return GetTotalAmount(ItemPickup2D.ItemType.Ammo); }
    }

    public int decontamKitsCount
    {
        get { return GetTotalAmount(ItemPickup2D.ItemType.DecontamKit); }
    }

    public bool hasGun = true;

    public bool AddItem(string name, Sprite icon, ItemPickup2D.ItemType type, int amount)
    {
        // 1. Verifica se o item já existe na Hotbar para agrupar (stack)
        foreach (var slot in slots)
        {
            if (slot.type == type)
            {
                slot.amount += amount;
                return true;
            }
        }

        // 2. Se for um item novo, insere no próximo slot livre em ordem crescente
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

        return false; // Inventário cheio
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