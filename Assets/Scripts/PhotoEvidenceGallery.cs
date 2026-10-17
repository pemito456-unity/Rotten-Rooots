using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Adds the gallery button and gallery overlay to the existing pause menu at runtime.
/// Unlocked evidence persists when MainGame is reloaded during the current play session.
/// </summary>
public class PhotoEvidenceGallery : MonoBehaviour
{
    public static PhotoEvidenceGallery Instance { get; private set; }

    [Header("Galeria")]
    [SerializeField, Min(1)] private int evidenceSlotCount = 10;
    [SerializeField] private Vector2 galleryButtonPosition = new Vector2(0f, -27.4f);

    private static readonly HashSet<int> collectedEvidence = new HashSet<int>();

    private GameObject pausePanel;
    private GameObject galleryPanel;
    private GameObject galleryMenuButton;
    private Button backButton;
    private TextMeshProUGUI countLabel;
    private Image[] evidenceImages;
    private TextMeshProUGUI[] evidenceLabels;
    private Sprite solidSprite;
    private bool initialized;

    public bool IsOpen => galleryPanel != null && galleryPanel.activeSelf;
    public int CollectedCount => CountCollectedEvidence();
    public int EvidenceSlotCount => evidenceSlotCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCollectedEvidence()
    {
        collectedEvidence.Clear();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Initialize(GameObject pausePanelObject)
    {
        if (initialized)
            return;

        pausePanel = pausePanelObject;
        if (pausePanel == null)
        {
            Debug.LogError("PhotoEvidenceGallery: PausePanel não foi atribuído.");
            return;
        }

        evidenceSlotCount = Mathf.Max(1, evidenceSlotCount);
        solidSprite = CreateSolidSprite();
        CreateGalleryMenuButton();
        CreateGalleryPanel();
        initialized = true;
    }

    public bool TryUnlockEvidence(EvidencePhotoTarget target)
    {
        if (target == null)
            return false;

        if (target.evidenceIndex < 1 || target.evidenceIndex > evidenceSlotCount)
        {
            Debug.LogWarning(
                $"A evidência '{target.evidenceName}' usa o índice {target.evidenceIndex}, " +
                $"mas a galeria aceita índices de 1 a {evidenceSlotCount}.",
                target
            );
            return false;
        }

        bool isNewEvidence = collectedEvidence.Add(target.evidenceIndex);
        RefreshGallery();
        return isNewEvidence;
    }

    public static void ResetProgress()
    {
        collectedEvidence.Clear();
    }

    private void CreateGalleryMenuButton()
    {
        GameObject buttonObject = new GameObject(
            "GalleryButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(pausePanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = galleryButtonPosition;
        buttonRect.sizeDelta = new Vector2(160f, 30f);
        buttonRect.localScale = Vector3.one * 1.5f;

        Image background = buttonObject.GetComponent<Image>();
        background.sprite = solidSprite;
        background.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateButtonColors();
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        button.onClick.AddListener(OpenGallery);
        galleryMenuButton = buttonObject;

        // Os botões existentes estavam em navegação explícita sem vizinhos definidos.
        // A navegação automática permite alcançar a Galeria com o direcional do controle.
        Button[] pauseButtons = pausePanel.GetComponentsInChildren<Button>(true);
        foreach (Button pauseButton in pauseButtons)
        {
            if (pauseButton.transform.parent != pausePanel.transform)
                continue;

            Navigation navigation = pauseButton.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            pauseButton.navigation = navigation;
        }

        TextMeshProUGUI label = CreateText(
            buttonObject.transform,
            "Label",
            "Galeria",
            16f,
            Color.black,
            TextAlignmentOptions.Center
        );
        StretchToParent(label.rectTransform, Vector2.zero, Vector2.zero);
        label.raycastTarget = false;
    }

    private void CreateGalleryPanel()
    {
        galleryPanel = new GameObject(
            "PhotoEvidenceGalleryPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        galleryPanel.transform.SetParent(pausePanel.transform, false);

        RectTransform panelRect = galleryPanel.GetComponent<RectTransform>();
        StretchToParent(panelRect, Vector2.zero, Vector2.zero);

        Image panelBackground = galleryPanel.GetComponent<Image>();
        panelBackground.sprite = solidSprite;
        panelBackground.color = new Color(0.025f, 0.035f, 0.03f, 0.98f);
        panelBackground.raycastTarget = true;

        TextMeshProUGUI title = CreateText(
            galleryPanel.transform,
            "GalleryTitle",
            "GALERIA DE EVIDÊNCIAS",
            30f,
            Color.white,
            TextAlignmentOptions.Center
        );
        SetTopCenteredRect(title.rectTransform, new Vector2(0f, -52f), new Vector2(620f, 48f));

        countLabel = CreateText(
            galleryPanel.transform,
            "EvidenceCount",
            "Evidências: 0/10",
            18f,
            new Color(0.78f, 0.86f, 0.78f, 1f),
            TextAlignmentOptions.Center
        );
        SetTopCenteredRect(countLabel.rectTransform, new Vector2(0f, -94f), new Vector2(400f, 32f));

        GameObject gridObject = new GameObject(
            "EvidenceGrid",
            typeof(RectTransform),
            typeof(GridLayoutGroup)
        );
        gridObject.transform.SetParent(galleryPanel.transform, false);
        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        SetCenteredRect(gridRect, new Vector2(0f, -5f), new Vector2(608f, 214f));

        GridLayoutGroup grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(112f, 94f);
        grid.spacing = new Vector2(12f, 12f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        evidenceImages = new Image[evidenceSlotCount];
        evidenceLabels = new TextMeshProUGUI[evidenceSlotCount];
        for (int i = 0; i < evidenceSlotCount; i++)
            CreateEvidenceCard(gridObject.transform, i);

        GameObject backObject = CreateButton(
            galleryPanel.transform,
            "GalleryBackButton",
            "Voltar",
            new Vector2(160f, 34f),
            new Vector2(0f, 42f),
            CloseGallery
        );
        backButton = backObject.GetComponent<Button>();
        galleryPanel.SetActive(false);
    }

    private void CreateEvidenceCard(Transform parent, int slotIndex)
    {
        GameObject card = new GameObject(
            $"EvidenceSlot_{slotIndex + 1}",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        card.transform.SetParent(parent, false);

        Image cardBackground = card.GetComponent<Image>();
        cardBackground.sprite = solidSprite;
        cardBackground.color = new Color(0.1f, 0.12f, 0.1f, 1f);
        cardBackground.raycastTarget = false;

        Outline outline = card.GetComponent<Outline>();
        outline.effectColor = new Color(0.48f, 0.56f, 0.48f, 1f);
        outline.effectDistance = new Vector2(1f, -1f);

        GameObject photoObject = new GameObject(
            "Photo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        photoObject.transform.SetParent(card.transform, false);
        RectTransform photoRect = photoObject.GetComponent<RectTransform>();
        photoRect.anchorMin = new Vector2(0.04f, 0.25f);
        photoRect.anchorMax = new Vector2(0.96f, 0.96f);
        photoRect.offsetMin = Vector2.zero;
        photoRect.offsetMax = Vector2.zero;

        Image photoImage = photoObject.GetComponent<Image>();
        photoImage.sprite = solidSprite;
        photoImage.color = Color.black;
        photoImage.preserveAspect = true;
        photoImage.raycastTarget = false;
        evidenceImages[slotIndex] = photoImage;

        TextMeshProUGUI label = CreateText(
            card.transform,
            "EvidenceLabel",
            "???",
            11f,
            Color.white,
            TextAlignmentOptions.Center
        );
        label.rectTransform.anchorMin = new Vector2(0.02f, 0f);
        label.rectTransform.anchorMax = new Vector2(0.98f, 0.24f);
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        evidenceLabels[slotIndex] = label;
    }

    private GameObject CreateButton(
        Transform parent,
        string objectName,
        string labelText,
        Vector2 size,
        Vector2 position,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image background = buttonObject.GetComponent<Image>();
        background.sprite = solidSprite;
        background.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = CreateButtonColors();
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        button.onClick.AddListener(onClick);

        TextMeshProUGUI label = CreateText(
            buttonObject.transform,
            "Label",
            labelText,
            16f,
            Color.black,
            TextAlignmentOptions.Center
        );
        StretchToParent(label.rectTransform, Vector2.zero, Vector2.zero);
        label.raycastTarget = false;

        return buttonObject;
    }

    private static ColorBlock CreateButtonColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.68f, 0.68f, 0.68f, 1f);
        colors.selectedColor = new Color(0.68f, 0.68f, 0.68f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.fadeDuration = 0.1f;
        return colors;
    }

    public void OpenGallery()
    {
        if (galleryPanel == null)
            return;

        RefreshGallery();
        galleryPanel.SetActive(true);
        SetSelectedObject(backButton != null ? backButton.gameObject : null);
    }

    public void CloseGallery()
    {
        if (galleryPanel == null || !galleryPanel.activeSelf)
            return;

        galleryPanel.SetActive(false);
        SetSelectedObject(galleryMenuButton);
    }

    public void CloseGalleryWithoutChangingSelection()
    {
        if (galleryPanel != null)
            galleryPanel.SetActive(false);
    }

    private void RefreshGallery()
    {
        if (galleryPanel == null)
            return;

        Dictionary<int, EvidencePhotoTarget> targetsByIndex = FindTargetsByIndex();

        if (countLabel != null)
            countLabel.text = $"Evidências: {CollectedCount}/{evidenceSlotCount}";

        for (int i = 0; i < evidenceSlotCount; i++)
        {
            int evidenceIndex = i + 1;
            bool isUnlocked = collectedEvidence.Contains(evidenceIndex);
            EvidencePhotoTarget target;
            targetsByIndex.TryGetValue(evidenceIndex, out target);

            if (evidenceImages[i] == null)
                continue;

            if (!isUnlocked)
            {
                evidenceImages[i].sprite = solidSprite;
                evidenceImages[i].color = Color.black;
                evidenceLabels[i].text = "???";
                continue;
            }

            if (target != null && target.galleryImage != null)
            {
                evidenceImages[i].sprite = target.galleryImage;
                evidenceImages[i].color = Color.white;
                evidenceLabels[i].text = target.evidenceName;
            }
            else
            {
                evidenceImages[i].sprite = solidSprite;
                evidenceImages[i].color = new Color(0.2f, 0.24f, 0.2f, 1f);
                evidenceLabels[i].text = "Imagem pendente";
            }
        }
    }

    private Dictionary<int, EvidencePhotoTarget> FindTargetsByIndex()
    {
        Dictionary<int, EvidencePhotoTarget> targetsByIndex =
            new Dictionary<int, EvidencePhotoTarget>();
        EvidencePhotoTarget[] allTargets = Resources.FindObjectsOfTypeAll<EvidencePhotoTarget>();

        foreach (EvidencePhotoTarget target in allTargets)
        {
            if (target == null || !target.gameObject.scene.IsValid() ||
                !target.gameObject.scene.isLoaded ||
                target.evidenceIndex < 1 || target.evidenceIndex > evidenceSlotCount)
            {
                continue;
            }

            if (!targetsByIndex.ContainsKey(target.evidenceIndex))
                targetsByIndex.Add(target.evidenceIndex, target);
        }

        return targetsByIndex;
    }

    private int CountCollectedEvidence()
    {
        int count = 0;
        foreach (int index in collectedEvidence)
        {
            if (index >= 1 && index <= evidenceSlotCount)
                count++;
        }

        return count;
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "GeneratedPhotoGallerySolidTexture";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply(false, true);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static void StretchToParent(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetTopCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private void SetSelectedObject(GameObject selected)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        if (selected != null && selected.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(selected);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (solidSprite != null)
        {
            Texture2D texture = solidSprite.texture;
            Destroy(solidSprite);
            if (texture != null)
                Destroy(texture);
        }
    }
}
