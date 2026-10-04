using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(-1000)]
public class CampusMainMenu : MonoBehaviour
{
    [SerializeField] string gameScene = "Assets/Scenes/level1-Campus.unity";
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;
    [SerializeField] RawImage menuBackground;
    [SerializeField] Texture2D startPressedTexture;
    bool loading;

    void Awake()
    {
        // Preserve the scene's authored UI and camera; only stop gameplay behaviour.
        foreach (var player in FindObjectsByType<CampusWalker>(FindObjectsSortMode.None))
        {
            if (player.gameObject.scene != gameObject.scene) continue;
            foreach (var camera in player.GetComponentsInChildren<Camera>(true))
                camera.transform.SetParent(null, true);
            player.gameObject.SetActive(false);
        }
        foreach (var hud in FindObjectsByType<Scence1CanvasUI>(FindObjectsSortMode.None))
            if (hud.gameObject.scene == gameObject.scene) hud.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (!FindFirstObjectByType<EventSystem>())
            new GameObject("Menu EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    void Start()
    {
        // Bind existing buttons without creating or changing any visuals.
        foreach (var button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (button.gameObject.scene != gameObject.scene) continue;
            var text = button.GetComponentInChildren<Text>();
            string label = text ? text.text.Trim() : button.name;
            if (!startButton && (label == "开始游戏" || label == "进入游戏" || button.name == "Start Button")) startButton = button;
            if (!quitButton && (label == "退出" || label == "退出游戏" || label == "结束游戏" || button.name == "Quit Button")) quitButton = button;
        }
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (quitButton) quitButton.onClick.AddListener(Quit);
        if (!startButton || !quitButton)
            Debug.LogWarning("Assign the scene's Start Button and Quit Button on Level0 Main Menu. Existing layout is preserved.", this);
    }

    public void StartGame()
    {
        if (loading) return;
        if (!Application.CanStreamedLevelBeLoaded(gameScene))
        {
            Debug.LogError("Main menu destination is missing from Build Settings: " + gameScene, this);
            return;
        }
        loading = true;
        if (menuBackground && startPressedTexture && startButton) StartCoroutine(AnimateStart());
        else SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }

    IEnumerator AnimateStart()
    {
        startButton.interactable = false;
        var rect = (RectTransform)startButton.transform;
        var layer = new GameObject("Start pressed animation", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        var layerRect = (RectTransform)layer.transform;
        layerRect.SetParent(rect, false);
        layerRect.anchorMin = Vector2.zero; layerRect.anchorMax = Vector2.one;
        layerRect.offsetMin = layerRect.offsetMax = Vector2.zero;
        var image = layer.GetComponent<RawImage>();
        image.texture = startPressedTexture;
        // Both images use matching framing; only the Start paper changes.
        image.uvRect = new Rect(rect.anchorMin, rect.anchorMax - rect.anchorMin);
        image.raycastTarget = false;
        Vector3 originalScale = rect.localScale;
        float elapsed = 0;
        while (elapsed < .38f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / .38f);
            image.color = new Color(1, 1, 1, Mathf.Clamp01(t * 4));
            rect.localScale = originalScale * (1 + .025f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        rect.localScale = originalScale;
        SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Single);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (startButton) startButton.onClick.RemoveListener(StartGame);
        if (quitButton) quitButton.onClick.RemoveListener(Quit);
    }
}
