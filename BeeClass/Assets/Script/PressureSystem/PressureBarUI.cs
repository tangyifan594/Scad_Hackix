using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PressureBarUI : MonoBehaviour
{
    [SerializeField] PressureSystem pressureSystem;
    [SerializeField] Image fillImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateTestUIIfMissing()
    {
        if(SceneManager.GetActiveScene().name != "Test" || FindFirstObjectByType<PressureBarUI>())return;

        PressureSystem system = FindFirstObjectByType<PressureSystem>();
        if(!system)system = new GameObject("Pressure System").AddComponent<PressureSystem>();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if(!canvas)
        {
            var canvasObject = new GameObject("Pressure Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();canvas.renderMode = RenderMode.ScreenSpaceOverlay;canvas.sortingOrder = 110;
            var scaler = canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution = new Vector2(1920,1080);scaler.matchWidthOrHeight = .5f;
        }

        BuildTestUI(canvas.transform, system);
        if(!FindFirstObjectByType<EventSystem>())new GameObject("Pressure EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    void Awake()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
    }

    void OnEnable()
    {
        if(!pressureSystem)return;
        pressureSystem.OnPressureChanged += UpdateBar;
        UpdateBar(pressureSystem.NormalizedPressure);
    }

    void OnDisable()
    {
        if(pressureSystem)pressureSystem.OnPressureChanged -= UpdateBar;
    }

    void UpdateBar(float percentage)
    {
        if(!fillImage)return;
        fillImage.fillAmount = Mathf.Clamp01(percentage);
    }

    public void Configure(PressureSystem system, Image fill)
    {
        pressureSystem = system;
        fillImage = fill;
        if(pressureSystem && fillImage)UpdateBar(pressureSystem.NormalizedPressure);
    }

    static void BuildTestUI(Transform canvas, PressureSystem system)
    {
        Font font = Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"}, 22);
        var root = UIObject("Pressure UI", canvas);Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        var bar = Panel("Pressure Bar", root.transform, new Color(.09f,.11f,.15f,.94f));
        SetRect(bar, new Vector2(0,1), new Vector2(0,1), new Vector2(0,1), new Vector2(360,34), new Vector2(24,-24));
        var fillObject = Panel("Fill", bar.transform, new Color(.92f,.42f,.22f,1));Stretch(fillObject, Vector2.zero, Vector2.one, new Vector2(5,5), new Vector2(-5,-5));
        var fill = fillObject.GetComponent<Image>();fill.sprite = CreateRuntimePressureSprite();fill.type = Image.Type.Filled;fill.fillMethod = Image.FillMethod.Horizontal;fill.fillOrigin = 0;fill.fillAmount = system.NormalizedPressure;fill.raycastTarget = false;

        var controls = UIObject("Pressure Test Controls", root.transform);SetRect(controls, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(440,64), Vector2.zero);
        Button lower = CreateButton("Pressure Lower Button", controls.transform, "压力 -10", font, new Color(.18f,.43f,.62f,1));SetRect(lower.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(-110,0));
        var lowerTrigger = lower.gameObject.AddComponent<PressureChangeTrigger>();lowerTrigger.Configure(system, PressureChangeTrigger.Direction.Lower, 10f);lower.onClick.AddListener(lowerTrigger.Trigger);
        Button higher = CreateButton("Pressure Higher Button", controls.transform, "压力 +10", font, new Color(.76f,.28f,.20f,1));SetRect(higher.gameObject, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(200,58), new Vector2(110,0));
        var higherTrigger = higher.gameObject.AddComponent<PressureChangeTrigger>();higherTrigger.Configure(system, PressureChangeTrigger.Direction.Higher, 10f);higher.onClick.AddListener(higherTrigger.Trigger);

        var ui = root.AddComponent<PressureBarUI>();ui.Configure(system, fill);
    }

    static Sprite CreateRuntimePressureSprite()
    {
        const int size = 32;
        const float radius = 8f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Runtime Pressure Fill Texture",
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
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    static GameObject UIObject(string name, Transform parent, params System.Type[] components){var item = new GameObject(name, typeof(RectTransform));item.transform.SetParent(parent, false);foreach(var component in components)item.AddComponent(component);return item;}
    static GameObject Panel(string name, Transform parent, Color color){var item = UIObject(name, parent, typeof(Image));item.GetComponent<Image>().color = color;return item;}
    static Button CreateButton(string name, Transform parent, string label, Font font, Color color){var item = UIObject(name, parent, typeof(Image), typeof(Button));var image = item.GetComponent<Image>();image.color = color;var button = item.GetComponent<Button>();button.targetGraphic = image;var textObject = UIObject("Text", item.transform, typeof(Text));Stretch(textObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);var text = textObject.GetComponent<Text>();text.font = font;text.text = label;text.fontSize = 22;text.alignment = TextAnchor.MiddleCenter;text.color = Color.white;text.raycastTarget = false;return button;}
    static void SetRect(GameObject item, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 position){var rect = item.GetComponent<RectTransform>();rect.anchorMin = min;rect.anchorMax = max;rect.pivot = pivot;rect.sizeDelta = size;rect.anchoredPosition = position;}
    static void Stretch(GameObject item, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax){var rect = item.GetComponent<RectTransform>();rect.anchorMin = min;rect.anchorMax = max;rect.offsetMin = offsetMin;rect.offsetMax = offsetMax;}
}
