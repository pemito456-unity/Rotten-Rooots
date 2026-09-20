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

    // --- DADOS PERSISTENTES (Permanecem entre trocas de cena) ---
    private static List<InventorySlotData> persistentSlots = new List<InventorySlotData>();
    private static bool persistentHasGun = false;
    private static bool hasSavedData = false;

    // Esse comando faz a Unity zerar as variáveis estáticas TODA VEZ que você apertar o botão de Play no Editor!
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetInventoryOnPlay()
    {
        persistentSlots = new List<InventorySlotData>();
        persistentHasGun = false;
        hasSavedData = false;
    }

    // Propriedades acessadas pelos outros scripts
    public List<InventorySlotData> slots => persistentSlots;
    public bool hasGun
    {
        get => persistentHasGun;
        set => persistentHasGun = value;
    }

    public int ammoCount => GetTotalAmount(ItemPickup2D.ItemType.Ammo);
    public int decontamKitsCount => GetTotalAmount(ItemPickup2D.ItemType.DecontamKit);

    private void Awake()
    {
        // Se for a primeira vez que o jogo roda, inicializa com o estado padrão
        if (!hasSavedData)
        {
            persistentSlots = new List<InventorySlotData>();
            persistentHasGun = false;
            hasSavedData = true;
        }
    }

    public bool AddItem(string name, Sprite icon, ItemPickup2D.ItemType type, int amount)
    {
        if (type == ItemPickup2D.ItemType.Gun)
        {
            hasGun = true;
        }

        // 1. Agrupa se já existir no inventário
        foreach (var slot in persistentSlots)
        {
            if (slot.type == type)
            {
                slot.amount += amount;
                return true;
            }
        }

        // 2. Adiciona em um slot livre
        if (persistentSlots.Count < maxSlots)
        {
            InventorySlotData newSlot = new InventorySlotData
            {
                itemName = name,
                icon = icon,
                type = type,
                amount = amount
            };
            persistentSlots.Add(newSlot);
            return true;
        }

        return false;
    }

    private int GetTotalAmount(ItemPickup2D.ItemType type)
    {
        foreach (var slot in persistentSlots)
        {
            if (slot.type == type) return slot.amount;
        }
        return 0;
    }
}