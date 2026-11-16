using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Cria os balões como objetos editáveis na hierarquia da MainGame.</summary>
[InitializeOnLoad]
public static class CreateGameplayDialogueUI
{
    private const string CanvasName = "GameplayDialogueCanvas";
    private const string CloudSpritePath = "Assets/Assets/UI/DialogueThoughtCloud.png";
    static CreateGameplayDialogueUI()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += ConfigureOpenMainGameOnce;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.name == "MainGame")
            EditorApplication.delayCall += ConfigureOpenMainGameOnce;
    }

    private static void ConfigureOpenMainGameOnce()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlaying ||
            EditorApplication.isUpdating || SceneManager.GetActiveScene().name != "MainGame" ||
            GameObject.Find(CanvasName) != null)
        {
            return;
        }

        CreateUI();
    }

    [MenuItem("Rotten Roots/Configurar balões de diálogo na MainGame")]
    public static void CreateUI()
    {
        if (SceneManager.GetActiveScene().name != "MainGame")
        {
            Debug.LogError("Abra a cena MainGame antes de configurar os balões de diálogo.");
            return;
        }

        GameObject existing = GameObject.Find(CanvasName);
        if (existing != null)
        {
            Debug.Log("GameplayDialogueCanvas já existe na MainGame; não criei outra cópia.", existing);
            Selection.activeGameObject = existing;
            return;
        }

        Sprite cloudSprite = CreateCloudSpriteAsset();
        if (cloudSprite == null)
        {
            Debug.LogError("Não foi possível criar o sprite do balão de pensamento.");
            return;
        }

        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GameplayDialogueManager)
        );
        Undo.RegisterCreatedObjectUndo(canvasObject, "Criar balões de diálogo");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject speech = CreateBubble(
            canvasObject.transform,
            "JournalistSpeechBubble",
            null,
            new Color(0.015f, 0.015f, 0.015f, 0.96f),
            "Assets/Font/Walnut Regular SDF.asset",
            Color.white,
            30f,
            TextAlignmentOptions.MidlineLeft
        );

        GameObject thought = CreateBubble(
            canvasObject.transform,
            "ThoughtCloudBubble",
            cloudSprite,
            new Color(0.84f, 0.85f, 0.85f, 0.98f),
            "Assets/Pixel_HUD_UI_FreeKit/Font/NotoSerif-Regular SDF.asset",
            new Color(0.14f, 0.14f, 0.14f, 1f),
            29f,
            TextAlignmentOptions.Center
        );

        GameplayDialogueManager manager = canvasObject.GetComponent<GameplayDialogueManager>();
        SerializedObject serialized = new SerializedObject(manager);
        Assign(serialized, "speechBubble", speech);
        Assign(serialized, "speechGroup", speech.GetComponent<CanvasGroup>());
        Assign(serialized, "speechRect", speech.GetComponent<RectTransform>());
        Assign(serialized, "speechText", speech.GetComponentInChildren<TextMeshProUGUI>());
        Assign(serialized, "thoughtBubble", thought);
        Assign(serialized, "thoughtGroup", thought.GetComponent<CanvasGroup>());
        Assign(serialized, "thoughtRect", thought.GetComponent<RectTransform>());
        Assign(serialized, "thoughtText", thought.GetComponentInChildren<TextMeshProUGUI>());
        serialized.ApplyModifiedPropertiesWithoutUndo();

        speech.SetActive(false);
        thought.SetActive(false);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = canvasObject;

        Debug.Log("Balões criados na hierarquia da MainGame e cena salva.", canvasObject);
    }

    private static GameObject CreateBubble(
        Transform parent,
        string objectName,
        Sprite sprite,
        Color backgroundColor,
        string fontPath,
        Color textColor,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject bubble = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup)
        );
        bubble.transform.SetParent(parent, false);

        RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
        bubbleRect.anchorMin = new Vector2(0.5f, 0f);
        bubbleRect.anchorMax = new Vector2(0.5f, 0f);
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        bubbleRect.anchoredPosition = new Vector2(0f, 370f);
        bubbleRect.sizeDelta = new Vector2(700f, 140f);

        Image background = bubble.GetComponent<Image>();
        background.sprite = sprite;
        background.color = backgroundColor;
        background.raycastTarget = false;
        background.type = Image.Type.Simple;

        CanvasGroup group = bubble.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;

        GameObject textObject = new GameObject(
            objectName + "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(bubble.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(640f, 100f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;

        if (text.font == null)
            Debug.LogWarning("Fonte não encontrada para " + objectName + ": " + fontPath);

        return bubble;
    }

    private static Sprite CreateCloudSpriteAsset()
    {
        if (!File.Exists(CloudSpritePath))
        {
            const int width = 512;
            const int height = 256;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "DialogueThoughtCloudTexture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color32[] pixels = new Color32[width * height];
            Vector2[] centers =
            {
                new Vector2(0.50f, 0.46f),
                new Vector2(0.18f, 0.43f),
                new Vector2(0.34f, 0.64f),
                new Vector2(0.52f, 0.69f),
                new Vector2(0.70f, 0.63f),
                new Vector2(0.83f, 0.43f),
                new Vector2(0.29f, 0.31f),
                new Vector2(0.48f, 0.28f),
                new Vector2(0.68f, 0.32f)
            };
            Vector2[] radii =
            {
                new Vector2(0.39f, 0.28f),
                new Vector2(0.17f, 0.22f),
                new Vector2(0.17f, 0.20f),
                new Vector2(0.18f, 0.22f),
                new Vector2(0.18f, 0.20f),
                new Vector2(0.16f, 0.22f),
                new Vector2(0.16f, 0.17f),
                new Vector2(0.17f, 0.18f),
                new Vector2(0.17f, 0.17f)
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 point = new Vector2(x / (float)width, y / (float)height);
                    float nearest = 1f;
                    for (int i = 0; i < centers.Length; i++)
                    {
                        Vector2 delta = (point - centers[i]) / radii[i];
                        nearest = Mathf.Min(nearest, delta.magnitude);
                    }

                    float alpha = Mathf.SmoothStep(1f, 0f, (nearest - 0.94f) / 0.12f);
                    pixels[y * width + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            File.WriteAllBytes(CloudSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(CloudSpritePath, ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer = AssetImporter.GetAtPath(CloudSpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(CloudSpritePath);
    }

    private static void Assign(SerializedObject target, string propertyName, Object value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            target.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
