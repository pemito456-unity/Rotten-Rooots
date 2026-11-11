using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class ConfigurePhotoEvidenceTutorial
{
    private const string ScenePath = "Assets/Scenes/MainGame.unity";
    private const string SetupSessionKey = "RottenRoots.PhotoEvidenceTutorialSetupV4";
    private const string FontPath = "Assets/Font/Walnut Regular SDF.asset";

    static ConfigurePhotoEvidenceTutorial()
    {
        EditorApplication.delayCall += ConfigureIfNeeded;
    }

    private static void ConfigureIfNeeded()
    {
        if (SessionState.GetBool(SetupSessionKey, false) ||
            AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            return;

        ConfigureScene();
    }

    [MenuItem("Tools/Rotten Roots/Configure Photo Evidence Tutorial")]
    public static void ConfigureScene()
    {
        Scene previousScene = SceneManager.GetActiveScene();
        Scene gameScene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !gameScene.IsValid() || !gameScene.isLoaded;

        try
        {
            if (openedHere)
                gameScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            if (gameScene.isDirty && FindTutorial(gameScene) == null)
            {
                Debug.LogWarning(
                    "O tutorial fotográfico não foi inserido porque MainGame tem alterações " +
                    "não salvas. Salve a cena e use Tools > Rotten Roots > Configure Photo Evidence Tutorial."
                );
                return;
            }

            SceneManager.SetActiveScene(gameScene);
            PhotoEvidenceTutorialController existingTutorial = FindTutorial(gameScene);
            if (existingTutorial == null)
                CreateTutorial(gameScene);
            else
            {
                MoveTutorialPanelUp(existingTutorial);
            }

            ConfigureEvidenceGlow(gameScene, FindTutorial(gameScene));
            RemoveOldPhotoMarker(gameScene);

            EditorSceneManager.MarkSceneDirty(gameScene);
            if (!EditorSceneManager.SaveScene(gameScene, ScenePath))
                throw new System.IO.IOException("A Unity não conseguiu salvar a cena MainGame.");

            SessionState.SetBool(SetupSessionKey, true);
            Debug.Log("Tutorial de fotografia configurado na primeira chunk (A2).");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);

            if (openedHere && gameScene.IsValid() && gameScene.isLoaded)
                EditorSceneManager.CloseScene(gameScene, true);
        }
    }

    private static PhotoEvidenceTutorialController FindTutorial(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            PhotoEvidenceTutorialController controller =
                root.GetComponentInChildren<PhotoEvidenceTutorialController>(true);
            if (controller != null)
                return controller;
        }

        return null;
    }

    private static void CreateTutorial(Scene scene)
    {
        Transform hudRoot = FindTransform(scene, "UI_HUD_Canvas");
        if (hudRoot == null)
            throw new InvalidOperationException("Não encontrei UI_HUD_Canvas na cena MainGame.");

        Camera mainCamera = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera candidate in root.GetComponentsInChildren<Camera>(true))
            {
                if (candidate.CompareTag("MainCamera"))
                {
                    mainCamera = candidate;
                    break;
                }
            }

            if (mainCamera != null)
                break;
        }

        if (mainCamera == null)
            throw new InvalidOperationException("Não encontrei a câmera principal na cena MainGame.");

        GameObject canvasObject = new GameObject(
            "PhotoEvidenceTutorialCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );
        canvasObject.transform.SetParent(hudRoot, false);
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCamera;
        canvas.planeDistance = 100f;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = CreatePanel(canvasObject.transform);
        TextMeshProUGUI title = CreateText(
            "TitleText", panel.transform, "REGISTRO DE EVIDÊNCIAS", 34f,
            new Vector2(0f, 70f), new Vector2(1040f, 48f), FontStyles.Bold
        );
        TextMeshProUGUI instruction = CreateText(
            "InstructionText", panel.transform,
            "A câmera registra provas da contaminação. Encontre uma evidência e mire nela.",
            28f, new Vector2(0f, 5f), new Vector2(1040f, 72f), FontStyles.Normal
        );
        TextMeshProUGUI controls = CreateText(
            "ControlsText", panel.transform,
            "Selecione o slot 2: tecla 2 ou Q/E; no controle, LB/RB (L1/R1).", 22f,
            new Vector2(0f, -70f), new Vector2(1040f, 38f), FontStyles.Normal
        );

        PhotoEvidenceTutorialController controller = canvasObject.AddComponent<PhotoEvidenceTutorialController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("firstChunkID").stringValue = "A2";
        serializedController.FindProperty("firstMessageDuration").floatValue = 9f;
        serializedController.FindProperty("successMessageDuration").floatValue = 2.5f;
        serializedController.FindProperty("fadeDuration").floatValue = 0.25f;
        serializedController.FindProperty("tutorialPanel").objectReferenceValue = panel;
        serializedController.FindProperty("tutorialGroup").objectReferenceValue = panel.GetComponent<CanvasGroup>();
        serializedController.FindProperty("titleText").objectReferenceValue = title;
        serializedController.FindProperty("instructionText").objectReferenceValue = instruction;
        serializedController.FindProperty("controlsText").objectReferenceValue = controls;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void ConfigureEvidenceGlow(Scene scene, PhotoEvidenceTutorialController controller)
    {
        if (controller == null)
            return;

        Transform player = FindTransform(scene, "Player");
        if (player == null)
            throw new InvalidOperationException("Não encontrei o objeto Player na cena MainGame.");

        Transform glowTransform = player.Find("EvidencePhotoGlowLight");
        Light2D glow = glowTransform != null ? glowTransform.GetComponent<Light2D>() : null;
        if (glow == null)
        {
            GameObject glowObject = new GameObject("EvidencePhotoGlowLight");
            glowObject.transform.SetParent(player, false);
            glow = glowObject.AddComponent<Light2D>();
        }

        Light2D flashlight = FindTransform(scene, "FlashlightController")?.GetComponent<Light2D>();
        glow.lightType = Light2D.LightType.Point;
        glow.blendStyleIndex = flashlight != null ? flashlight.blendStyleIndex : 0;
        glow.color = flashlight != null ? flashlight.color : new Color(1f, 0.93f, 0.68f, 1f);
        glow.intensity = 0f;
        glow.falloffIntensity = 0.72f;
        glow.pointLightInnerRadius = 0.2f;
        glow.pointLightOuterRadius = 1.05f;
        glow.shadowsEnabled = false;
        glow.enabled = false;

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("evidenceGlowLight").objectReferenceValue = glow;
        serializedController.FindProperty("evidenceGlowIntensity").floatValue = 0.9f;
        serializedController.FindProperty("evidenceGlowTransitionSpeed").floatValue = 5f;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(glow);
    }

    private static void RemoveOldPhotoMarker(Scene scene)
    {
        Transform oldMarker = FindTransform(scene, "PhotoTargetIndicator");
        if (oldMarker != null)
            UnityEngine.Object.DestroyImmediate(oldMarker.gameObject);
    }

    private static void MoveTutorialPanelUp(PhotoEvidenceTutorialController controller)
    {
        Transform panel = controller.transform.Find("PhotoEvidenceTutorialPanel");
        if (panel == null)
            return;

        RectTransform panelRect = panel as RectTransform;
        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(0f, 120f);
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject(
            "PhotoEvidenceTutorialPanel",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 120f);
        rect.sizeDelta = new Vector2(1120f, 220f);

        Image background = panel.GetComponent<Image>();
        background.sprite = BuiltinUiSprite();
        background.type = Image.Type.Sliced;
        background.color = new Color(0.015f, 0.045f, 0.035f, 0.94f);
        background.raycastTarget = false;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        panel.SetActive(false);
        return panel;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string value,
        float fontSize,
        Vector2 position,
        Vector2 size,
        FontStyles style)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.enableWordWrapping = true;
        text.raycastTarget = false;

        TMP_FontAsset gameFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (gameFont != null)
            text.font = gameFont;

        return text;
    }

    private static Sprite BuiltinUiSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static Transform FindTransform(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                    return child;
            }
        }

        return null;
    }
}
