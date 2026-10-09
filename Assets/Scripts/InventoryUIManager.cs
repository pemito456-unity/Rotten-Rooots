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
        [System.NonSerialized] public Image batteryBarFill;
    }

    [Header("Slots da hotbar")]
    public List<UISlotReference> uiSlots = new List<UISlotReference>();

    [Header("Notificação")]
    public GameObject notificationBanner;
    public TextMeshProUGUI notificationText;

    [Header("Referência do jogador")]
    public PlayerInventory playerInventory;

    private Coroutine notificationCoroutine;
    private UnityEngine.Canvas ammoNoticeCanvas;
    private Sprite solidSprite;

    private void Awake()
    {
        Instance = this;
        CreateAmmoNoticeCanvas();
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

    private void Update()
    {
        UpdateFlashlightBatteryBars();
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

                UpdateSlotBatteryBar(i, slotData);
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

                if (uiSlots[i].batteryBarFill != null)
                    uiSlots[i].batteryBarFill.transform.parent.gameObject.SetActive(false);
            }

            // Mantém a borda de seleção acima das barras criadas dinamicamente.
            UpdateSlotSelectionBorder(i);
        }
    }

    private void UpdateFlashlightBatteryBars()
    {
        if (playerInventory == null)
            return;

        for (int i = 0; i < playerInventory.slots.Count && i < uiSlots.Count; i++)
        {
            InventorySlotData slotData = playerInventory.slots[i];
            if (slotData.isFlashlight)
                UpdateSlotBatteryBar(i, slotData);
        }
    }

    private void UpdateSlotBatteryBar(int slotIndex, InventorySlotData slotData)
    {
        UISlotReference uiSlot = uiSlots[slotIndex];
        bool isFlashlightSlot = slotData.isFlashlight;

        if (!isFlashlightSlot)
        {
            if (uiSlot.batteryBarFill != null)
                uiSlot.batteryBarFill.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (uiSlot.iconImage == null)
            return;

        if (uiSlot.batteryBarFill == null)
            CreateBatteryBar(slotIndex, uiSlot);

        FlashlightController flashlight = playerInventory.FlashlightController;
        if (flashlight != null)
            uiSlot.batteryBarFill.fillAmount = flashlight.BatteryNormalized;
    }

    private void CreateBatteryBar(int slotIndex, UISlotReference uiSlot)
    {
        if (solidSprite == null)
        {
            solidSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f)
            );
        }

        GameObject background = new GameObject(
            $"FlashlightBattery_Slot{slotIndex + 1}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        background.transform.SetParent(uiSlot.iconImage.transform, false);

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0f);
        backgroundRect.anchorMax = new Vector2(1f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.anchoredPosition = new Vector2(0f, 3f);
        backgroundRect.sizeDelta = new Vector2(-8f, 6f);

        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.sprite = solidSprite;
        backgroundImage.color = new Color(0f, 0f, 0f, 0.9f);
        backgroundImage.raycastTarget = false;

        GameObject fill = new GameObject(
            "Fill",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        fill.transform.SetParent(background.transform, false);

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.one;
        fillRect.offsetMax = -Vector2.one;

        uiSlot.batteryBarFill = fill.GetComponent<Image>();
        uiSlot.batteryBarFill.sprite = solidSprite;
        uiSlot.batteryBarFill.type = Image.Type.Filled;
        uiSlot.batteryBarFill.fillMethod = Image.FillMethod.Horizontal;
        uiSlot.batteryBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        uiSlot.batteryBarFill.fillAmount = 1f;
        uiSlot.batteryBarFill.color = new Color(0.45f, 0.9f, 0.25f, 1f);
        uiSlot.batteryBarFill.raycastTarget = false;
    }

    private void UpdateSlotSelectionBorder(int slotIndex)
    {
        UISlotReference slot = uiSlots[slotIndex];
        if (slot.iconImage == null)
            return;

        if (slot.selectionBorder == null)
            slot.selectionBorder = CreateSelectionBorder(slotIndex, slot.iconImage.transform);

        slot.selectionBorder.transform.SetAsLastSibling();
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

    public void ShowAmmoPickupNotice(Vector3 worldPosition, int amount)
    {
        if (ammoNoticeCanvas == null)
            CreateAmmoNoticeCanvas();

        Camera mainCamera = Camera.main;
        if (ammoNoticeCanvas == null || mainCamera == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z < 0f)
            return;

        RectTransform canvasRect = ammoNoticeCanvas.transform as RectTransform;
        if (canvasRect == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, null, out Vector2 localPosition))
        {
            return;
        }

        GameObject noticeObject = new GameObject(
            "AmmoPickupNotice",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(UnityEngine.UI.Outline)
        );
        noticeObject.transform.SetParent(ammoNoticeCanvas.transform, false);

        RectTransform noticeRect = noticeObject.GetComponent<RectTransform>();
        noticeRect.anchorMin = new Vector2(0.5f, 0.5f);
        noticeRect.anchorMax = new Vector2(0.5f, 0.5f);
        noticeRect.pivot = new Vector2(0.5f, 0.5f);
        noticeRect.anchoredPosition = localPosition + Vector2.up * 22f;
        noticeRect.sizeDelta = new Vector2(240f, 42f);

        TextMeshProUGUI noticeText = noticeObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            noticeText.font = TMP_Settings.defaultFontAsset;
        noticeText.text = $"+{Mathf.Max(1, amount)} MUNIÇÃO";
        noticeText.fontSize = 16f;
        noticeText.fontStyle = FontStyles.Bold;
        noticeText.color = Color.white;
        noticeText.alignment = TextAlignmentOptions.Center;
        noticeText.enableWordWrapping = false;
        noticeText.raycastTarget = false;

        UnityEngine.UI.Outline outline = noticeObject.GetComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        StartCoroutine(AnimateAmmoPickupNotice(noticeText, noticeRect));
    }

    private void CreateAmmoNoticeCanvas()
    {
        if (ammoNoticeCanvas != null)
            return;

        GameObject canvasObject = new GameObject(
            "AmmoPickupNoticeCanvas",
            typeof(RectTransform),
            typeof(UnityEngine.Canvas),
            typeof(UnityEngine.UI.CanvasScaler)
        );

        ammoNoticeCanvas = canvasObject.GetComponent<UnityEngine.Canvas>();
        ammoNoticeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ammoNoticeCanvas.overrideSorting = true;
        ammoNoticeCanvas.sortingOrder = 100;

        UnityEngine.UI.CanvasScaler scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private IEnumerator AnimateAmmoPickupNotice(
        TextMeshProUGUI noticeText,
        RectTransform noticeRect)
    {
        const float duration = 1f;
        const float riseDistance = 28f;
        Vector2 startPosition = noticeRect.anchoredPosition;
        Color startColor = noticeText.color;
        float elapsed = 0f;

        while (elapsed < duration && noticeText != null && noticeRect != null)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            noticeRect.anchoredPosition = startPosition + Vector2.up * (riseDistance * t);

            Color color = startColor;
            color.a = 1f - t;
            noticeText.color = color;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (noticeText != null)
            Destroy(noticeText.gameObject);
    }

    private IEnumerator NotificationSequence()
    {
        notificationBanner.SetActive(true);
        yield return new WaitForSeconds(2f);
        notificationBanner.SetActive(false);
    }
}
