#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TestFadeInInstaller
{
    const string ScenePath = "Assets/Scenes/Test.unity";
    const string MarkerPath = "Library/TestFadeInInstaller.v1.done";
    const float FadeDuration = 1.5f;

    [MenuItem("Tools/Codex/Install Test UI Fade In")]
    public static void Install()
    {
        // Installation is manual and must never interrupt Play mode.
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!File.Exists(ScenePath))
        {
            Debug.LogWarning("Installation skipped: scene does not exist: " + ScenePath);
            return;
        }

        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            bool classOverConfigured = ConfigureObject(scene, "ClassOver");
            bool honeyWindowConfigured = ConfigureObject(scene, "Honey Choice Window");

            if(!classOverConfigured || !honeyWindowConfigured)
            {
                Debug.LogError("Fade-in installation failed because ClassOver or Honey Choice Window was not found in Test.unity.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("ClassOver and Honey Choice Window now fade in over 1.5 seconds.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    static bool ConfigureObject(Scene scene, string objectName)
    {
        GameObject target = FindGameObject(scene, objectName);
        if(!target)return false;

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if(!canvasGroup)canvasGroup = target.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        FadeInOnEnable fade = target.GetComponent<FadeInOnEnable>();
        if(!fade)fade = target.AddComponent<FadeInOnEnable>();
        fade.Configure(FadeDuration);

        EditorUtility.SetDirty(canvasGroup);
        EditorUtility.SetDirty(fade);
        return true;
    }

    static GameObject FindGameObject(Scene scene, string objectName)
    {
        foreach(GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach(Transform item in transforms)
                if(item.name == objectName)return item.gameObject;
        }
        return null;
    }
}
#endif
