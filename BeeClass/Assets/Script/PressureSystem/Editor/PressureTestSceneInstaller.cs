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
    const string MarkerPath = "Library/PressureTestSceneSetup.v6.done";
    const string FontPath = "Assets/Art/test-Campus/UI/Scence1 UI Font.fontsettings";
    const string SpritePath = "Assets/Art/Ui/PressureFillSprite.asset";
    const string LockSpritePath = "Assets/Art/Ui/PressureUnlockLock.asset";
    const string PointerSpritePath = "Assets/Art/Ui/PressurePointer.asset";

    [InitializeOnLoadMethod]
    static void ScheduleInstall()
    {
        if(!File.Exists(MarkerPath))EditorApplication.delayCall += InstallWhenReady;
    }

    static void InstallWhenReady()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += InstallWhenReady;
            return;
        }
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
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

            PressureContinuousChange continuousChange = FindInScene<PressureContinuousChange>(scene);
            if(!continuousChange)
            {
                var continuousObject = new GameObject("Pressure Continuous Change");
                SceneManager.MoveGameObjectToScene(continuousObject, scene);
                continuousChange = continuousObject.AddComponent<PressureContinuousChange>();
            }
            continuousChange.Configure(system, 5f);
            EditorUtility.SetDirty(continuousChange);

            Canvas canvas = FindInScene<Canvas>(scene);
            if(!canvas)canvas = CreateCanvas(scene);

            CampusWalker walker = FindInScene<CampusWalker>(scene);
            if(walker)
            {
                walker.enableMouseLook = false;
                EditorUtility.SetDirty(walker);
            }

            Sprite pressureSprite = EnsurePressureSprite();
            Sprite lockSprite = EnsureLockSprite();
            Sprite pointerSprite = EnsurePointerSprite();
            Transform existing = canvas.transform.Find("Pressure UI");
            if(!existing)BuildPressureUI(canvas.transform, system, pressureSprite, pointerSprite);
            else ConfigureExistingPressureUI(existing, system, pointerSprite);
            existing = canvas.transform.Find("Pressure UI");
            EnsurePressureContinuousControls(existing, continuousChange, pressureSprite);
            EnsurePressureUnlock(existing, system, pressureSprite, lockSprite);
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

    static Sprite EnsureLockSprite()
    {
        Sprite existing = LoadSprite(LockSpritePath);
        if(existing)return existing;

        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "PressureUnlockLockTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[size * size];
        for(int y = 0; y < size; y++)
        for(int x = 0; x < size; x++)
        {
            float dx = (x - 31.5f) / 17f;
            float dy = (y - 36f) / 20f;
            float innerDx = (x - 31.5f) / 9f;
            float innerDy = (y - 36f) / 12f;
            bool shackle = y >= 29 && dx * dx + dy * dy <= 1f && innerDx * innerDx + innerDy * innerDy >= 1f;
            bool body = x >= 11 && x <= 52 && y >= 7 && y <= 35;
            bool keyHole = (x - 31.5f) * (x - 31.5f) + (y - 21f) * (y - 21f) <= 16f || (x >= 29 && x <= 34 && y >= 12 && y <= 21);
            byte alpha = (shackle || (body && !keyHole)) ? (byte)255 : (byte)0;
            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        AssetDatabase.CreateAsset(texture, LockSpritePath);
        var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "PressureUnlockLock";
        AssetDatabase.AddObjectToAsset(sprite, texture);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(LockSpritePath, ImportAssetOptions.ForceUpdate);
        return LoadSprite(LockSpritePath);
    }

    static Sprite EnsurePointerSprite()
    {
        Sprite existing = LoadSprite(PointerSpritePath);
        if(existing)return existing;

        const int width = 32;
        const int height = 48;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "PressurePointerTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[width * height];
        for(int y = 0; y < height; y++)
        for(int x = 0; x < width; x++)
        {
            bool shaft = x >= 14 && x <= 17 && y >= 16 && y <= 44;
            bool arrowHead = y >= 3 && y <= 18 && Mathf.Abs(x - 15.5f) <= (y - 3) * .8f;
            byte alpha = shaft || arrowHead ? (byte)255 : (byte)0;
            pixels[y * width + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        AssetDatabase.CreateAsset(texture, PointerSpritePath);
        var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "PressurePointer";
        AssetDatabase.AddObjectToAsset(sprite, texture);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(PointerSpritePath, ImportAssetOptions.ForceUpdate);
        return LoadSprite(PointerSpritePath);
    }

    static Sprite LoadPressureSprite()
    {
        return LoadSprite(SpritePath);
    }

    static Sprite LoadSprite(string path)
    {
        foreach(Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if(asset is Sprite sprite)return sprite;
        return null;
    }

    static void EnsurePressureUnlock(Transform pressureRoot, PressureSystem system, Sprite panelSprite, Sprite lockSprite)
    {
        if(!pressureRoot)return;
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if(!font)font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Transform existing = pressureRoot.Find("Pressure Unlock UI");
        if(existing)
        {
            PressureUnlock unlock = existing.GetComponent<PressureUnlock>();
            Button existingButton = existing.GetComponentInChildren<Button>(true);
            Image buttonImage = existingButton ? existingButton.GetComponent<Image>() : null;
            Transform lockTransform = existingButton ? existingButton.transform.Find("Lock Icon") : null;
            Transform labelTransform = existingButton ? existingButton.transform.Find("Text") : null;
            Text label = labelTransform ? labelTransform.GetComponent<Text>() : null;
            if(!unlock)unlock = existing.gameObject.AddComponent<PressureUnlock>();
            unlock.Configure(system, 20f, existingButton, buttonImage, lockTransform ? lockTransform.gameObject : null, label);
            EditorUtility.SetDirty(unlock);
            return;
        }

        var root = Panel("Pressure Unlock UI", pressureRoot, new Color(.055f, .07f, .10f, .94f));
        SetRect(root, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(320, 132), new Vector2(0, 86));
        Image panelImage = root.GetComponent<Image>();panelImage.sprite = panelSprite;panelImage.type = Image.Type.Sliced;panelImage.raycastTarget = false;

        Button button = CreateButton("Pressure Unlock Button", root.transform, "", font, new Color(.16f, .18f, .22f, .96f));
        SetRect(button.gameObject, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(236, 76), Vector2.zero);
        Image buttonImageNew = button.GetComponent<Image>();buttonImageNew.sprite = panelSprite;buttonImageNew.type = Image.Type.Sliced;button.transition = Selectable.Transition.None;button.interactable = false;

        Transform textTransform = button.transform.Find("Text");
        Text buttonLabel = textTransform.GetComponent<Text>();buttonLabel.text = "点击激活";buttonLabel.gameObject.SetActive(false);

        var lockObject = UIObject("Lock Icon", button.transform, typeof(Image));
        SetRect(lockObject, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(54, 54), Vector2.zero);
        Image lockImage = lockObject.GetComponent<Image>();lockImage.sprite = lockSprite;lockImage.color = new Color(.92f, .94f, 1f, 1f);lockImage.preserveAspect = true;lockImage.raycastTarget = false;

        PressureUnlock pressureUnlock = root.AddComponent<PressureUnlock>();
        pressureUnlock.Configure(system, 20f, button, buttonImageNew, lockObject, buttonLabel);
        EditorUtility.SetDirty(button);EditorUtility.SetDirty(pressureUnlock);
    }

    static void EnsurePressureContinuousControls(Transform pressureRoot, PressureContinuousChange continuousChange, Sprite buttonSprite)
    {
        if(!pressureRoot || !continuousChange)return;
        if(pressureRoot.Find("Pressure Continuous Controls"))return;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if(!font)font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var controls = UIObject("Pressure Continuous Controls", pressureRoot);
        SetRect(controls, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(440,64), new Vector2(0,-82));

        Button startButton = CreateButton("Pressure Start Continuous Button", controls.transform, "开始增加", font, new Color(.18f,.58f,.34f,1));
        SetRect(startButton.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(-110,0));
        Image startImage = startButton.GetComponent<Image>();startImage.sprite = buttonSprite;startImage.type = Image.Type.Sliced;
        UnityEventTools.AddPersistentListener(startButton.onClick, continuousChange.StartChanging);

        Button stopButton = CreateButton("Pressure Stop Continuous Button", controls.transform, "停止增加", font, new Color(.62f,.22f,.22f,1));
        SetRect(stopButton.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(110,0));
        Image stopImage = stopButton.GetComponent<Image>();stopImage.sprite = buttonSprite;stopImage.type = Image.Type.Sliced;
        UnityEventTools.AddPersistentListener(stopButton.onClick, continuousChange.StopChanging);

        EditorUtility.SetDirty(startButton);EditorUtility.SetDirty(stopButton);
    }

    static void ConfigureExistingPressureUI(Transform root, PressureSystem system, Sprite pointerSprite)
    {
        Transform pointerTransform = root.Find("Pressure Bar/Pressure Pointer");
        if(!pointerTransform)pointerTransform = root.Find("Pressure Bar/Pressure Difference");
        if(!pointerTransform)pointerTransform = root.Find("Pressure Bar/Fill");
        Image pointer = pointerTransform ? pointerTransform.GetComponent<Image>() : null;
        if(pointer)
        {
            pointer.gameObject.name = "Pressure Pointer";
            pointer.sprite = pointerSprite;
            pointer.type = Image.Type.Simple;
            pointer.color = new Color(1f,.73f,.12f,1);
            pointer.preserveAspect = true;
            pointer.raycastTarget = false;
            RectTransform pointerRect = pointer.rectTransform;
            pointerRect.anchorMin = new Vector2(.5f,.5f);
            pointerRect.anchorMax = new Vector2(.5f,.5f);
            pointerRect.pivot = new Vector2(.5f,.5f);
            pointerRect.anchoredPosition = Vector2.zero;
            pointerRect.sizeDelta = new Vector2(22,32);
            pointer.enabled = true;
            EditorUtility.SetDirty(pointer);
        }

        Transform bar = root.Find("Pressure Bar");
        Transform markerTransform = bar ? bar.Find("Initial Pressure Marker") : null;
        if(markerTransform)Object.DestroyImmediate(markerTransform.gameObject);

        PressureBarUI barUI = root.GetComponent<PressureBarUI>();
        if(!barUI)barUI = root.gameObject.AddComponent<PressureBarUI>();
        barUI.Configure(system, pointer);
        EditorUtility.SetDirty(barUI);
    }

    static void BuildPressureUI(Transform canvas, PressureSystem system, Sprite pressureSprite, Sprite pointerSprite)
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if(!font)font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var root = UIObject("Pressure UI", canvas);
        Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var bar = Panel("Pressure Bar", root.transform, new Color(.09f,.11f,.15f,.94f));
        SetRect(bar, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(360,34), new Vector2(24,-24));

        var pointerObject = Panel("Pressure Pointer", bar.transform, new Color(1f,.73f,.12f,1));
        SetRect(pointerObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(22,32), Vector2.zero);
        var pointer = pointerObject.GetComponent<Image>();pointer.sprite = pointerSprite;pointer.type = Image.Type.Simple;pointer.preserveAspect = true;pointer.raycastTarget = false;

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
        barUI.Configure(system, pointer);
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
