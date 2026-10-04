#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ClassroomBackgroundInstaller
{
    const string ScenePath = "Assets/Scenes/Test.unity";
    const string SpritePath = "Assets/Art/background/Classroom1.png";
    const string MarkerPath = "Library/ClassroomWorldBackground.v1.done";

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

    [MenuItem("Tools/Codex/Install Classroom World Background")]
    public static void Install()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            Camera camera = FindInScene<Camera>(scene);
            if(!camera)
            {
                Debug.LogError("Classroom background installation failed: Test scene has no Camera.");
                return;
            }

            GameObject background = FindGameObject(scene, "Classroom1");
            if(!background)
            {
                background = new GameObject("Classroom1", typeof(SpriteRenderer));
                SceneManager.MoveGameObjectToScene(background, scene);
            }

            SpriteRenderer spriteRenderer = background.GetComponent<SpriteRenderer>();
            if(!spriteRenderer)spriteRenderer = background.AddComponent<SpriteRenderer>();
            Sprite classroomSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if(classroomSprite)spriteRenderer.sprite = classroomSprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = short.MinValue;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;

            background.layer = camera.gameObject.layer;
            background.transform.SetParent(camera.transform, false);

            WorldSpaceBackground fitter = background.GetComponent<WorldSpaceBackground>();
            if(!fitter)fitter = background.AddComponent<WorldSpaceBackground>();
            fitter.Configure(camera, 500f);

            EditorUtility.SetDirty(spriteRenderer);
            EditorUtility.SetDirty(fitter);
            EditorUtility.SetDirty(background.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("Classroom1 is now a world-space background fitted to the Test scene camera.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach(GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(true);
            if(component)return component;
        }
        return null;
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
