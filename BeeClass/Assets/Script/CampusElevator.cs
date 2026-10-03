using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class CampusElevator : MonoBehaviour
{
    public Transform cabin;
    public static bool MenuOpen { get; private set; }
    public static bool Travelling { get; private set; }
    public int CurrentFloor { get; private set; } = 1;
    CampusWalker player; Font font; GUIStyle style,button;
    bool Near => player && Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(-10,-1))<2.15f;
    void Awake() { MenuOpen=false;Travelling=false;player=FindFirstObjectByType<CampusWalker>(); }
    void Update()
    {
        if(!player || Travelling)return;
        var k=Keyboard.current;
        if(MenuOpen && (!Near || (k!=null && k.escapeKey.wasPressedThisFrame)))MenuOpen=false;
        var inventory=player.GetComponent<CampusInventory>();
        if(Near && k!=null && k.eKey.wasPressedThisFrame && (!inventory || !inventory.BackpackOpen)) { MenuOpen=!MenuOpen;Cursor.lockState=CursorLockMode.None;Cursor.visible=true; }
    }
    public void GoToFloor(int floor)
    {
        if(floor<1 || floor>3 || Travelling)return;
        MenuOpen=false;
        if(floor!=CurrentFloor)StartCoroutine(Ride(floor));
    }
    IEnumerator Ride(int floor)
    {
        Travelling=true;var controller=player.GetComponent<CharacterController>();controller.enabled=false;
        float from=(CurrentFloor-1)*4,to=(floor-1)*4,duration=Mathf.Abs(to-from)/2f;
        float elapsed=0;
        while(elapsed<duration)
        {
            elapsed+=Time.deltaTime;float y=Mathf.Lerp(from,to,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration)));
            cabin.localPosition=new Vector3(-10,y,-1);player.transform.position=new Vector3(-10,y+.1f,-1);yield return null;
        }
        CurrentFloor=floor;cabin.localPosition=new Vector3(-10,to,-1);player.transform.position=new Vector3(-10,to+.1f,-1);
        controller.enabled=true;Travelling=false;
    }
    void OnDisable() {MenuOpen=false;Travelling=false;if(player){var cc=player.GetComponent<CharacterController>();if(cc)cc.enabled=true;}}
    void OnGUI()
    {
        if(style==null){font=Font.CreateDynamicFontFromOSFont("Microsoft YaHei",20);style=new GUIStyle(GUI.skin.box){font=font,fontSize=20};button=new GUIStyle(GUI.skin.button){font=font,fontSize=20};}
        if(Travelling){GUI.Box(new Rect((Screen.width-300)/2,120,300,55),"电梯运行中…",style);return;}
        if(Near && !MenuOpen)GUI.Box(new Rect((Screen.width-340)/2,120,340,50),"按 E 使用电梯",style);
        if(!MenuOpen)return;
        Rect panel=new Rect((Screen.width-380)/2,(Screen.height-290)/2,380,290);GUI.Box(panel,"电梯 · 选择楼层",style);
        for(int i=1;i<=3;i++)if(GUI.Button(new Rect(panel.x+35,panel.y+45+(i-1)*55,310,45),i+" 楼 · 教室 "+i+"01 / "+i+"02 / "+i+"03",button))GoToFloor(i);
        if(GUI.Button(new Rect(panel.x+35,panel.y+220,310,40),"关闭 [Esc]",button))MenuOpen=false;
    }
}
