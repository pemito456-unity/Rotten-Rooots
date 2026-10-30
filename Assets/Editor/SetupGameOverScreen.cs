using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SetupGameOverScreen
{
    private const string ScenePath = "Assets/Scenes/MainGame.unity";

    [InitializeOnLoadMethod]
    private static void SetupAfterScriptsReload()
    {
        EditorApplication.delayCall += SetupIfNeeded;
    }

    [MenuItem("Tools/Rotten Roots/Criar tela de Game Over")]
    private static void SetupIfNeeded()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            return;

        if (GameObject.Find("GameOverCanvas") != null)
            return;

        GameObject manager = GameObject.Find("PauseMenuManager");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (manager == null || player == null)
        {
            Debug.LogError("Game Over: não encontrei PauseMenuManager ou Player na MainGame.");
            return;
        }

        GameOverController gameOver = manager.GetComponent<GameOverController>();
        if (gameOver == null)
            gameOver = Undo.AddComponent<GameOverController>(manager);

        GameObject canvasObject = new GameObject(
            "GameOverCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)
        );
        Undo.RegisterCreatedObjectUndo(canvasObject, "Criar tela de Game Over");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 11000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"
            );

        GameObject panel = CreateRect("GameOverPanel", canvasObject.transform);
        StretchToParent(panel.GetComponent<RectTransform>());
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.015f, 0.025f, 0.02f, 0.94f);
        background.raycastTarget = true;

        CreateText("GameOverTitle", "GAME OVER", panel.transform, font,
            88f, Color.white, new Vector2(0f, 150f), new Vector2(850f, 130f));
        CreateText("GameOverMessage", "A floresta tomou você.", panel.transform, font,
            34f, new Color(0.78f, 0.88f, 0.78f), new Vector2(0f, 55f), new Vector2(850f, 70f));

        Button retryButton = CreateButton("RetryButton", "TENTAR NOVAMENTE",
            panel.transform, font, new Vector2(0f, -70f));
        Button menuButton = CreateButton("MainMenuButton", "MENU PRINCIPAL",
            panel.transform, font, new Vector2(0f, -175f));

        UnityEventTools.AddPersistentListener(retryButton.onClick, gameOver.RestartGame);
        UnityEventTools.AddPersistentListener(menuButton.onClick, gameOver.ReturnToMainMenu);

        SerializedObject serializedGameOver = new SerializedObject(gameOver);
        serializedGameOver.FindProperty("gameOverPanel").objectReferenceValue = panel;
        serializedGameOver.FindProperty("firstSelectedButton").objectReferenceValue = retryButton.gameObject;
        serializedGameOver.FindProperty("deathAnimationFallbackDuration").floatValue = 1f;
        serializedGameOver.FindProperty("deathAnimationTimeout").floatValue = 4f;
        serializedGameOver.FindProperty("menuSceneName").stringValue = "Menu";
        serializedGameOver.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(gameOver);
        EditorUtility.SetDirty(canvasObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Tela de Game Over criada na hierarquia da MainGame.");
    }

    private static GameObject CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static TextMeshProUGUI CreateText(
        string name, string content, Transform parent, TMP_FontAsset font,
        float fontSize, Color color, Vector2 position, Vector2 size)
    {
        GameObject obj = CreateRect(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        string name, string label, Transform parent, TMP_FontAsset font, Vector2 position)
    {
        GameObject obj = CreateRect(name, parent);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(420f, 78f);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.22f, 0.32f, 0.25f, 1f);

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.32f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.48f, 0.4f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.pressedColor = new Color(0.13f, 0.21f, 0.16f, 1f);
        button.colors = colors;

        TextMeshProUGUI text = CreateText("Label", label, obj.transform, font,
            27f, Color.white, Vector2.zero, rect.sizeDelta);
        text.fontStyle = FontStyles.Bold;
        return button;
    }
}
