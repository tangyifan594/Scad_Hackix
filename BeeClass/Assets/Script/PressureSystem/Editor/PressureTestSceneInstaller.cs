#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PressureTestSceneInstaller
{
    const string ScenePath = "Assets/Scenes/Test.unity";
    const string MarkerPath = "Library/PressureTestSceneSetup.v2.done";
    const string FontPath = "Assets/Art/test-Campus/UI/Scence1 UI Font.fontsettings";
    const string SpritePath = "Assets/Script/PressureSystem/PressureFillSprite.asset";

    [InitializeOnLoadMethod]
    static void ScheduleInstall()
    {
        if(!File.Exists(MarkerPath))EditorApplication.delayCall += InstallWhenReady;
    }

    static void InstallWhenReady()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += InstallWhenReady;
            return;
        }
        Install();
    }

    [MenuItem("Tools/Codex/Install Pressure Test UI")]
    public static void Install()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PressureSystem system = FindInScene<PressureSystem>(scene);
            if(!system)
            {
                var systemObject = new GameObject("Pressure System");
                SceneManager.MoveGameObjectToScene(systemObject, scene);
                system = systemObject.AddComponent<PressureSystem>();
            }

            Canvas canvas = FindInScene<Canvas>(scene);
            if(!canvas)canvas = CreateCanvas(scene);

            CampusWalker walker = FindInScene<CampusWalker>(scene);
            if(walker)
            {
                walker.enableMouseLook = false;
                EditorUtility.SetDirty(walker);
            }

            Sprite pressureSprite = EnsurePressureSprite();
            Transform existing = canvas.transform.Find("Pressure UI");
            if(!existing)BuildPressureUI(canvas.transform, system, pressureSprite);
            else ConfigureExistingPressureUI(existing, system, pressureSprite);
            EnsureEventSystem(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("Pressure system and test controls installed in Test.unity.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)EditorSceneManager.CloseScene(scene, true);
        }
    }

    static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach(var root in scene.GetRootGameObjects())
        {
            var component = root.GetComponentInChildren<T>(true);
            if(component)return component;
        }
        return null;
    }

    static Canvas CreateCanvas(Scene scene)
    {
        var root = new GameObject("Pressure Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(root, scene);
        var canvas = root.GetComponent<Canvas>();canvas.renderMode = RenderMode.ScreenSpaceOverlay;canvas.sortingOrder = 110;
        var scaler = root.GetComponent<CanvasScaler>();scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution = new Vector2(1920,1080);scaler.matchWidthOrHeight = .5f;
        return canvas;
    }

    static Sprite EnsurePressureSprite()
    {
        Sprite existing = LoadPressureSprite();
        if(existing)return existing;

        const int size = 32;
        const float radius = 8f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "PressureFillTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[size * size];
        for(int y = 0; y < size; y++)
        for(int x = 0; x < size; x++)
        {
            float cornerX = x < radius ? radius - x : x >= size - radius ? x - (size - radius - 1) : 0f;
            float cornerY = y < radius ? radius - y : y >= size - radius ? y - (size - radius - 1) : 0f;
            byte alpha = cornerX * cornerX + cornerY * cornerY <= radius * radius ? (byte)255 : (byte)0;
            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        AssetDatabase.CreateAsset(texture, SpritePath);
        var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        sprite.name = "PressureFillSprite";
        AssetDatabase.AddObjectToAsset(sprite, texture);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
        return LoadPressureSprite();
    }

    static Sprite LoadPressureSprite()
    {
        foreach(Object asset in AssetDatabase.LoadAllAssetsAtPath(SpritePath))
            if(asset is Sprite sprite)return sprite;
        return null;
    }

    static void ConfigureExistingPressureUI(Transform root, PressureSystem system, Sprite pressureSprite)
    {
        Transform fillTransform = root.Find("Pressure Bar/Fill");
        Image fill = fillTransform ? fillTransform.GetComponent<Image>() : null;
        if(fill)
        {
            fill.sprite = pressureSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.preserveAspect = false;
            fill.raycastTarget = false;
            fill.fillAmount = system.NormalizedPressure;
            EditorUtility.SetDirty(fill);
        }

        PressureBarUI barUI = root.GetComponent<PressureBarUI>();
        if(!barUI)barUI = root.gameObject.AddComponent<PressureBarUI>();
        barUI.Configure(system, fill);
        EditorUtility.SetDirty(barUI);
    }

    static void BuildPressureUI(Transform canvas, PressureSystem system, Sprite pressureSprite)
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if(!font)font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var root = UIObject("Pressure UI", canvas);
        Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var bar = Panel("Pressure Bar", root.transform, new Color(.09f,.11f,.15f,.94f));
        SetRect(bar, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(360,34), new Vector2(24,-24));

        var fillObject = Panel("Fill", bar.transform, new Color(.92f,.42f,.22f,1));
        Stretch(fillObject, Vector2.zero, Vector2.one, new Vector2(5,5), new Vector2(-5,-5));
        var fill = fillObject.GetComponent<Image>();fill.sprite = pressureSprite;fill.type = Image.Type.Filled;fill.fillMethod = Image.FillMethod.Horizontal;fill.fillOrigin = 0;fill.fillAmount = system.NormalizedPressure;fill.raycastTarget = false;

        var controls = UIObject("Pressure Test Controls", root.transform);
        SetRect(controls, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(440,64), Vector2.zero);

        Button lowerButton = CreateButton("Pressure Lower Button", controls.transform, "压力 -10", font, new Color(.18f,.43f,.62f,1));
        SetRect(lowerButton.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(-110,0));
        var lowerTrigger = lowerButton.gameObject.AddComponent<PressureChangeTrigger>();
        lowerTrigger.Configure(system, PressureChangeTrigger.Direction.Lower, 10f);
        UnityEventTools.AddPersistentListener(lowerButton.onClick, lowerTrigger.Trigger);

        Button higherButton = CreateButton("Pressure Higher Button", controls.transform, "压力 +10", font, new Color(.76f,.28f,.20f,1));
        SetRect(higherButton.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(110,0));
        var higherTrigger = higherButton.gameObject.AddComponent<PressureChangeTrigger>();
        higherTrigger.Configure(system, PressureChangeTrigger.Direction.Higher, 10f);
        UnityEventTools.AddPersistentListener(higherButton.onClick, higherTrigger.Trigger);

        var barUI = root.AddComponent<PressureBarUI>();
        barUI.Configure(system, fill);
        EditorUtility.SetDirty(lowerTrigger);EditorUtility.SetDirty(higherTrigger);EditorUtility.SetDirty(barUI);
    }

    static void EnsureEventSystem(Scene scene)
    {
        if(FindInScene<EventSystem>(scene))return;
        var eventSystem = new GameObject("Pressure EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
    }

    static GameObject UIObject(string name, Transform parent, params System.Type[] components)
    {
        var item = new GameObject(name, typeof(RectTransform));item.transform.SetParent(parent, false);
        foreach(var component in components)item.AddComponent(component);
        return item;
    }

    static GameObject Panel(string name, Transform parent, Color color)
    {
        var item = UIObject(name, parent, typeof(Image));item.GetComponent<Image>().color = color;return item;
    }

    static Button CreateButton(string name, Transform parent, string label, Font font, Color color)
    {
        var item = UIObject(name, parent, typeof(Image), typeof(Button));
        var image = item.GetComponent<Image>();image.color = color;
        var button = item.GetComponent<Button>();button.targetGraphic = image;
        var textObject = UIObject("Text", item.transform, typeof(Text));Stretch(textObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var text = textObject.GetComponent<Text>();text.font = font;text.text = label;text.fontSize = 22;text.alignment = TextAnchor.MiddleCenter;text.color = Color.white;text.raycastTarget = false;
        return button;
    }

    static void SetRect(GameObject item, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position)
    {
        var rect = item.GetComponent<RectTransform>();rect.anchorMin = anchorMin;rect.anchorMax = anchorMax;rect.pivot = pivot;rect.sizeDelta = size;rect.anchoredPosition = position;
    }

    static void Stretch(GameObject item, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rect = item.GetComponent<RectTransform>();rect.anchorMin = anchorMin;rect.anchorMax = anchorMax;rect.offsetMin = offsetMin;rect.offsetMax = offsetMax;
    }
}
#endif
