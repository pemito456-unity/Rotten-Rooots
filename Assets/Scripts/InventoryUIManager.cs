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
        [System.NonSerialized] public GameObject selectionBorder;
    }

    [Header("Slots da hotbar")]
    public List<UISlotReference> uiSlots = new List<UISlotReference>();

    [Header("Notificação")]
    public GameObject notificationBanner;
    public TextMeshProUGUI notificationText;

    [Header("Referência do jogador")]
    public PlayerInventory playerInventory;

    private Coroutine notificationCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (notificationBanner != null)
            notificationBanner.SetActive(false);

        if (playerInventory == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                playerInventory = playerObject.GetComponent<PlayerInventory>();
        }

        UpdateHotbarUI();
    }

    public void UpdateHotbarUI()
    {
        if (playerInventory == null)
            return;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            UpdateSlotSelectionBorder(i);

            if (i < playerInventory.slots.Count)
            {
                InventorySlotData slotData = playerInventory.slots[i];

                if (uiSlots[i].iconImage != null)
                {
                    uiSlots[i].iconImage.sprite = slotData.icon;
                    uiSlots[i].iconImage.enabled = slotData.icon != null;
                }

                if (slotData.type == ItemPickup2D.ItemType.Gun &&
                    uiSlots[i].countText == null &&
                    uiSlots[i].iconImage != null)
                {
                    uiSlots[i].countText = CreateAmmoTextForSlot(i, uiSlots[i].iconImage);
                }

                if (uiSlots[i].countText != null)
                {
                    if (slotData.type == ItemPickup2D.ItemType.Gun)
                    {
                        // Número simples dentro do slot da pistola, como em Minecraft.
                        uiSlots[i].countText.text = slotData.ammoCount.ToString();
                    }
                    else
                    {
                        uiSlots[i].countText.text =
                            slotData.amount > 1 ? $"x{slotData.amount}" : "";
                    }
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
                    uiSlots[i].countText.text = "";
            }
        }
    }

    private void UpdateSlotSelectionBorder(int slotIndex)
    {
        UISlotReference slot = uiSlots[slotIndex];
        if (slot.iconImage == null)
            return;

        if (slot.selectionBorder == null)
            slot.selectionBorder = CreateSelectionBorder(slotIndex, slot.iconImage.transform);

        slot.selectionBorder.SetActive(slotIndex == playerInventory.selectedSlotIndex);
    }

    private GameObject CreateSelectionBorder(int slotIndex, Transform slotTransform)
    {
        GameObject border = new GameObject(
            $"SelectionBorder_Slot{slotIndex + 1}",
            typeof(RectTransform)
        );
        border.transform.SetParent(slotTransform, false);

        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        Sprite whiteSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f)
        );

        CreateBorderEdge(border.transform, "Top", whiteSprite,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 3f));
        CreateBorderEdge(border.transform, "Bottom", whiteSprite,
            Vector2.zero, new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 3f));
        CreateBorderEdge(border.transform, "Left", whiteSprite,
            Vector2.zero, new Vector2(0f, 1f),
            new Vector2(0f, 0.5f), Vector2.zero, new Vector2(3f, 0f));
        CreateBorderEdge(border.transform, "Right", whiteSprite,
            new Vector2(1f, 0f), Vector2.one,
            new Vector2(1f, 0.5f), Vector2.zero, new Vector2(3f, 0f));

        // Mantém a moldura por cima do ícone e do contador de munição.
        border.transform.SetAsLastSibling();
        border.SetActive(false);
        return border;
    }

    private void CreateBorderEdge(
        Transform parent,
        string edgeName,
        Sprite whiteSprite,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject edge = new GameObject(
            edgeName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        edge.transform.SetParent(parent, false);

        RectTransform edgeRect = edge.GetComponent<RectTransform>();
        edgeRect.anchorMin = anchorMin;
        edgeRect.anchorMax = anchorMax;
        edgeRect.pivot = pivot;
        edgeRect.anchoredPosition = anchoredPosition;
        edgeRect.sizeDelta = sizeDelta;

        Image edgeImage = edge.GetComponent<Image>();
        edgeImage.sprite = whiteSprite;
        edgeImage.color = Color.white;
        edgeImage.raycastTarget = false;
    }

    private TextMeshProUGUI CreateAmmoTextForSlot(int slotIndex, Image slotIcon)
    {
        GameObject textObject = new GameObject(
            $"AmmoCount_Slot{slotIndex + 1}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(slotIcon.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(1f, 0f);
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.pivot = new Vector2(1f, 0f);
        textRect.anchoredPosition = new Vector2(-2f, 1f);
        textRect.sizeDelta = new Vector2(42f, 24f);

        TextMeshProUGUI ammoText = textObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            ammoText.font = TMP_Settings.defaultFontAsset;
        ammoText.fontSize = 21f;
        ammoText.fontStyle = FontStyles.Bold;
        ammoText.color = Color.white;
        ammoText.alignment = TextAlignmentOptions.BottomRight;
        ammoText.enableWordWrapping = false;
        ammoText.raycastTarget = false;

        UnityEngine.UI.Outline outline = textObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(1f, -1f);

        return ammoText;
    }

    public void ShowCollectionNotice(string itemName, int amount)
    {
        if (notificationBanner == null || notificationText == null)
            return;

        notificationText.text = $"+{amount} {itemName}";

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(NotificationSequence());
    }

    private IEnumerator NotificationSequence()
    {
        notificationBanner.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationBanner.SetActive(false);
    }
}
