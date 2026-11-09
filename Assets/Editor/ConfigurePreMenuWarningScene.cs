using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ConfigurePreMenuWarningScene
{
    private const string ScenePath = "Assets/Scenes/PreMenuWarning.unity";
    private const string SetupSessionKey = "RottenRoots.PreMenuWarningSetupV5";

    static ConfigurePreMenuWarningScene()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    private static void ConfigureIfNeeded()
    {
        if (SessionState.GetBool(SetupSessionKey, false))
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            return;

        SessionState.SetBool(SetupSessionKey, true);
        ConfigureScene();
    }

    [MenuItem("Tools/Rotten Roots/Configure PreMenuWarning Scene")]
    public static void ConfigureScene()
    {
        Scene previousScene = SceneManager.GetActiveScene();
        Scene warningScene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !warningScene.IsValid() || !warningScene.isLoaded;

        try
        {
            if (openedHere)
                warningScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            GameObject existingCanvas = FindWarningCanvas(warningScene);
            if (existingCanvas != null)
            {
                SceneManager.SetActiveScene(warningScene);
                UpgradeExistingCanvas(existingCanvas);
                EditorSceneManager.MarkSceneDirty(warningScene);
                EditorSceneManager.SaveScene(warningScene, ScenePath);
                EnsureSceneInBuildSettings();
                return;
            }

            SceneManager.SetActiveScene(warningScene);
            CreateWarningUI();

            if (!EditorSceneManager.SaveScene(warningScene, ScenePath))
                throw new System.IO.IOException("A Unity não conseguiu salvar a cena PreMenuWarning.");

            EnsureSceneInBuildSettings();
            Debug.Log("PreMenuWarning configurada: aviso com fade e transição para Menu.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);

            if (openedHere && warningScene.IsValid() && warningScene.isLoaded)
                EditorSceneManager.CloseScene(warningScene, true);
        }
    }

    private static GameObject FindWarningCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "PreMenuWarningCanvas")
                return root;
        }

        return null;
    }

    private static void UpgradeExistingCanvas(GameObject canvasObject)
    {
        CanvasGroup canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = canvasObject.AddComponent<CanvasGroup>();

        TextMeshProUGUI notice = canvasObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (notice == null)
            return;

        RectTransform textRect = notice.rectTransform;
        textRect.anchoredPosition = new Vector2(0f, -90f);
        textRect.sizeDelta = new Vector2(1600f, 140f);
        CreateHeadphoneIconIfNeeded(notice.gameObject);

        CanvasGroup noticeGroup = notice.GetComponent<CanvasGroup>();
        if (noticeGroup == null)
            noticeGroup = notice.gameObject.AddComponent<CanvasGroup>();

        PreMenuWarningController controller = canvasObject.GetComponent<PreMenuWarningController>();
        if (controller == null)
            controller = canvasObject.AddComponent<PreMenuWarningController>();

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedController.FindProperty("noticeGroup").objectReferenceValue = noticeGroup;
        serializedController.FindProperty("menuSceneName").stringValue = "Menu";
        serializedController.FindProperty("fadeInDuration").floatValue = 0.6f;
        serializedController.FindProperty("displayDuration").floatValue = 2.5f;
        serializedController.FindProperty("contentFadeOutDuration").floatValue = 0.6f;
        serializedController.FindProperty("fadeOutDuration").floatValue = 0.8f;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener listener in listeners)
                listener.enabled = false;
        }
    }

    private static void CreateWarningUI()
    {
        GameObject canvasObject = new GameObject(
            "PreMenuWarningCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0f;

        CanvasGroup canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject backgroundObject = new GameObject(
            "BlackBackground",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = Color.black;
        background.raycastTarget = false;

        GameObject textObject = new GameObject(
            "HeadphoneNoticeText",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(CanvasGroup),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(backgroundObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, -90f);
        textRect.sizeDelta = new Vector2(1600f, 140f);

        CreateHeadphoneIconIfNeeded(textObject);

        TextMeshProUGUI notice = textObject.GetComponent<TextMeshProUGUI>();
        CanvasGroup noticeGroup = textObject.GetComponent<CanvasGroup>();
        noticeGroup.alpha = 1f;
        notice.text = "ESTE JOGO É MELHOR APROVEITADO COM FONES DE OUVIDO";
        notice.color = Color.white;
        notice.alignment = TextAlignmentOptions.Center;
        notice.fontSize = 38f;
        notice.enableAutoSizing = true;
        notice.fontSizeMin = 28f;
        notice.fontSizeMax = 38f;
        notice.raycastTarget = false;

        TMP_FontAsset gameFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/Font/Walnut Regular SDF.asset"
        );
        if (gameFont != null)
            notice.font = gameFont;

        PreMenuWarningController controller = canvasObject.AddComponent<PreMenuWarningController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedController.FindProperty("noticeGroup").objectReferenceValue = noticeGroup;
        serializedController.FindProperty("menuSceneName").stringValue = "Menu";
        serializedController.FindProperty("fadeInDuration").floatValue = 0.6f;
        serializedController.FindProperty("displayDuration").floatValue = 2.5f;
        serializedController.FindProperty("contentFadeOutDuration").floatValue = 0.6f;
        serializedController.FindProperty("fadeOutDuration").floatValue = 0.8f;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener listener in listeners)
                listener.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void CreateHeadphoneIconIfNeeded(GameObject noticeObject)
    {
        Transform existingIcon = noticeObject.transform.Find("HeadphoneIcon");
        if (existingIcon != null)
            return;

        GameObject icon = new GameObject("HeadphoneIcon", typeof(RectTransform));
        icon.transform.SetParent(noticeObject.transform, false);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 145f);
        iconRect.sizeDelta = new Vector2(150f, 110f);

        Sprite whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        AddIconPart(icon.transform, "Headband", whiteSprite, new Vector2(0f, 31f), new Vector2(78f, 8f));
        AddIconPart(icon.transform, "LeftBand", whiteSprite, new Vector2(-36f, 12f), new Vector2(8f, 34f));
        AddIconPart(icon.transform, "RightBand", whiteSprite, new Vector2(36f, 12f), new Vector2(8f, 34f));
        AddIconPart(icon.transform, "LeftEarCup", whiteSprite, new Vector2(-39f, -15f), new Vector2(22f, 43f));
        AddIconPart(icon.transform, "RightEarCup", whiteSprite, new Vector2(39f, -15f), new Vector2(22f, 43f));
    }

    private static void AddIconPart(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject part = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        part.transform.SetParent(parent, false);
        RectTransform rect = part.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = part.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private static void EnsureSceneInBuildSettings()
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < currentScenes.Length; i++)
        {
            if (currentScenes[i].path == ScenePath)
            {
                if (!currentScenes[i].enabled)
                {
                    currentScenes[i] = new EditorBuildSettingsScene(ScenePath, true);
                    EditorBuildSettings.scenes = currentScenes;
                }
                return;
            }
        }

        var updatedScenes = new List<EditorBuildSettingsScene>(currentScenes);
        int menuIndex = updatedScenes.FindIndex(scene => scene.path == "Assets/Scenes/Menu.unity");
        if (menuIndex < 0)
            menuIndex = 0;

        updatedScenes.Insert(menuIndex, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = updatedScenes.ToArray();
    }
}
