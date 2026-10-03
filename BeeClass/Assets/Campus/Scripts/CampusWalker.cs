using UnityEngine;
using UnityEngine.InputSystem;
public class CampusWalker : MonoBehaviour
{
    public Transform visual,leftLeg,rightLeg,leftArm,rightArm;
    public Camera followCamera;
    public bool topDown=false;
    public bool firstPerson=false;
    public float eyeHeight=1.25f;
    CharacterController controller;
    float verticalSpeed,yaw,pitch;bool captured;
    void Awake()
    {
        firstPerson=false;topDown=false; // This game uses third-person follow.
        controller=GetComponent<CharacterController>();yaw=transform.eulerAngles.y;pitch=18;
        if(visual)foreach(var r in visual.GetComponentsInChildren<Renderer>(true))r.forceRenderingOff=firstPerson;
        if(followCamera){followCamera.orthographic=false;followCamera.fieldOfView=65;followCamera.nearClipPlane=.06f;}
    }
    void Update()
    {
        var k=Keyboard.current;var m=Mouse.current;if(k==null)return;
        var inventory=GetComponent<CampusInventory>();
        if((inventory && inventory.BackpackOpen)||CampusElevator.MenuOpen||CampusElevator.Travelling){captured=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
        if(m!=null && m.leftButton.wasPressedThisFrame)captured=true;
        if(k.escapeKey.wasPressedThisFrame)captured=false;
        Cursor.lockState=captured?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!captured;
        if(captured && m!=null){var delta=m.delta.ReadValue();yaw+=delta.x*.10f;pitch=Mathf.Clamp(pitch-delta.y*.09f,-75,75);}
        var input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));input=Vector2.ClampMagnitude(input,1);
        var direction=Quaternion.Euler(0,yaw,0)*new Vector3(input.x,0,input.y);
        if(controller.isGrounded && verticalSpeed<0)verticalSpeed=-2;
        if(controller.isGrounded && k.spaceKey.wasPressedThisFrame)verticalSpeed=6;
        verticalSpeed-=18*Time.deltaTime;controller.Move((direction*(k.leftShiftKey.isPressed?5.4f:2.8f)+Vector3.up*verticalSpeed)*Time.deltaTime);
        if(visual && direction.sqrMagnitude>.01f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(direction),Time.deltaTime*12);
        if(transform.position.y<-10){controller.enabled=false;transform.position=new Vector3(0,.1f,-12);controller.enabled=true;verticalSpeed=0;}
    }
    void LateUpdate()
    {
        if(!followCamera)return;
        followCamera.orthographic=false;
        if(firstPerson){followCamera.transform.SetPositionAndRotation(transform.position+Vector3.up*eyeHeight,Quaternion.Euler(pitch,yaw,0));return;}
        pitch=Mathf.Clamp(pitch,-10,65);
        Vector3 focus=transform.position+Vector3.up*1.05f;
        Vector3 offset=Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-4.2f);
        float distance=offset.magnitude;
        foreach(var hit in Physics.SphereCastAll(focus,.18f,offset.normalized,distance,~0,QueryTriggerInteraction.Ignore))
            if(!hit.transform.IsChildOf(transform))distance=Mathf.Min(distance,Mathf.Max(.45f,hit.distance-.12f));
        followCamera.transform.position=focus+offset.normalized*distance;followCamera.transform.LookAt(focus);
    }
    void OnDisable()
    {
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        if(visual)foreach(var r in visual.GetComponentsInChildren<Renderer>(true))r.forceRenderingOff=false;
    }
    void OnGUI()
    {
        var style=new GUIStyle(GUI.skin.box){fontSize=16,alignment=TextAnchor.UpperLeft,padding=new RectOffset(16,16,12,12)};
        GUI.Box(new Rect(18,18,440,88),"CAMPUS BEE - THIRD PERSON\nWASD Move   Shift Run   Space Jump\nClick: Look   E: Elevator   B: Backpack   Esc: Cursor",style);
        if(captured)GUI.Box(new Rect(Screen.width/2f-2,Screen.height/2f-2,4,4),"");
    }
}

