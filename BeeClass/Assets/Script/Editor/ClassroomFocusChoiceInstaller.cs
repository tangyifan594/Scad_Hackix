#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ClassroomFocusChoiceInstaller
{
    const string ScenePath = "Assets/Scenes/Classroom.unity";
    const string MarkerPath = "Library/ClassroomFocusChoice.v1.done";
    const double StudentSpeechEndTime = 6.64821d;

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

    [MenuItem("Tools/Codex/Install Classroom Focus Choice Trigger")]
    public static void Install()
    {
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            GameObject directorObject = FindGameObject(scene, "Playable Director");
            GameObject focusChoice = FindGameObject(scene, "Focus Choice");
            GameObject professorLower = FindGameObject(scene, "Professor_lower");
            GameObject studentSpeech = FindGameObject(scene, "学生插话");
            GameObject audioRoot = FindGameObject(scene, "Audio");

            PlayableDirector director = directorObject
                ? directorObject.GetComponent<PlayableDirector>()
                : null;
            Button professorButton = professorLower
                ? professorLower.GetComponent<Button>()
                : null;

            if(!director || !focusChoice || !professorButton)
            {
                Debug.LogError("Classroom Focus Choice installation failed: required scene objects were not found.");
                return;
            }

            // Timeline audio does not reliably drive AudioSource.isPlaying, so
            // activate the choice at the exact end time of the student clip.
            PlayableDirectorTimeEvent timeEvent = directorObject.GetComponent<PlayableDirectorTimeEvent>();
            if(!timeEvent)timeEvent = directorObject.AddComponent<PlayableDirectorTimeEvent>();
            timeEvent.Configure(director, StudentSpeechEndTime);
            ClearPersistentListeners(timeEvent.OnTimeReached);
            UnityEventTools.AddBoolPersistentListener(timeEvent.OnTimeReached, focusChoice.SetActive, true);

            // Focus Choice starts hidden. The director keeps playing after it
            // appears; Professor_lower remains responsible for stopping it.
            focusChoice.SetActive(false);
            UnityEventTools.RemovePersistentListener(professorButton.onClick, director.Stop);
            UnityEventTools.AddPersistentListener(professorButton.onClick, director.Stop);

            // Disable the old AudioSource completion watcher on the Timeline-
            // controlled student clip to prevent two competing trigger paths.
            if(studentSpeech)
            {
                AudioCompletionEvent oldCompletion = studentSpeech.GetComponent<AudioCompletionEvent>();
                if(oldCompletion)
                {
                    oldCompletion.enabled = false;
                    EditorUtility.SetDirty(oldCompletion);
                }
            }

            // This separate source played classroomprofessor on awake while the
            // Timeline also played the same clip.
            if(audioRoot)
            {
                AudioSource duplicateSource = audioRoot.GetComponent<AudioSource>();
                if(duplicateSource)
                {
                    duplicateSource.playOnAwake = false;
                    EditorUtility.SetDirty(duplicateSource);
                }
            }

            EditorUtility.SetDirty(timeEvent);
            EditorUtility.SetDirty(focusChoice);
            EditorUtility.SetDirty(professorButton);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("Classroom Focus Choice trigger installed at Timeline 6.64821s; Professor_lower stops the director; duplicate professor Play On Awake disabled.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    static void ClearPersistentListeners(UnityEngine.Events.UnityEvent unityEvent)
    {
        for(int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(unityEvent, i);
    }

    static GameObject FindGameObject(Scene scene, string objectName)
    {
        foreach(GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach(Transform item in transforms)
                if(item.name.Trim() == objectName)return item.gameObject;
        }
        return null;
    }
}
#endif
