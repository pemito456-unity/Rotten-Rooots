using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Recursos Coletados")]
    public int ammoCount = 0;
    public int decontamKitsCount = 0;

    [Header("Equipamento")]
    public bool hasGun = true; // No J1, o jogador já começa equipado com a arma

    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        Debug.Log($"[Inventário] +{amount} Munição. Total: {ammoCount}");
    }

    public bool UseAmmo(int amount = 1)
    {
        if (ammoCount >= amount)
        {
            ammoCount -= amount;
            Debug.Log($"[Inventário] -{amount} Munição. Restante: {ammoCount}");
            return true;
        }
        Debug.Log("[Inventário] Sem munição!");
        return false;
    }

    public void AddDecontamKit(int amount)
    {
        decontamKitsCount += amount;
        Debug.Log($"[Inventário] +{amount} Kit Eco. Total: {decontamKitsCount}");
    }

    public bool UseDecontamKit(int amount = 1)
    {
        if (decontamKitsCount >= amount)
        {
            decontamKitsCount -= amount;
            Debug.Log($"[Inventário] -{amount} Kit Eco. Restante: {decontamKitsCount}");
            return true;
        }
        Debug.Log("[Inventário] Sem Kits de Descontaminação!");
        return false;
    }
}