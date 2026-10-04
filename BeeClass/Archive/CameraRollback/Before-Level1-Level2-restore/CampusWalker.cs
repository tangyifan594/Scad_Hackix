using UnityEngine;
using UnityEngine.InputSystem;
public class CampusWalker : MonoBehaviour
{
    public Transform visual,leftLeg,rightLeg,leftArm,rightArm;
    public Camera followCamera;
    [Tooltip("Use the Canvas HUD instead of the legacy OnGUI HUD in this scene.")]
    public bool useCanvasUI;
    public bool topDown=false;
    public bool firstPerson=false;
    public float eyeHeight=1.25f;
    public float cameraDistance=3.2f;
    public float cameraFocusHeight=1.2f;
    public float initialCameraPitch=12f;
    public bool Captured => captured;
    CharacterController controller;
    Vector3 spawnPosition;
    float verticalSpeed,yaw,pitch;bool captured;
    float currentCameraDistance;
    void Awake()
    {
        firstPerson=false;topDown=false;
        controller=GetComponent<CharacterController>();yaw=transform.eulerAngles.y;pitch=Mathf.Clamp(initialCameraPitch,-10,65);
        currentCameraDistance=cameraDistance;
        spawnPosition=transform.position;
        if(visual)foreach(var r in visual.GetComponentsInChildren<Renderer>(true))r.forceRenderingOff=false;
        if(followCamera){followCamera.orthographic=false;followCamera.fieldOfView=65;followCamera.nearClipPlane=.06f;}
        UpdateCamera();
    }
    void Update()
    {
        var k=Keyboard.current;var m=Mouse.current;if(k==null)return;
        var inventory=GetComponent<CampusInventory>();
        if((inventory && inventory.BackpackOpen)||CampusElevator.MenuOpen||CampusElevator.Travelling){captured=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;return;}
        if(m!=null && m.leftButton.wasPressedThisFrame)captured=true;
        if(k.escapeKey.wasPressedThisFrame)captured=false;
        Cursor.lockState=captured?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!captured;
        if(captured && m!=null){var delta=m.delta.ReadValue();yaw+=delta.x*.10f;pitch=Mathf.Clamp(pitch-delta.y*.09f,-10,65);}
        var input=new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));input=Vector2.ClampMagnitude(input,1);
        var direction=Quaternion.Euler(0,yaw,0)*new Vector3(input.x,0,input.y);
        if(controller.isGrounded && verticalSpeed<0)verticalSpeed=-2;
        if(controller.isGrounded && k.spaceKey.wasPressedThisFrame)verticalSpeed=6;
        verticalSpeed-=18*Time.deltaTime;controller.Move((direction*(k.leftShiftKey.isPressed?5.4f:2.8f)+Vector3.up*verticalSpeed)*Time.deltaTime);
        if(visual && direction.sqrMagnitude>.01f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(direction),Time.deltaTime*12);
        if(transform.position.y<-10){controller.enabled=false;transform.position=spawnPosition;controller.enabled=true;verticalSpeed=0;}
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var door = hit.collider.GetComponentInParent<CampusDoorTransition>();
        if (door) door.Enter(this);
    }
    void LateUpdate()
    {
        UpdateCamera();
    }
    void UpdateCamera()
    {
        if(!followCamera)return;
        followCamera.orthographic=false;
        pitch=Mathf.Clamp(pitch,-10,65);
        Vector3 focus=transform.position+Vector3.up*cameraFocusHeight;
        Vector3 offset=Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-cameraDistance);
        float distance=CameraClearance(focus,offset);
        // Pull in immediately for walls; ease back out without changing the orbit direction.
        currentCameraDistance=distance<currentCameraDistance?distance:Mathf.MoveTowards(currentCameraDistance,distance,4*Time.deltaTime);
        followCamera.transform.position=focus+offset.normalized*currentCameraDistance;followCamera.transform.LookAt(focus);
    }
    float CameraClearance(Vector3 focus,Vector3 offset)
    {
        float distance=offset.magnitude;
        foreach(var hit in Physics.SphereCastAll(focus,.18f,offset.normalized,distance,~0,QueryTriggerInteraction.Ignore))
            if(!hit.transform.IsChildOf(transform))distance=Mathf.Min(distance,Mathf.Max(.1f,hit.distance-.12f));
        return distance;
    }
    void OnDisable()
    {
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        if(visual)foreach(var r in visual.GetComponentsInChildren<Renderer>(true))r.forceRenderingOff=false;
    }
    void OnGUI()
    {
        if(useCanvasUI)return;
        var style=new GUIStyle(GUI.skin.box){fontSize=16,alignment=TextAnchor.UpperLeft,padding=new RectOffset(16,16,12,12)};
        GUI.Box(new Rect(18,18,440,88),"CAMPUS BEE - THIRD PERSON\nWASD Move   Shift Run   Space Jump\nClick: Look   E: Elevator   B: Backpack   Esc: Cursor",style);
        if(captured)GUI.Box(new Rect(Screen.width/2f-2,Screen.height/2f-2,4,4),"");
    }
}

