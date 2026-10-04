#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LabNotebookSlideInstaller
{
    const string ScenePath = "Assets/Scenes/lab.unity";
    const string MarkerPath = "Library/LabNotebookSlide.v2.done";

    [MenuItem("Tools/Codex/Install Lab Notebook Slide")]
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
            GameObject notebook = FindGameObject(scene, "notebook");
            GameObject icon1 = FindGameObject(scene, "icon1");
            GameObject iconClose = FindGameObject(scene, "icon _close");
            GameObject icon2 = FindGameObject(scene, "icon _2");
            Button openButton = icon1 ? icon1.GetComponentInChildren<Button>(true) : null;
            Button closeButton = iconClose ? iconClose.GetComponentInChildren<Button>(true) : null;

            if(!notebook || !icon1 || !iconClose || !icon2 || !openButton || !closeButton)
            {
                Debug.LogError("Lab notebook slide installation failed: notebook or icon buttons were not found.");
                return;
            }

            RectTransform notebookRect = notebook.GetComponent<RectTransform>();
            if(!notebookRect)
            {
                Debug.LogError("Lab notebook slide installation failed: notebook has no RectTransform.");
                return;
            }

            Vector2 hiddenPosition = notebookRect.anchoredPosition;
            Vector2 shownPosition = new Vector2(147f, hiddenPosition.y);

            CanvasGroup canvasGroup = notebook.GetComponent<CanvasGroup>();
            if(!canvasGroup)canvasGroup = notebook.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            PaperPanelController controller = notebook.GetComponent<PaperPanelController>();
            if(!controller)controller = notebook.AddComponent<PaperPanelController>();
            controller.Configure(shownPosition, hiddenPosition, 0.5f, true);

            // The large note image was the last child, so it sat above the icon
            // buttons and swallowed their pointer raycasts. Keep the three icon
            // controls above the paper, and let only their child Buttons receive
            // clicks.
            icon1.transform.SetAsLastSibling();
            iconClose.transform.SetAsLastSibling();
            icon2.transform.SetAsLastSibling();
            DisableRootGraphicRaycast(icon1);
            DisableRootGraphicRaycast(iconClose);
            DisableRootGraphicRaycast(icon2);
            openButton.interactable = true;
            closeButton.interactable = true;
            if(openButton.targetGraphic)openButton.targetGraphic.raycastTarget = true;
            if(closeButton.targetGraphic)closeButton.targetGraphic.raycastTarget = true;

            icon1.SetActive(true);
            iconClose.SetActive(false);
            icon2.SetActive(false);

            UnityEventTools.RemovePersistentListener(openButton.onClick, controller.SlideIn);
            UnityEventTools.AddPersistentListener(openButton.onClick, controller.SlideIn);
            UnityEventTools.RemovePersistentListener(closeButton.onClick, controller.SlideOut);
            UnityEventTools.AddPersistentListener(closeButton.onClick, controller.SlideOut);

            ClearPersistentListeners(controller.OnSlideInFinished);
            UnityEventTools.AddBoolPersistentListener(controller.OnSlideInFinished, icon1.SetActive, false);
            UnityEventTools.AddBoolPersistentListener(controller.OnSlideInFinished, iconClose.SetActive, true);

            ClearPersistentListeners(controller.OnSlideOutFinished);
            UnityEventTools.AddBoolPersistentListener(controller.OnSlideOutFinished, iconClose.SetActive, false);
            UnityEventTools.AddBoolPersistentListener(controller.OnSlideOutFinished, icon2.SetActive, true);

            EditorUtility.SetDirty(canvasGroup);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(openButton);
            EditorUtility.SetDirty(closeButton);
            EditorUtility.SetDirty(icon1);
            EditorUtility.SetDirty(iconClose);
            EditorUtility.SetDirty(icon2);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, ScenePath);
            Debug.Log("Lab notebook slide interaction installed: X 873 to X 147 and back; icon buttons moved above the paper.");
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

    static void DisableRootGraphicRaycast(GameObject icon)
    {
        Graphic graphic = icon.GetComponent<Graphic>();
        if(graphic)
        {
            graphic.raycastTarget = false;
            EditorUtility.SetDirty(graphic);
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
