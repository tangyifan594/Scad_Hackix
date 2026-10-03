#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Scence1CanvasInstaller
{
    const string ScenePath="Assets/Scenes/Scence1.unity";
    const string Marker="Library/Scence1CanvasMigration.v1.done";
    static Font uiFont;

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        if(!File.Exists(Marker))EditorApplication.delayCall+=InstallWhenReady;
    }

    static void InstallWhenReady()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall+=InstallWhenReady;return;
        }
        Install();
    }

    [MenuItem("Tools/Codex/Install Scence1 Canvas UI")]
    public static void Install()
    {
        Scene scene=SceneManager.GetSceneByPath(ScenePath);
        bool openedHere=!scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            CampusWalker walker=null;
            foreach(var root in scene.GetRootGameObjects())
            {
                walker=root.GetComponentInChildren<CampusWalker>(true);
                if(walker)break;
            }
            if(!walker)throw new System.Exception("Player - Bee / CampusWalker was not found in Scence1.");
            var inventory=walker.GetComponent<CampusInventory>();
            if(!inventory)throw new System.Exception("CampusInventory was not found on Player - Bee.");
            walker.useCanvasUI=true;inventory.useCanvasUI=true;
            EditorUtility.SetDirty(walker);EditorUtility.SetDirty(inventory);

            GameObject canvasRoot=null;
            foreach(var root in scene.GetRootGameObjects())if(root.name=="Scence1 Canvas"){canvasRoot=root;break;}
            if(!canvasRoot)canvasRoot=BuildCanvas(scene);
            if(!canvasRoot.GetComponent<Scence1CanvasUI>())canvasRoot.AddComponent<Scence1CanvasUI>();
            EnsureEventSystem(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            File.WriteAllText(Marker,ScenePath);
            Debug.Log("Scence1 Canvas UI installed. Legacy OnGUI is disabled only for this scene.");
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)EditorSceneManager.CloseScene(scene,true);
        }
    }

    static Font Font()
    {
        if(uiFont)return uiFont;
        const string folder="Assets/Campus/UI";
        const string path=folder+"/Scence1 UI Font.fontsettings";
        uiFont=AssetDatabase.LoadAssetAtPath<Font>(path);
        if(uiFont)return uiFont;
        Directory.CreateDirectory(folder);
        uiFont=UnityEngine.Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},24);
        uiFont.name="Scence1 UI Font";
        AssetDatabase.CreateAsset(uiFont,path);AssetDatabase.SaveAssets();
        return uiFont;
    }

    static GameObject BuildCanvas(Scene scene)
    {
        var root=new GameObject("Scence1 Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(Scence1CanvasUI));
        SceneManager.MoveGameObjectToScene(root,scene);
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;

        var help=Panel("Help Panel",root.transform,new Color(.035f,.07f,.11f,.88f));
        Rect(help,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(450,112),new Vector2(24,-24));
        Text("Help Text",help.transform,"校园小蜜蜂\nWASD 移动   Shift 奔跑   Space 跳跃\n鼠标点击后转动视角   B 打开背包",18,TextAnchor.MiddleLeft,Color.white,true);

        var cross=Panel("Crosshair",root.transform,Color.white);Rect(cross,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(5,5),Vector2.zero);

        var backpackButton=Button("Backpack Button",root.transform,"背包 [B]",20,new Color(.12f,.32f,.50f,.96f));
        Rect(backpackButton.gameObject,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(190,54),new Vector2(-24,-24));

        var toast=Panel("Toast Panel",root.transform,new Color(.04f,.15f,.23f,.96f));
        Rect(toast,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(480,94),new Vector2(0,-125));
        Text("Text",toast.transform,"获得卡片",20,TextAnchor.MiddleCenter,Color.white,true);

        var backpack=Panel("Backpack Panel",root.transform,new Color(.045f,.075f,.12f,.98f));
        Stretch(backpack,new Vector2(.08f,.08f),new Vector2(.92f,.92f),new Vector2(0,0),new Vector2(0,0));
        var title=Text("Title",backpack.transform,"我的背包",34,TextAnchor.MiddleLeft,Color.white,false);
        Rect(title.gameObject,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(400,56),new Vector2(30,-24));
        var count=Text("Count Text",backpack.transform,"已收集 0 / 0",18,TextAnchor.MiddleLeft,new Color(.78f,.86f,.94f),false);
        Rect(count.gameObject,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(620,38),new Vector2(30,-82));
        var close=Button("Close Button",backpack.transform,"关闭 [B]",18,new Color(.28f,.34f,.43f,1));
        Rect(close.gameObject,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(140,46),new Vector2(-28,-26));
        var empty=Text("Empty Text",backpack.transform,"背包还是空的。\n走到 NPC 身边即可获得卡片。",23,TextAnchor.MiddleCenter,new Color(.78f,.84f,.9f),true);
        Stretch(empty.gameObject,new Vector2(.12f,.24f),new Vector2(.88f,.72f),Vector2.zero,Vector2.zero);

        var scroll=UI("Card Scroll",backpack.transform,typeof(ScrollRect));
        Stretch(scroll,new Vector2(.035f,.055f),new Vector2(.965f,.80f),Vector2.zero,Vector2.zero);
        var viewport=Panel("Viewport",scroll.transform,new Color(1,1,1,.02f));
        Stretch(viewport,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);viewport.AddComponent<Mask>().showMaskGraphic=false;
        var content=UI("Content",viewport.transform,typeof(GridLayoutGroup),typeof(ContentSizeFitter));
        var contentRect=content.GetComponent<RectTransform>();contentRect.anchorMin=new Vector2(0,1);contentRect.anchorMax=new Vector2(1,1);contentRect.pivot=new Vector2(.5f,1);contentRect.offsetMin=Vector2.zero;contentRect.offsetMax=Vector2.zero;
        var grid=content.GetComponent<GridLayoutGroup>();grid.cellSize=new Vector2(285,190);grid.spacing=new Vector2(18,18);grid.padding=new RectOffset(10,10,10,10);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=3;
        content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        var sr=scroll.GetComponent<ScrollRect>();sr.viewport=viewport.GetComponent<RectTransform>();sr.content=contentRect;sr.horizontal=false;sr.vertical=true;sr.movementType=ScrollRect.MovementType.Clamped;
        BuildCardTemplate(content.transform);

        var overlay=Panel("Detail Overlay",root.transform,new Color(0,0,0,.72f));Stretch(overlay,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var detail=Panel("Detail Card",overlay.transform,new Color(.07f,.13f,.21f,1));Rect(detail,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(600,430),Vector2.zero);
        var detailTitle=Text("Title",detail.transform,"卡片标题",32,TextAnchor.MiddleLeft,Color.white,false);Rect(detailTitle.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,62),new Vector2(0,-30));
        var source=Text("Source",detail.transform,"来自：",21,TextAnchor.MiddleLeft,new Color(.95f,.72f,.28f),false);Rect(source.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,42),new Vector2(0,-102));
        var description=Text("Description",detail.transform,"卡片说明",21,TextAnchor.UpperLeft,Color.white,true);Rect(description.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,150),new Vector2(0,-180));
        var note=Text("Note",detail.transform,"删除后可再次碰到该 NPC 重新领取。",16,TextAnchor.MiddleLeft,new Color(.7f,.76f,.82f),false);Rect(note.gameObject,new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(-56,34),new Vector2(0,82));
        var delete=Button("Delete Button",detail.transform,"删除卡片",18,new Color(.58f,.19f,.18f,1));Rect(delete.gameObject,new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(140,44),new Vector2(28,26));
        var detailClose=Button("Close Button",detail.transform,"关闭 [Esc]",18,new Color(.28f,.34f,.43f,1));Rect(detailClose.gameObject,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(140,44),new Vector2(-28,26));
        return root;
    }

    static void BuildCardTemplate(Transform parent)
    {
        var card=Button("Card Template",parent,"",18,new Color(.14f,.20f,.29f,1)).gameObject;
        card.GetComponent<RectTransform>().sizeDelta=new Vector2(285,190);
        var buttonText=card.transform.Find("Text");if(buttonText)Object.DestroyImmediate(buttonText.gameObject);
        var accent=Panel("Accent",card.transform,new Color(.95f,.63f,.18f,1));Stretch(accent,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-8),Vector2.zero);
        var title=Text("Title",card.transform,"卡片标题",23,TextAnchor.MiddleLeft,Color.white,false);Stretch(title.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-62),new Vector2(-16,-17));
        var source=Text("Source",card.transform,"来自：NPC",16,TextAnchor.MiddleLeft,new Color(.82f,.88f,.94f),false);Stretch(source.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-100),new Vector2(-16,-66));
        var description=Text("Description",card.transform,"卡片描述",15,TextAnchor.UpperLeft,new Color(.72f,.79f,.87f),true);Stretch(description.gameObject,new Vector2(0,0),new Vector2(1,1),new Vector2(16,14),new Vector2(-16,-105));
        card.SetActive(false);
    }

    static void EnsureEventSystem(Scene scene)
    {
        foreach(var root in scene.GetRootGameObjects())if(root.GetComponent<EventSystem>())return;
        var system=new GameObject("Scence1 EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));SceneManager.MoveGameObjectToScene(system,scene);
    }

    static GameObject UI(string name,Transform parent,params System.Type[] components)
    {
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);
        foreach(var type in components)g.AddComponent(type);return g;
    }

    static GameObject Panel(string name,Transform parent,Color color)
    {
        var g=UI(name,parent,typeof(Image));g.GetComponent<Image>().color=color;return g;
    }

    static Text Text(string name,Transform parent,string value,int size,TextAnchor alignment,Color color,bool wrap)
    {
        var g=UI(name,parent,typeof(Text));var t=g.GetComponent<Text>();t.font=Font();t.text=value;t.fontSize=size;t.alignment=alignment;t.color=color;t.horizontalOverflow=wrap?HorizontalWrapMode.Wrap:HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;Stretch(g,Vector2.zero,Vector2.one,new Vector2(14,10),new Vector2(-14,-10));return t;
    }

    static Button Button(string name,Transform parent,string label,int size,Color color)
    {
        var g=UI(name,parent,typeof(Image),typeof(Button));var image=g.GetComponent<Image>();image.color=color;var button=g.GetComponent<Button>();button.targetGraphic=image;
        var text=Text("Text",g.transform,label,size,TextAnchor.MiddleCenter,Color.white,true);Stretch(text.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);return button;
    }

    static void Rect(GameObject g,Vector2 min,Vector2 max,Vector2 pivot,Vector2 size,Vector2 position)
    {
        var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.pivot=pivot;r.sizeDelta=size;r.anchoredPosition=position;
    }

    static void Stretch(GameObject g,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax)
    {
        var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;
    }
}
#endif
