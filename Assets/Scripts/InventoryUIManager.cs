using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [System.Serializable]
    public class UISlotReference
    {
        public Image iconImage;           
        public TextMeshProUGUI countText; 
    }

    [Header("Hotbar Slots (Em Ordem Crescente 1 ao 5)")]
    public List<UISlotReference> uiSlots = new List<UISlotReference>();

    [Header("Notificação")]
    public GameObject notificationBanner;   
    public TextMeshProUGUI notificationText;

    [Header("Referência do Jogador")]
    public PlayerInventory playerInventory;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        // Garante que o Singleton se reatribua na nova cena
        Instance = this;
    }

    private void Start()
    {
        if (notificationBanner != null) notificationBanner.SetActive(false);

        // Busca o Player da nova cena se a referência estiver vazia no Inspector
        if (playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerInventory = playerObj.GetComponent<PlayerInventory>();
            }
        }

        // Desenha os itens salvos na Hotbar assim que a cena inicia
        UpdateHotbarUI();
    }

    public void UpdateHotbarUI()
    {
        if (playerInventory == null) return;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < playerInventory.slots.Count)
            {
                var slotData = playerInventory.slots[i];
                
                if (uiSlots[i].iconImage != null)
                {
                    uiSlots[i].iconImage.sprite = slotData.icon;
                    uiSlots[i].iconImage.enabled = true;
                }

                if (uiSlots[i].countText != null)
                {
                    uiSlots[i].countText.text = slotData.amount > 1 ? $"x{slotData.amount}" : "";
                }
            }
            else
            {
                if (uiSlots[i].iconImage != null)
                {
                    uiSlots[i].iconImage.sprite = null;
                    uiSlots[i].iconImage.enabled = false;
                }

                if (uiSlots[i].countText != null)
                {
                    uiSlots[i].countText.text = "";
                }
            }
        }
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