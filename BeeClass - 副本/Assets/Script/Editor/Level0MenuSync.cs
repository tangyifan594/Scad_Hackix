#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Level0MenuSync
{
    const string ScenePath = "Assets/Scenes/Level0-UI.unity";
    const string Marker = "Library/Level0MenuSync.v3.done";

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += ApplyWhenReady;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += ApplyWhenReady;
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode) { ApplyWhenReady(); }

    static void ApplyWhenReady()
    {
        if (System.IO.File.Exists(Marker) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += ApplyWhenReady;
            return;
        }
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded) Apply();
    }

    [MenuItem("Tools/Bee Class/Sync Level0 Menu")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded) return;
        System.IO.Directory.CreateDirectory("Assets/_Recovery");
        string backup = "Assets/_Recovery/Level0-before-menu-sync-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".unity";
        if (!EditorSceneManager.SaveScene(scene, backup, true))
        {
            Debug.LogError("Could not back up Level0; menu sync was skipped.");
            return;
        }

        CampusMainMenu menu = null;
        Canvas canvas = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (!menu) menu = root.GetComponentInChildren<CampusMainMenu>(true);
            foreach (var candidate in root.GetComponentsInChildren<Canvas>(true))
                if (candidate.name == "Main Menu Canvas" || candidate.name == "Scence1 Canvas") canvas = candidate;
        }
        if (!menu)
        {
            var obj = new GameObject("Level0 Main Menu");
            SceneManager.MoveGameObjectToScene(obj, scene);
            menu = obj.AddComponent<CampusMainMenu>();
        }
        if (!canvas)
        {
            var obj = new GameObject("Main Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(obj, scene);
            canvas = obj.GetComponent<Canvas>();
        }
        canvas.name = "Main Menu Canvas";
        canvas.gameObject.SetActive(true);
        canvas.transform.localScale = Vector3.one;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        if (!canvas.GetComponent<GraphicRaycaster>()) canvas.gameObject.AddComponent<GraphicRaycaster>();
        var hud = canvas.GetComponent<Scence1CanvasUI>();
        if (hud) hud.enabled = false;
        foreach (Transform child in canvas.transform)
            if (child.name != "Start Button" && child.name != "Quit Button" && child.name != "Bee class Title") child.gameObject.SetActive(false);

        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/UIFonts/BeeClassChinese.ttf");
        if (!font)
        {
            Debug.LogWarning("Waiting for BeeClassChinese.ttf to finish importing.");
            EditorApplication.delayCall += ApplyWhenReady;
            return;
        }
        var start = Button(canvas.transform, "Start Button", "开始游戏", -270, new Color(.12f, .42f, .9f), font);
        var quit = Button(canvas.transform, "Quit Button", "结束游戏", 270, new Color(.88f, .18f, .22f), font);
        var title = Text(canvas.transform, "Bee class Title", "Bee class", font, 76);
        Rect(title.rectTransform, Vector2.zero, new Vector2(1000, 140));
        title.fontStyle = FontStyle.Bold;
        var serializedMenu = new SerializedObject(menu);
        serializedMenu.FindProperty("startButton").objectReferenceValue = start;
        serializedMenu.FindProperty("quitButton").objectReferenceValue = quit;
        serializedMenu.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            System.IO.File.WriteAllText(Marker, backup);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("Level0 menu synced to the open scene: blue entry, red exit, Bee class title. Backup: " + backup);
        }
    }

    static Button Button(Transform parent, string name, string label, float x, Color color, Font font)
    {
        var existing = parent.Find(name);
        var obj = existing ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.SetActive(true);
        Rect(obj.GetComponent<RectTransform>(), new Vector2(x, -140), new Vector2(360, 90));
        var image = obj.GetComponent<Image>();
        if (!image) image = obj.AddComponent<Image>();
        image.color = color;
        var button = obj.GetComponent<Button>();
        if (!button) button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        var text = Text(obj.transform, "Text", label, font, 32);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    static Text Text(Transform parent, string name, string label, Font font, int size)
    {
        var existing = parent.Find(name);
        var obj = existing ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.SetActive(true);
        var text = obj.GetComponent<Text>();
        if (!text) text = obj.AddComponent<Text>();
        text.text = label; text.font = font; text.fontSize = size;
        text.color = Color.white; text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
        return text;
    }

    static void Rect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one;
    }
}
#endif
