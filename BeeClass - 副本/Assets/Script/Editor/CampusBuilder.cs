using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static class CampusBuilder
{
    static Material cream, brick, blue, glass, paving, grass, dark, white, skin, hair, leaf, wood, track;
    static Material Mat(string name, Color color)
    {
        string path = "Assets/Campus/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path); }
        m.color = color; return m;
    }
    static GameObject Part(string name, Vector3 pos, Vector3 size, Material material, Transform parent = null, PrimitiveType type = PrimitiveType.Cube, bool collision = true)
    {
        var g = GameObject.CreatePrimitive(type); g.name = name;
        g.transform.SetParent(parent,false); g.transform.localPosition = pos; g.transform.localScale = size;
        g.GetComponent<Renderer>().sharedMaterial = material;
        if (!collision) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    static void Label(string text, Vector3 pos, float size, Transform parent = null)
    {
        var g = new GameObject(text); g.transform.SetParent(parent,false); g.transform.localPosition = pos;
        g.transform.localRotation = Quaternion.identity;
        var t = g.AddComponent<TextMesh>(); t.text = text; t.fontSize = 64; t.characterSize = size; t.anchor = TextAnchor.MiddleCenter; t.color = Color.white;
    }
    static void Building(string name, Vector3 pos, int bays)
    {
        var root = new GameObject(name).transform; root.position = pos;
        float width = bays * 3.2f;
        Part("Main building",new Vector3(0,4.5f,0),new Vector3(width,9,8),cream,root);
        Part("Brick base",new Vector3(0,.65f,-4.05f),new Vector3(width,1.3f,.16f),brick,root);
        for(int level=0;level<3;level++)
        {
            Part("Floor ribbon",new Vector3(0,level*3+.15f,-4.14f),new Vector3(width,.18f,.18f),blue,root);
            for(int b=0;b<bays;b++)
            {
                float x = (b-(bays-1)*.5f)*3.2f;
                Part("Window frame",new Vector3(x,level*3+1.8f,-4.16f),new Vector3(2.25f,1.65f,.18f),white,root);
                Part("Blue window",new Vector3(x,level*3+1.8f,-4.28f),new Vector3(2,1.4f,.07f),glass,root);
                Part("Window mullion",new Vector3(x,level*3+1.8f,-4.34f),new Vector3(.07f,1.4f,.04f),white,root);
            }
        }
        Part("Roof",new Vector3(0,9.1f,0),new Vector3(width+.6f,.35f,8.6f),blue,root);
        Part("Entrance",new Vector3(0,1.25f,-4.4f),new Vector3(2.1f,2.5f,.25f),dark,root);
        Part("Canopy",new Vector3(0,3,-5.4f),new Vector3(5,.25f,3.2f),blue,root);
        Part("School sign",new Vector3(0,7.75f,-4.4f),new Vector3(9,.9f,.12f),blue,root);
        Label(name,new Vector3(0,7.75f,-4.5f),.07f,root);
        Part("Entrance steps",new Vector3(0,.12f,-5.8f),new Vector3(5,.24f,3),paving,root);
    }
    static void Tree(Vector3 p, float scale = 1)
    {
        Part("Tree trunk",p+Vector3.up*1.5f*scale,new Vector3(.3f,1.5f,.3f)*scale,wood,null,PrimitiveType.Cylinder);
        Part("Tree crown",p+Vector3.up*3.4f*scale,new Vector3(2.5f,3.2f,2.5f)*scale,leaf,null,PrimitiveType.Sphere,false);
        Part("Tree crown top",p+Vector3.up*4.5f*scale,new Vector3(1.8f,2,1.8f)*scale,leaf,null,PrimitiveType.Sphere,false);
    }
    static Transform Limb(string name, Transform parent, Vector3 pivot, Vector3 center, Vector3 size, Material material)
    {
        var joint = new GameObject(name).transform; joint.SetParent(parent,false); joint.localPosition = pivot;
        Part(name+" mesh",center,size,material,joint,PrimitiveType.Capsule,false); return joint;
    }
    [MenuItem("Tools/Codex/Build Campus")]
    public static void Build()
    {
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(current.isDirty && !string.IsNullOrEmpty(current.path)) EditorSceneManager.SaveScene(current);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        cream=Mat("Warm plaster",new Color(.89f,.85f,.72f)); brick=Mat("Brick",new Color(.55f,.25f,.18f)); blue=Mat("Campus blue",new Color(.12f,.28f,.48f));
        glass=Mat("Window glass",new Color(.26f,.53f,.66f)); paving=Mat("Paving",new Color(.65f,.65f,.61f)); grass=Mat("Grass",new Color(.3f,.51f,.29f)); dark=Mat("Charcoal",new Color(.10f,.12f,.17f));
        white=Mat("White",new Color(.95f,.95f,.92f)); skin=Mat("Skin",new Color(.95f,.72f,.52f)); hair=Mat("Hair",new Color(.06f,.065f,.11f)); leaf=Mat("Leaves",new Color(.15f,.36f,.19f)); wood=Mat("Wood",new Color(.35f,.20f,.12f)); track=Mat("Track",new Color(.64f,.25f,.18f));
        Part("Campus ground",new Vector3(0,-.2f,4),new Vector3(90,.4f,90),grass);
        Part("Main avenue",new Vector3(0,.015f,-7),new Vector3(9,.03f,54),paving);
        Part("Cross avenue",new Vector3(0,.035f,10),new Vector3(66,.035f,7),paving);
        Part("Courtyard",new Vector3(0,.025f,21),new Vector3(27,.04f,19),paving);
        Building("HACKTHORN SCHOOL",new Vector3(0,0,37),9);
        Building("SCIENCE WING",new Vector3(-30,0,20),5);
        Building("LIBRARY",new Vector3(30,0,20),5);
        Part("Sports court",new Vector3(27,.04f,-12),new Vector3(22,.06f,30),track);
        for(int i=0;i<5;i++) Part("Court line",new Vector3(18+i*4.5f,.08f,-12),new Vector3(.09f,.015f,28),white,null,PrimitiveType.Cube,false);
        foreach(float z in new float[]{-25,1}) Part("Court end line",new Vector3(27,.08f,z),new Vector3(21,.015f,.09f),white,null,PrimitiveType.Cube,false);
        Part("Sports net",new Vector3(27,1.2f,-12),new Vector3(22,.04f,.05f),white);
        for(int i=0;i<8;i++) { Tree(new Vector3(-8,0,-27+i*7),.8f+(i%3)*.1f); Tree(new Vector3(8,0,-27+i*7),.9f); }
        for(int i=0;i<5;i++) Tree(new Vector3(-25+i*5,0,28),.9f);
        for(int i=0;i<4;i++)
        {
            float z=-20+i*12;
            Part("Bench seat",new Vector3(-12,.55f,z),new Vector3(3,.2f,.8f),wood);
            Part("Bench back",new Vector3(-12,1,z+.35f),new Vector3(3,.9f,.12f),wood);
            foreach(float x in new float[]{-13,-11}) Part("Bench leg",new Vector3(x,.25f,z),new Vector3(.15f,.5f,.65f),dark);
            Part("Lamp post",new Vector3(5.6f,1.8f,z),new Vector3(.12f,3.6f,.12f),dark);
            Part("Lamp",new Vector3(5.6f,3.8f,z),new Vector3(.45f,.4f,.45f),white);
        }
        Part("Gate left",new Vector3(-5.6f,2,-31),new Vector3(1.1f,4,1.1f),brick);
        Part("Gate right",new Vector3(5.6f,2,-31),new Vector3(1.1f,4,1.1f),brick);
        Part("Gate header",new Vector3(0,4.3f,-31),new Vector3(12.3f,.75f,1),blue);
        Label("HACKTHORN CAMPUS",new Vector3(0,4.3f,-31.55f),.055f);
        foreach(float x in new float[]{-44,44}) Part("Boundary",new Vector3(x,.65f,4),new Vector3(.4f,1.3f,88),brick);
        foreach(float z in new float[]{-40,48}) Part("Boundary",new Vector3(0,.65f,z),new Vector3(88,1.3f,.4f),brick);
        var sun=new GameObject("Afternoon sun").AddComponent<Light>(); sun.type=LightType.Directional; sun.intensity=2.1f; sun.shadows=LightShadows.Soft; sun.transform.rotation=Quaternion.Euler(38,-32,0); sun.color=new Color(1,.94f,.84f);
        RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(.62f,.70f,.79f); RenderSettings.fog=true; RenderSettings.fogColor=new Color(.72f,.82f,.90f); RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=45; RenderSettings.fogEndDistance=140;
        var player=new GameObject("Player - temporary student"); player.transform.position=new Vector3(0,.1f,-23);
        var cc=player.AddComponent<CharacterController>(); cc.height=1.85f; cc.radius=.3f; cc.center=new Vector3(0,.95f,0); cc.stepOffset=.28f;
        var vis=new GameObject("Student visual").transform; vis.SetParent(player.transform,false);
        Part("White jacket",new Vector3(0,1.12f,0),new Vector3(.62f,.62f,.34f),white,vis,PrimitiveType.Capsule,false);
        Part("Blue hood",new Vector3(0,1.43f,0),new Vector3(.51f,.16f,.39f),blue,vis,PrimitiveType.Sphere,false);
        Part("Head",new Vector3(0,1.7f,0),new Vector3(.51f,.52f,.48f),skin,vis,PrimitiveType.Sphere,false);
        Part("Hair",new Vector3(0,1.88f,-.035f),new Vector3(.55f,.30f,.51f),hair,vis,PrimitiveType.Sphere,false);
        for(int i=0;i<5;i++) { var lockObj=Part("Hair lock",new Vector3((i-2)*.085f,1.85f,.16f),new Vector3(.14f,.22f,.13f),hair,vis,PrimitiveType.Capsule,false); lockObj.transform.localRotation=Quaternion.Euler(0,0,(i-2)*12); }
        foreach(float x in new float[]{-.1f,.1f}) Part("Eye",new Vector3(x,1.72f,.217f),new Vector3(.065f,.08f,.035f),blue,vis,PrimitiveType.Sphere,false);
        Part("Backpack",new Vector3(0,1.18f,-.25f),new Vector3(.5f,.58f,.23f),dark,vis,PrimitiveType.Capsule,false);
        var lLeg=Limb("Left leg",vis,new Vector3(-.16f,.85f,0),new Vector3(0,-.30f,0),new Vector3(.25f,.31f,.25f),dark);
        var rLeg=Limb("Right leg",vis,new Vector3(.16f,.85f,0),new Vector3(0,-.30f,0),new Vector3(.25f,.31f,.25f),dark);
        foreach(var leg in new Transform[]{lLeg,rLeg}) { Part("Shoe",new Vector3(0,-.72f,.08f),new Vector3(.28f,.18f,.42f),white,leg,PrimitiveType.Cube,false); Part("Shoe stripe",new Vector3(0,-.66f,.17f),new Vector3(.29f,.06f,.13f),blue,leg,PrimitiveType.Cube,false); }
        var lArm=Limb("Left arm",vis,new Vector3(-.35f,1.4f,0),new Vector3(0,-.23f,0),new Vector3(.22f,.28f,.22f),white);
        var rArm=Limb("Right arm",vis,new Vector3(.35f,1.4f,0),new Vector3(0,-.23f,0),new Vector3(.22f,.28f,.22f),white);
        foreach(var arm in new Transform[]{lArm,rArm}) Part("Hand",new Vector3(0,-.52f,0),new Vector3(.17f,.18f,.17f),skin,arm,PrimitiveType.Sphere,false);
        var originalModel=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/character1.fbx");
        if(originalModel)
        {
            foreach(Transform child in vis) child.gameObject.SetActive(false);
            var model=(GameObject)PrefabUtility.InstantiatePrefab(originalModel); model.transform.SetParent(vis,false);
            var mesh=model.GetComponent<MeshFilter>().sharedMesh; var bounds=mesh.bounds;
            float scale=1.9f/bounds.size.z;
            model.transform.localRotation=Quaternion.Euler(0,-90,0)*Quaternion.Euler(-90,0,0);model.transform.localScale=Vector3.one*scale;
            model.transform.localPosition=new Vector3(-bounds.center.x*scale,-bounds.min.z*scale,bounds.center.y*scale);
            var renderer=model.GetComponent<Renderer>(); var sourceMat=renderer.sharedMaterial;
            var characterMat=new Material(Shader.Find("Campus/StudentWalk"));
            characterMat.name="Student textured walk";
            if(sourceMat.HasProperty("_BaseMap")) characterMat.SetTexture("_BaseMap",sourceMat.GetTexture("_BaseMap"));
            else characterMat.SetTexture("_BaseMap",sourceMat.mainTexture);
            if(!characterMat.GetTexture("_BaseMap")) characterMat.SetColor("_BaseColor",new Color(.65f,.69f,.76f));
            characterMat.SetFloat("_MeshHeight",bounds.size.z);characterMat.SetFloat("_MeshBottom",bounds.min.z);
            string matPath="Assets/Campus/Student textured walk.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(existing) { EditorUtility.CopySerialized(characterMat,existing);Object.DestroyImmediate(characterMat);characterMat=existing; }
            else AssetDatabase.CreateAsset(characterMat,matPath);
            renderer.sharedMaterial=characterMat;model.AddComponent<CampusMeshWalk>();
            foreach(var collider in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
            player.name="Player - character1";
        }
        var cam=new GameObject("Main Camera").AddComponent<Camera>(); cam.tag="MainCamera"; cam.fieldOfView=58; cam.backgroundColor=new Color(.63f,.77f,.88f); cam.clearFlags=CameraClearFlags.SolidColor; cam.gameObject.AddComponent<AudioListener>();
        cam.transform.position=player.transform.position+new Vector3(0,3,-5.5f); cam.transform.LookAt(player.transform.position+Vector3.up*1.4f);
        var walker=player.AddComponent<CampusWalker>(); walker.visual=vis; walker.leftLeg=lLeg; walker.rightLeg=rLeg; walker.leftArm=lArm; walker.rightArm=rArm; walker.followCamera=cam;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Campus/Scenes/CampusWalk.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Campus/Scenes/CampusWalk.unity",true)};
        AssetDatabase.SaveAssets(); Selection.activeGameObject=player;
        if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0,2,8),Quaternion.Euler(35,0,0),42);
        Debug.Log("Campus walk ready: WASD / Shift / Space / click to look.");
    }
}



