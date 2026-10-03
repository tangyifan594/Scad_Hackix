using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class CampusCard
{
    public string id, title, npcName, description;
    public Color color = Color.cyan;
}

public class CampusInventory : MonoBehaviour
{
    public const string SaveKey = "Hackthorn.CampusCards.v1";
    public List<CampusCard> catalog = new List<CampusCard>();
    [Tooltip("Use the Canvas backpack instead of the legacy OnGUI backpack in this scene.")]
    public bool useCanvasUI;
    public bool BackpackOpen { get; private set; }
    public int Count => collected.Count;
    public string Toast => toast;
    public float ToastUntil => toastUntil;
    readonly HashSet<string> collected = new HashSet<string>();
    string toast; float toastUntil; Vector2 scroll;
    GUIStyle heading, text, cardTitle, small, button;
    Font font;
    CampusCard detailCard;
    public string DetailCardId => detailCard == null ? null : detailCard.id;
    [Serializable] class SavedCards { public List<string> ids = new List<string>(); }
    void Awake()
    {
        try {
            var saved = JsonUtility.FromJson<SavedCards>(PlayerPrefs.GetString(SaveKey,"{}"));
            if(saved != null && saved.ids != null) foreach(var id in saved.ids) if(catalog.Exists(c => c.id == id)) collected.Add(id);
        } catch(Exception) { Debug.LogWarning("Card save could not be read; starting an empty backpack."); }
    }
    public bool HasCard(string id) => collected.Contains(id);
    public bool TryAdd(CampusCard card)
    {
        if(card == null || string.IsNullOrEmpty(card.id) || collected.Contains(card.id)) return false;
        if(!catalog.Exists(c => c.id == card.id)) catalog.Add(card);
        collected.Add(card.id);
        Save();
        toast = "获得卡片：" + card.title + "\n已存入背包，按 B 查看"; toastUntil = Time.unscaledTime + 4;
        return true;
    }
    void Save()
    {
        PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(new SavedCards { ids = new List<string>(collected) }));PlayerPrefs.Save();
    }
    public bool TryRemove(string id)
    {
        if(string.IsNullOrEmpty(id) || !collected.Remove(id)) return false;
        if(detailCard != null && detailCard.id == id) detailCard=null;
        Save();return true;
    }
    public void OpenDetails(string id)
    {
        if(!HasCard(id))return;
        detailCard=catalog.Find(c=>c.id==id);SetOpen(true);
    }
    public void CloseDetails() { detailCard=null; }
    public void SetOpen(bool open)
    {
        BackpackOpen=open;
        if(!open)detailCard=null;
        Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
    }
    void Update()
    {
        var k=Keyboard.current;
        if(k != null && k.bKey.wasPressedThisFrame) SetOpen(!BackpackOpen);
        if(k != null && BackpackOpen && k.escapeKey.wasPressedThisFrame) { if(detailCard != null)CloseDetails();else SetOpen(false); }
    }
    void Styles()
    {
        if(heading != null) return;
        font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},20);
        text=new GUIStyle(GUI.skin.label){font=font,fontSize=18,wordWrap=true,normal={textColor=Color.white}};
        heading=new GUIStyle(text){fontSize=30,fontStyle=FontStyle.Bold};
        cardTitle=new GUIStyle(text){fontSize=23,fontStyle=FontStyle.Bold};
        small=new GUIStyle(text){fontSize=15};
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};
    }
    static void Fill(Rect rect,Color color) { var before=GUI.color; GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=before; }
    void OnGUI()
    {
        if(useCanvasUI)return;
        Styles();
        if(!BackpackOpen)
        {
            if(GUI.Button(new Rect(Screen.width-190,20,170,45),"背包 [B]  " + Count,button)) SetOpen(true);
            if(Time.unscaledTime<toastUntil)
            {
                Rect notice=new Rect((Screen.width-400)/2,115,400,85);Fill(notice,new Color(.06f,.15f,.23f,.94f));GUI.Label(new Rect(notice.x+18,notice.y+14,365,64),toast,text);
            }
            return;
        }
        Fill(new Rect(0,0,Screen.width,Screen.height),new Color(.02f,.04f,.07f,.88f));
        float width=Mathf.Min(940,Screen.width-40),height=Mathf.Min(590,Screen.height-40);
        Rect panel=new Rect((Screen.width-width)/2,(Screen.height-height)/2,width,height);Fill(panel,new Color(.08f,.12f,.19f));
        GUI.Label(new Rect(panel.x+25,panel.y+22,400,44),"我的背包",heading);
        GUI.Label(new Rect(panel.x+25,panel.y+70,500,28),"已收集 " + Count + " / " + catalog.Count + "  ·  双击卡片查看详情",small);
        bool previousEnabled=GUI.enabled;
        GUI.enabled=detailCard==null;
        if(GUI.Button(new Rect(panel.x+width-145,panel.y+25,120,38),"关闭 [B]",button))SetOpen(false);
        if(Count==0) { GUI.Label(new Rect(panel.x+30,panel.y+150,width-60,100),"背包还是空的。\n走到 NPC 身边，碰到他们即可获得卡片。",text);GUI.enabled=previousEnabled;return; }
        int columns=Mathf.Max(1,Mathf.FloorToInt((width-40)/260));float cw=(width-40)/columns-16;int rows=Mathf.CeilToInt((float)Count/columns);
        scroll=GUI.BeginScrollView(new Rect(panel.x+20,panel.y+115,width-40,height-140),scroll,new Rect(0,0,width-60,rows*280));
        int index=0;
        foreach(var card in catalog)
        {
            if(!HasCard(card.id))continue;
            Rect r=new Rect((index%columns)*(cw+16),Mathf.Floor(index/columns)*280,cw,260);index++;
            Fill(r,new Color(.14f,.20f,.29f));Fill(new Rect(r.x,r.y,r.width,8),card.color);
            Fill(new Rect(r.x+18,r.y+25,64,64),card.color);
            GUI.Label(new Rect(r.x+36,r.y+41,50,42),"★",heading);
            GUI.Label(new Rect(r.x+18,r.y+105,r.width-36,35),card.title,cardTitle);
            GUI.Label(new Rect(r.x+18,r.y+147,r.width-36,30),"来自："+card.npcName,small);
            GUI.Label(new Rect(r.x+18,r.y+184,r.width-36,38),card.description,small);
            Rect deleteRect=new Rect(r.x+r.width-85,r.y+223,70,28);
            if(GUI.Button(deleteRect,"删除",button))TryRemove(card.id);
            var evt=Event.current;
            if(GUI.enabled && evt.type==EventType.MouseDown && evt.button==0 && evt.clickCount==2 && r.Contains(evt.mousePosition) && !deleteRect.Contains(evt.mousePosition))
            { OpenDetails(card.id);evt.Use(); }
        }
        GUI.EndScrollView();
        GUI.enabled=previousEnabled;
        if(detailCard != null) DrawDetails();
    }
    void DrawDetails()
    {
        Fill(new Rect(0,0,Screen.width,Screen.height),new Color(0,0,0,.65f));
        float w=Mathf.Min(510,Screen.width-40),h=Mathf.Min(370,Screen.height-40);
        Rect r=new Rect((Screen.width-w)/2,(Screen.height-h)/2,w,h);
        Fill(r,new Color(.10f,.16f,.24f));Fill(new Rect(r.x,r.y,w,8),detailCard.color);
        GUI.Label(new Rect(r.x+28,r.y+30,w-56,48),detailCard.title,heading);
        GUI.Label(new Rect(r.x+28,r.y+98,w-56,35),"来自："+detailCard.npcName,text);
        GUI.Label(new Rect(r.x+28,r.y+150,w-56,110),detailCard.description,text);
        GUI.Label(new Rect(r.x+28,r.y+h-95,w-56,25),"删除后可再次碰到该 NPC 重新领取。",small);
        if(GUI.Button(new Rect(r.x+28,r.y+h-58,120,36),"删除卡片",button)) { var id=detailCard.id;TryRemove(id); }
        if(GUI.Button(new Rect(r.x+w-148,r.y+h-58,120,36),"关闭 [Esc]",button))CloseDetails();
        if(Event.current.type==EventType.MouseDown)Event.current.Use();
    }}

