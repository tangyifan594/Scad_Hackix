using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scence1CanvasUI : MonoBehaviour
{
    CampusWalker walker;
    CampusInventory inventory;
    GameObject helpPanel, crosshair, toastPanel, backpackPanel, detailOverlay, emptyText, cardTemplate;
    Text backpackButtonText, countText, toastText, detailTitle, detailSource, detailDescription;
    Transform cardContent;
    Button backpackButton, closeBackpackButton, closeDetailButton, deleteDetailButton;
    string renderedCards = "";
    string shownDetailId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if(SceneManager.GetActiveScene().name!="Scence1" || FindFirstObjectByType<Scence1CanvasUI>())return;
        var root=new GameObject("Scence1 Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        BuildRuntimeHierarchy(root.transform);
        root.AddComponent<Scence1CanvasUI>();
        if(!FindFirstObjectByType<EventSystem>())new GameObject("Scence1 EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
    }

    void Awake()
    {
        // The main-menu scene owns its layout; do not hide or rebind its panels.
        foreach(var root in gameObject.scene.GetRootGameObjects())
            if(root.GetComponentInChildren<CampusMainMenu>(true)){enabled=false;return;}
        walker=FindFirstObjectByType<CampusWalker>();
        inventory=walker?walker.GetComponent<CampusInventory>():FindFirstObjectByType<CampusInventory>();
        Bind();
        if(walker)walker.useCanvasUI=true;
        if(inventory)inventory.useCanvasUI=true;
    }

    T Find<T>(string path) where T:Component
    {
        var child=transform.Find(path);
        return child?child.GetComponent<T>():null;
    }

    GameObject FindObject(string path)
    {
        var child=transform.Find(path);
        return child?child.gameObject:null;
    }

    void Bind()
    {
        helpPanel=FindObject("Help Panel");
        crosshair=FindObject("Crosshair");
        toastPanel=FindObject("Toast Panel");
        backpackPanel=FindObject("Backpack Panel");
        detailOverlay=FindObject("Detail Overlay");
        emptyText=FindObject("Backpack Panel/Empty Text");
        cardTemplate=FindObject("Backpack Panel/Card Scroll/Viewport/Content/Card Template");
        cardContent=cardTemplate?cardTemplate.transform.parent:null;
        backpackButton=Find<Button>("Backpack Button");
        backpackButtonText=Find<Text>("Backpack Button/Text");
        closeBackpackButton=Find<Button>("Backpack Panel/Close Button");
        countText=Find<Text>("Backpack Panel/Count Text");
        toastText=Find<Text>("Toast Panel/Text");
        detailTitle=Find<Text>("Detail Overlay/Detail Card/Title");
        detailSource=Find<Text>("Detail Overlay/Detail Card/Source");
        detailDescription=Find<Text>("Detail Overlay/Detail Card/Description");
        closeDetailButton=Find<Button>("Detail Overlay/Detail Card/Close Button");
        deleteDetailButton=Find<Button>("Detail Overlay/Detail Card/Delete Button");

        if(backpackButton)backpackButton.onClick.AddListener(()=>inventory?.SetOpen(true));
        if(closeBackpackButton)closeBackpackButton.onClick.AddListener(()=>inventory?.SetOpen(false));
        if(closeDetailButton)closeDetailButton.onClick.AddListener(()=>inventory?.CloseDetails());
        if(deleteDetailButton)deleteDetailButton.onClick.AddListener(DeleteShownCard);
        if(cardTemplate)cardTemplate.SetActive(false);
        if(backpackPanel)backpackPanel.SetActive(false);
        if(detailOverlay)detailOverlay.SetActive(false);
        if(toastPanel)toastPanel.SetActive(false);
    }

    void Update()
    {
        if(!inventory)return;
        if(helpPanel)helpPanel.SetActive(!inventory.BackpackOpen);
        if(crosshair)crosshair.SetActive(walker && walker.Captured && !inventory.BackpackOpen);
        if(backpackButton)backpackButton.gameObject.SetActive(!inventory.BackpackOpen);
        if(backpackButtonText)backpackButtonText.text="背包 [B]  "+inventory.Count;

        bool showToast=!inventory.BackpackOpen && Time.unscaledTime<inventory.ToastUntil && !string.IsNullOrEmpty(inventory.Toast);
        if(toastPanel)toastPanel.SetActive(showToast);
        if(showToast && toastText)toastText.text=inventory.Toast;

        if(backpackPanel)backpackPanel.SetActive(inventory.BackpackOpen);
        if(!inventory.BackpackOpen)
        {
            renderedCards="";
            if(detailOverlay)detailOverlay.SetActive(false);
            return;
        }

        string signature=inventory.Count+":"+string.Join("|",CollectedIds());
        if(signature!=renderedCards){renderedCards=signature;RebuildCards();}
        ShowDetails(inventory.DetailCardId);
    }

    List<string> CollectedIds()
    {
        var ids=new List<string>();
        foreach(var card in inventory.catalog)if(card!=null && inventory.HasCard(card.id))ids.Add(card.id);
        return ids;
    }

    void RebuildCards()
    {
        if(!cardContent || !cardTemplate)return;
        for(int i=cardContent.childCount-1;i>=0;i--)
        {
            var child=cardContent.GetChild(i);
            if(child.gameObject!=cardTemplate)Destroy(child.gameObject);
        }
        if(countText)countText.text="已收集 "+inventory.Count+" / "+inventory.catalog.Count+"  ·  点击卡片查看详情";
        if(emptyText)emptyText.SetActive(inventory.Count==0);
        foreach(var card in inventory.catalog)
        {
            if(card==null || !inventory.HasCard(card.id))continue;
            var item=Instantiate(cardTemplate,cardContent);
            item.name="Card - "+card.id;item.SetActive(true);
            var image=item.GetComponent<Image>();if(image)image.color=new Color(.14f,.20f,.29f,1);
            SetText(item.transform,"Title",card.title);
            SetText(item.transform,"Source","来自："+card.npcName);
            SetText(item.transform,"Description",card.description);
            var accent=item.transform.Find("Accent");if(accent){var a=accent.GetComponent<Image>();if(a)a.color=card.color;}
            string id=card.id;var button=item.GetComponent<Button>();if(button)button.onClick.AddListener(()=>inventory.OpenDetails(id));
        }
    }

    static void SetText(Transform root,string childName,string value)
    {
        var child=root.Find(childName);if(!child)return;
        var label=child.GetComponent<Text>();if(label)label.text=value;
    }

    void ShowDetails(string id)
    {
        bool show=!string.IsNullOrEmpty(id);
        if(detailOverlay)detailOverlay.SetActive(show);
        if(!show){shownDetailId=null;return;}
        if(shownDetailId==id)return;
        shownDetailId=id;
        var card=inventory.catalog.Find(c=>c!=null && c.id==id);
        if(card==null)return;
        if(detailTitle)detailTitle.text=card.title;
        if(detailSource)detailSource.text="来自："+card.npcName;
        if(detailDescription)detailDescription.text=card.description;
    }

    void DeleteShownCard()
    {
        if(string.IsNullOrEmpty(shownDetailId))return;
        inventory.TryRemove(shownDetailId);shownDetailId=null;renderedCards="";
    }

    static Font RuntimeFont()
    {
        return Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},24);
    }

    static void BuildRuntimeHierarchy(Transform root)
    {
        var font=RuntimeFont();
        var help=MakePanel("Help Panel",root,new Color(.035f,.07f,.11f,.88f));
        SetRect(help,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(450,112),new Vector2(24,-24));
        MakeText("Help Text",help.transform,"校园小蜜蜂\nWASD 移动   Shift 奔跑   Space 跳跃\n鼠标点击后转动视角   B 打开背包",18,TextAnchor.MiddleLeft,Color.white,true,font);
        var cross=MakePanel("Crosshair",root,Color.white);SetRect(cross,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(5,5),Vector2.zero);
        var open=MakeButton("Backpack Button",root,"背包 [B]",20,new Color(.12f,.32f,.50f,.96f),font);SetRect(open.gameObject,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(190,54),new Vector2(-24,-24));
        var toast=MakePanel("Toast Panel",root,new Color(.04f,.15f,.23f,.96f));SetRect(toast,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(480,94),new Vector2(0,-125));MakeText("Text",toast.transform,"获得卡片",20,TextAnchor.MiddleCenter,Color.white,true,font);

        var backpack=MakePanel("Backpack Panel",root,new Color(.045f,.075f,.12f,.98f));SetStretch(backpack,new Vector2(.08f,.08f),new Vector2(.92f,.92f),Vector2.zero,Vector2.zero);
        var title=MakeText("Title",backpack.transform,"我的背包",34,TextAnchor.MiddleLeft,Color.white,false,font);SetRect(title.gameObject,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(400,56),new Vector2(30,-24));
        var count=MakeText("Count Text",backpack.transform,"已收集 0 / 0",18,TextAnchor.MiddleLeft,new Color(.78f,.86f,.94f),false,font);SetRect(count.gameObject,new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),new Vector2(620,38),new Vector2(30,-82));
        var close=MakeButton("Close Button",backpack.transform,"关闭 [B]",18,new Color(.28f,.34f,.43f,1),font);SetRect(close.gameObject,new Vector2(1,1),new Vector2(1,1),new Vector2(1,1),new Vector2(140,46),new Vector2(-28,-26));
        var empty=MakeText("Empty Text",backpack.transform,"背包还是空的。\n走到 NPC 身边即可获得卡片。",23,TextAnchor.MiddleCenter,new Color(.78f,.84f,.9f),true,font);SetStretch(empty.gameObject,new Vector2(.12f,.24f),new Vector2(.88f,.72f),Vector2.zero,Vector2.zero);
        var scroll=MakeObject("Card Scroll",backpack.transform,typeof(ScrollRect));SetStretch(scroll,new Vector2(.035f,.055f),new Vector2(.965f,.80f),Vector2.zero,Vector2.zero);
        var viewport=MakePanel("Viewport",scroll.transform,new Color(1,1,1,.02f));SetStretch(viewport,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);viewport.AddComponent<Mask>().showMaskGraphic=false;
        var content=MakeObject("Content",viewport.transform,typeof(GridLayoutGroup),typeof(ContentSizeFitter));var cr=content.GetComponent<RectTransform>();cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(.5f,1);cr.offsetMin=Vector2.zero;cr.offsetMax=Vector2.zero;
        var grid=content.GetComponent<GridLayoutGroup>();grid.cellSize=new Vector2(285,190);grid.spacing=new Vector2(18,18);grid.padding=new RectOffset(10,10,10,10);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=3;content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        var sr=scroll.GetComponent<ScrollRect>();sr.viewport=viewport.GetComponent<RectTransform>();sr.content=cr;sr.horizontal=false;sr.vertical=true;
        BuildRuntimeCardTemplate(content.transform,font);

        var overlay=MakePanel("Detail Overlay",root,new Color(0,0,0,.72f));SetStretch(overlay,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
        var detail=MakePanel("Detail Card",overlay.transform,new Color(.07f,.13f,.21f,1));SetRect(detail,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(600,430),Vector2.zero);
        var dt=MakeText("Title",detail.transform,"卡片标题",32,TextAnchor.MiddleLeft,Color.white,false,font);SetRect(dt.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,62),new Vector2(0,-30));
        var ds=MakeText("Source",detail.transform,"来自：",21,TextAnchor.MiddleLeft,new Color(.95f,.72f,.28f),false,font);SetRect(ds.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,42),new Vector2(0,-102));
        var dd=MakeText("Description",detail.transform,"卡片说明",21,TextAnchor.UpperLeft,Color.white,true,font);SetRect(dd.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),new Vector2(-56,150),new Vector2(0,-180));
        var note=MakeText("Note",detail.transform,"删除后可再次碰到该 NPC 重新领取。",16,TextAnchor.MiddleLeft,new Color(.7f,.76f,.82f),false,font);SetRect(note.gameObject,new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),new Vector2(-56,34),new Vector2(0,82));
        var del=MakeButton("Delete Button",detail.transform,"删除卡片",18,new Color(.58f,.19f,.18f,1),font);SetRect(del.gameObject,new Vector2(0,0),new Vector2(0,0),new Vector2(0,0),new Vector2(140,44),new Vector2(28,26));
        var dc=MakeButton("Close Button",detail.transform,"关闭 [Esc]",18,new Color(.28f,.34f,.43f,1),font);SetRect(dc.gameObject,new Vector2(1,0),new Vector2(1,0),new Vector2(1,0),new Vector2(140,44),new Vector2(-28,26));
    }

    static void BuildRuntimeCardTemplate(Transform parent,Font font)
    {
        var card=MakeButton("Card Template",parent,"",18,new Color(.14f,.20f,.29f,1),font).gameObject;card.GetComponent<RectTransform>().sizeDelta=new Vector2(285,190);Destroy(card.transform.Find("Text").gameObject);
        var accent=MakePanel("Accent",card.transform,new Color(.95f,.63f,.18f,1));SetStretch(accent,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-8),Vector2.zero);
        var title=MakeText("Title",card.transform,"卡片标题",23,TextAnchor.MiddleLeft,Color.white,false,font);SetStretch(title.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-62),new Vector2(-16,-17));
        var source=MakeText("Source",card.transform,"来自：NPC",16,TextAnchor.MiddleLeft,new Color(.82f,.88f,.94f),false,font);SetStretch(source.gameObject,new Vector2(0,1),new Vector2(1,1),new Vector2(16,-100),new Vector2(-16,-66));
        var description=MakeText("Description",card.transform,"卡片描述",15,TextAnchor.UpperLeft,new Color(.72f,.79f,.87f),true,font);SetStretch(description.gameObject,new Vector2(0,0),new Vector2(1,1),new Vector2(16,14),new Vector2(-16,-105));card.SetActive(false);
    }

    static GameObject MakeObject(string name,Transform parent,params System.Type[] components){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);foreach(var type in components)g.AddComponent(type);return g;}
    static GameObject MakePanel(string name,Transform parent,Color color){var g=MakeObject(name,parent,typeof(Image));g.GetComponent<Image>().color=color;return g;}
    static Text MakeText(string name,Transform parent,string value,int size,TextAnchor alignment,Color color,bool wrap,Font font){var g=MakeObject(name,parent,typeof(Text));var t=g.GetComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.alignment=alignment;t.color=color;t.horizontalOverflow=wrap?HorizontalWrapMode.Wrap:HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;SetStretch(g,Vector2.zero,Vector2.one,new Vector2(14,10),new Vector2(-14,-10));return t;}
    static Button MakeButton(string name,Transform parent,string label,int size,Color color,Font font){var g=MakeObject(name,parent,typeof(Image),typeof(Button));var image=g.GetComponent<Image>();image.color=color;var button=g.GetComponent<Button>();button.targetGraphic=image;var text=MakeText("Text",g.transform,label,size,TextAnchor.MiddleCenter,Color.white,true,font);SetStretch(text.gameObject,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);return button;}
    static void SetRect(GameObject g,Vector2 min,Vector2 max,Vector2 pivot,Vector2 size,Vector2 position){var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.pivot=pivot;r.sizeDelta=size;r.anchoredPosition=position;}
    static void SetStretch(GameObject g,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax){var r=g.GetComponent<RectTransform>();r.anchorMin=min;r.anchorMax=max;r.offsetMin=offsetMin;r.offsetMax=offsetMax;}
}
