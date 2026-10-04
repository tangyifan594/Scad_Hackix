using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class CampusSchoolSetup
{
    static Material wall,floor,wood,blue,glass,dark;
    static Material Mat(string name,Color color){string path="Assets/Campus/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;return m;}
    static GameObject Box(string name,Transform parent,Vector3 pos,Vector3 size,Material material,bool solid=true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=material;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Label(string name,Transform parent,Vector3 pos,string text,float size=.05f)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;var t=g.AddComponent<TextMesh>();t.text=text;t.characterSize=size;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;
    }
    [MenuItem("Tools/Codex/Build Three Floor School")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before building school.");
        var player=Object.FindFirstObjectByType<CampusWalker>();if(!player)throw new System.Exception("Player missing.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach(var root in scene.GetRootGameObjects())if(root!=player.gameObject && !root.GetComponent<Camera>() && !root.GetComponent<Light>())Object.DestroyImmediate(root);
        wall=Mat("School warm walls",new Color(.88f,.85f,.75f));floor=Mat("School floor",new Color(.62f,.68f,.71f));wood=Mat("School desks",new Color(.58f,.37f,.19f));blue=Mat("School blue",new Color(.12f,.32f,.51f));glass=Mat("School windows",new Color(.38f,.71f,.84f));dark=Mat("School chalkboard",new Color(.09f,.24f,.20f));
        var building=new GameObject("Three floor school - 9 classrooms").transform;
        var grass=Mat("School lawn",new Color(.33f,.5f,.31f));Box("Ground",building,new Vector3(0,-.35f,5),new Vector3(48,.4f,46),grass);
        Box("Entrance path",building,new Vector3(0,-.08f,-12),new Vector3(5,.15f,16),floor);
        for(int level=0;level<3;level++)
        {
            float y=level*4;var landing=new GameObject("Floor "+(level+1)).transform;landing.SetParent(building,false);
            Box("Main floor slab",landing,new Vector3(2,y-.12f,5),new Vector3(20,.24f,18),floor);
            Box("Floor beside elevator south",landing,new Vector3(-10,y-.12f,-3.5f),new Vector3(4,.24f,1),floor);
            Box("Floor beside elevator north",landing,new Vector3(-10,y-.12f,7.5f),new Vector3(4,.24f,13),floor);
            Box("Back outer wall",landing,new Vector3(0,y+1.9f,14),new Vector3(24,.0f+3.8f,.25f),wall);
            foreach(float x in new[]{-12f,12f})Box("Side outer wall",landing,new Vector3(x,y+1.9f,5),new Vector3(.25f,3.8f,18),wall);
            if(level==0){foreach(float x in new[]{-7f,7f})Box("Entrance front wall",landing,new Vector3(x,y+1.9f,-4),new Vector3(10,3.8f,.25f),wall);Box("Entrance lintel",landing,new Vector3(0,y+3.25f,-4),new Vector3(4,1.1f,.25f),wall);}
            else Box("Front outer wall",landing,new Vector3(0,y+1.9f,-4),new Vector3(24,3.8f,.25f),wall);
            for(int room=0;room<3;room++)
            {
                float x=-8+room*8;string number=((level+1)*100+room+1).ToString();
                var classroom=new GameObject("Classroom "+number).transform;classroom.SetParent(landing,false);
                foreach(float side in new[]{-1f,1f})Box("Door side wall",classroom,new Vector3(x+side*2.55f,y+1.9f,2),new Vector3(3.05f,3.8f,.18f),wall);
                Box("Door lintel",classroom,new Vector3(x,y+3.3f,2),new Vector3(2,1,.18f),wall);
                Box("Room sign",classroom,new Vector3(x,y+2.8f,1.84f),new Vector3(1.7f,.48f,.06f),blue,false);Label("Room number",classroom,new Vector3(x,y+2.8f,1.79f),"ROOM "+number,.045f);
                if(room<2)Box("Classroom partition",classroom,new Vector3(x+4,y+1.9f,8),new Vector3(.18f,3.8f,12),wall);
                Box("Chalkboard",classroom,new Vector3(x,y+1.7f,13.8f),new Vector3(5,1.5f,.08f),dark);Label("Chalkboard text",classroom,new Vector3(x,y+1.7f,13.73f),"WELCOME\nCLASS "+number,.055f);
                Box("Teacher desk",classroom,new Vector3(x,y+.65f,11.6f),new Vector3(2,1.3f,.8f),wood);
                for(int row=0;row<3;row++)foreach(float dx in new[]{-1.65f,1.65f})
                {
                    float z=4.6f+row*2;Box("Desk top",classroom,new Vector3(x+dx,y+.75f,z),new Vector3(1.25f,.12f,.65f),wood);Box("Desk legs",classroom,new Vector3(x+dx,y+.35f,z),new Vector3(.85f,.7f,.40f),blue);
                    Box("Chair",classroom,new Vector3(x+dx,y+.4f,z-.7f),new Vector3(.6f,.8f,.5f),blue);
                }
                if(level>0 || room!=1)Box("Front window",landing,new Vector3(x,y+2,-4.16f),new Vector3(2.6f,1.5f,.08f),glass,false);
                Box("Back window",classroom,new Vector3(x+2.6f,y+2,13.82f),new Vector3(1.4f,1.7f,.06f),glass,false);
                var light=new GameObject("Classroom light").AddComponent<Light>();light.transform.SetParent(classroom,false);light.transform.localPosition=new Vector3(x,y+3.45f,8);light.type=LightType.Point;light.range=12;light.intensity=4;light.shadows=LightShadows.None;
            }
            Box("Elevator left shaft",landing,new Vector3(-10,y+1.9f,-2.85f),new Vector3(4,3.8f,.15f),blue);
            Box("Elevator right shaft",landing,new Vector3(-10,y+1.9f,.85f),new Vector3(4,3.8f,.15f),blue);
            Box("Elevator back shaft",landing,new Vector3(-11.85f,y+1.9f,-1),new Vector3(.15f,3.8f,3.7f),blue);
            Box("Elevator landing sign",landing,new Vector3(-8.03f,y+2.9f,-1),new Vector3(.07f,.5f,3),blue,false);Label("Elevator label",landing,new Vector3(-7.98f,y+2.9f,-1),"LIFT  /  FLOOR "+(level+1),.042f);
        }
        foreach(var t in building.GetComponentsInChildren<Transform>())if(t.name=="Elevator label")t.localRotation=Quaternion.Euler(0,-90,0);
        Box("Roof",building,new Vector3(0,11.92f,5),new Vector3(24,.2f,18),blue);
        Box("School sign",building,new Vector3(0,11,-4.2f),new Vector3(10,.7f,.1f),blue,false);Label("School name",building,new Vector3(0,11,-4.28f),"HACKTHORN SCHOOL",.06f);
        var lift=new GameObject("Elevator cabin").transform;lift.SetParent(building,false);lift.localPosition=new Vector3(-10,0,-1);lift.localRotation=Quaternion.Euler(0,90,0);
        Box("Lift platform",lift,new Vector3(0,-.1f,0),new Vector3(3.6f,.2f,3.6f),floor);
        Box("Cabin left",lift,new Vector3(-1.7f,1.35f,0),new Vector3(.1f,2.7f,3.5f),glass);
        Box("Cabin right",lift,new Vector3(1.7f,1.35f,0),new Vector3(.1f,2.7f,3.5f),glass);
        Box("Cabin back",lift,new Vector3(0,1.35f,-1.7f),new Vector3(3.5f,2.7f,.1f),glass);
        var elevator=building.gameObject.AddComponent<CampusElevator>();elevator.cabin=lift;
        player.transform.position=new Vector3(0,.1f,-12);player.transform.rotation=Quaternion.identity;
        if(player.followCamera){player.followCamera.transform.position=new Vector3(0,3,-17.5f);player.followCamera.transform.LookAt(new Vector3(0,1.5f,-12));}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Selection.activeGameObject=building.gameObject;
        Debug.Log("One 3-floor school built. Nine classrooms. NPCs removed.");
    }
}


