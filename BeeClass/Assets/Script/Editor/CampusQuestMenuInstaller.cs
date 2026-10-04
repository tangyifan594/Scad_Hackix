#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CampusQuestMenuInstaller
{
    const string Path = "Assets/Scenes/Level0-UI.unity";
    const string Marker = "Library/CampusQuestMenu.done";
    [InitializeOnLoadMethod] static void Schedule() { EditorApplication.delayCall += Ready; }
    static void Ready()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Ready; return; }
        Install();
    }
    [MenuItem("Tools/Bee Class/Install Campus Quest Menu")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Menu/CampusQuestMenu.png");
        if (!texture) { EditorApplication.delayCall += Ready; return; }
        var scene = SceneManager.GetSceneByPath(Path);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
        Directory.CreateDirectory("Archive/CampusQuestMenu");
        if (!EditorSceneManager.SaveScene(scene, "Archive/CampusQuestMenu/Level0-before-menu.unity", true)) return;
        var menu = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<CampusMainMenu>(true)).FirstOrDefault();
        var canvas = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).FirstOrDefault(c => c.name == "Main Menu Canvas");
        if (!menu || !canvas) { Debug.LogError("Level0 menu or canvas missing."); return; }
        foreach (Transform child in canvas.transform.Cast<Transform>().ToArray()) Object.DestroyImmediate(child.gameObject);
        var oldUI = canvas.GetComponent<Scence1CanvasUI>();
        if (oldUI) Object.DestroyImmediate(oldUI);
        canvas.gameObject.SetActive(true);
        canvas.enabled = true;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (!scaler) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2048, 1153);
        scaler.matchWidthOrHeight = .5f;
        if (!canvas.GetComponent<GraphicRaycaster>()) canvas.gameObject.AddComponent<GraphicRaycaster>();
        var backdrop = Rect("Black Backdrop", canvas.transform);
        Stretch(backdrop);
        var black = backdrop.gameObject.AddComponent<Image>(); black.color = Color.black; black.raycastTarget = false;
        var frame = Rect("Campus Quest Image", canvas.transform);
        frame.anchorMin = frame.anchorMax = new Vector2(.5f, .5f);
        var aspect = frame.gameObject.AddComponent<AspectRatioFitter>();
        aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        // Source image ratio, independent of texture import downscaling.
        aspect.aspectRatio = 5050f / 2842f;
        var image = frame.gameObject.AddComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
        var start = Hotspot(frame, "Start Button", 70, 418, 580, 554);
        var quit = Hotspot(frame, "Quit Button", 95, 797, 530, 905);
        var so = new SerializedObject(menu);
        so.FindProperty("gameScene").stringValue = "Assets/Scenes/level1-Final.unity";
        so.FindProperty("startButton").objectReferenceValue = start;
        so.FindProperty("quitButton").objectReferenceValue = quit;
        so.ApplyModifiedPropertiesWithoutUndo();
        var builds = EditorBuildSettings.scenes.ToList();
        foreach (string path in new[] { Path, "Assets/Scenes/level1-Final.unity" })
        {
            var entry = builds.FirstOrDefault(s => s.path == path);
            if (entry != null) entry.enabled = true;
            else builds.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = builds.ToArray();
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene))
        {
            File.WriteAllText(Marker, "Start Button -> Assets/Scenes/level1-Final.unity");
            File.WriteAllText("Library/Level0MenuSync.v3.done", "Campus Quest menu replaces old menu.");
            Debug.Log("Campus Quest menu saved. Start Game opens level1-Final.");
        }
        if (opened) EditorSceneManager.CloseScene(scene, true);
    }
    static RectTransform Rect(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        return (RectTransform)obj.transform;
    }
    static void Stretch(RectTransform r)
    { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    static Button Hotspot(Transform parent, string name, float left, float top, float right, float bottom)
    {
        var r = Rect(name, parent);
        r.anchorMin = new Vector2(left / 2048f, 1 - bottom / 1153f);
        r.anchorMax = new Vector2(right / 2048f, 1 - top / 1153f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        var image = r.gameObject.AddComponent<Image>(); image.color = Color.clear;
        var button = r.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        return button;
    }
}
#endif
