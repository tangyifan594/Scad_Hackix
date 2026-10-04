#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NectarClassOverDelayInstaller
{
    const string ScenePath = "Assets/Scenes/Classroom.unity";
    const string MarkerPath = "Library/NectarClassOverDelay.v1.done";
    const string DelayObjectName = "Nectar ClassOver Delay";

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

    [MenuItem("Tools/Codex/Install Nectar ClassOver Delay")]
    public static void Install()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            GameObject nectarObject = FindGameObject(scene, "Nectar Choice Button");
            GameObject classOver = FindGameObject(scene, "ClassOver");
            Button nectarButton = nectarObject ? nectarObject.GetComponent<Button>() : null;

            if(!nectarButton || !classOver)
            {
                Debug.LogError("Could not install Nectar delay: Nectar Choice Button or ClassOver was not found in Classroom.unity.");
                return;
            }

            GameObject delayObject = FindGameObject(scene, DelayObjectName);
            if(!delayObject)
            {
                delayObject = new GameObject(DelayObjectName);
                SceneManager.MoveGameObjectToScene(delayObject, scene);
            }

            DelayedEventTrigger delayedTrigger = delayObject.GetComponent<DelayedEventTrigger>();
            if(!delayedTrigger)delayedTrigger = delayObject.AddComponent<DelayedEventTrigger>();

            SerializedObject serializedTrigger = new SerializedObject(delayedTrigger);
            serializedTrigger.FindProperty("delay").floatValue = 2f;
            serializedTrigger.FindProperty("useUnscaledTime").boolValue = false;
            serializedTrigger.FindProperty("restartDelayWhenTriggeredAgain").boolValue = true;
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();

            for(int i = delayedTrigger.OnDelayFinished.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(delayedTrigger.OnDelayFinished, i);
            UnityEventTools.AddBoolPersistentListener(delayedTrigger.OnDelayFinished, classOver.SetActive, true);
            UnityEventTools.RemovePersistentListener(nectarButton.onClick, delayedTrigger.Trigger);
            UnityEventTools.AddPersistentListener(nectarButton.onClick, delayedTrigger.Trigger);

            EditorUtility.SetDirty(delayedTrigger);
            EditorUtility.SetDirty(nectarButton);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("Nectar Choice Button now activates ClassOver after a 2 second delay.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
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
