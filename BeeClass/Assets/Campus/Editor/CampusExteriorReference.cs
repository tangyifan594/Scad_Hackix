using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class CampusExteriorReference
{
    const string Marker="Library/CodexCampusExteriorV2.done";
    static Material stone, coral, slate, glass, green, gold, bark, iron;
    static CampusExteriorReference(){}
    static void AutoApply()
    {
        if(File.Exists(Marker))return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=AutoApply;return;}
        Scene previous=SceneManager.GetActiveScene();
        Scene target=SceneManager.GetSceneByPath("Assets/Scenes/Campus.unity");
        bool opened=!target.isLoaded;
        if(opened)target=EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity",OpenSceneMode.Additive);
        SceneManager.SetActiveScene(target);
        try{Build();File.WriteAllText(Marker,"Applied reference campus exterior");}
        finally{if(opened)EditorSceneManager.CloseScene(target,true);if(previous.isLoaded)SceneManager.SetActiveScene(previous);}
    }
    static Material Mat(string name,Color color)
    {
        string path="Assets/Campus/Art/Exterior "+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Smoothness",.1f);return m;
    }
    static GameObject Box(Transform root,string name,Vector3 p,Vector3 scale,Material m,bool solid=false)
    {
        GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static Mesh crown;
    static void Tree(Transform root,float x,float z,int index)
    {
        Box(root,"Tree trunk",new Vector3(x,1.3f,z),new Vector3(.3f,2.6f,.3f),bark);
        var g=new GameObject("Faceted tree crown");g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(x,3.3f,z);g.transform.localScale=new Vector3(2.5f,3.2f,2.5f);g.AddComponent<MeshFilter>().sharedMesh=crown;g.AddComponent<MeshRenderer>().sharedMaterial=index%3==0?gold:green;
    }
    static Mesh Crown()
    {
        string path="Assets/Campus/Art/Faceted campus crown.asset";Mesh m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m)return m;
        Vector3[] points={new Vector3(0,.7f,0),new Vector3(0,-.5f,0),new Vector3(-.5f,0,-.3f),new Vector3(.5f,0,-.3f),new Vector3(.5f,0,.3f),new Vector3(-.5f,0,.3f)};
        int[] idx={0,3,2,0,4,3,0,5,4,0,2,5,1,2,3,1,3,4,1,4,5,1,5,2};
        Vector3[] v=new Vector3[idx.Length];int[] t=new int[idx.Length];for(int i=0;i<idx.Length;i++){v[i]=points[idx[i]];t[i]=i;}
        m=new Mesh();m.name="Faceted canopy";m.vertices=v;m.triangles=t;m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
    }
    [MenuItem("Tools/Codex/Apply Campus Reference Exterior")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)return;
        Scene scene=SceneManager.GetActiveScene();GameObject school=null;
        foreach(var g in scene.GetRootGameObjects())if(g.name=="Three floor school - 9 classrooms")school=g;
        if(!school)throw new System.Exception("Open the Campus scene containing the three-floor school.");
        foreach(var g in scene.GetRootGameObjects())if(g.name=="Harbor ambience" || g.name=="Reference campus exterior")Object.DestroyImmediate(g);
        stone=Mat("Warm limestone",new Color(.83f,.76f,.65f));coral=Mat("Terracotta",new Color(.70f,.39f,.31f));slate=Mat("Slate",new Color(.36f,.39f,.46f));glass=Mat("Windows",new Color(.29f,.38f,.44f));green=Mat("Foliage",new Color(.49f,.58f,.29f));gold=Mat("Autumn",new Color(.81f,.62f,.27f));bark=Mat("Bark",new Color(.36f,.25f,.19f));iron=Mat("Iron",new Color(.19f,.23f,.26f));crown=Crown();
        Transform root=new GameObject("Reference campus exterior").transform;
        Box(root,"Campus ground",new Vector3(0,-.39f,0),new Vector3(130,.3f,120),green,true);
        Box(root,"Paved forecourt",new Vector3(0,-.14f,-17),new Vector3(38,.18f,25),stone,true);
        Box(root,"Arrival road",new Vector3(0,-.15f,-34),new Vector3(100,.14f,9),slate,true);
        for(int i=-3;i<=3;i++)Box(root,"Pedestrian crossing",new Vector3(i*.8f,-.07f,-34),new Vector3(.4f,.02f,7),stone);
        for(int side=-1;side<=1;side+=2)
        {
            for(int i=0;i<5;i++)
            {
                float x=side*(10+i%2*5),z=-9-i*4;
                Box(root,"Raised planter",new Vector3(x,.12f,z),new Vector3(3.3f,.3f,3.1f),stone,true);Tree(root,x,z,i);
                float lampX=side*7.5f;
                Box(root,"Lamp post",new Vector3(lampX,1.7f,z),new Vector3(.12f,3.4f,.12f),iron);
                Box(root,"Lamp lantern",new Vector3(lampX,3.45f,z),new Vector3(.38f,.55f,.38f),stone);
                Box(root,"Campus banner",new Vector3(lampX+side*.4f,2.4f,z),new Vector3(.65f,1.1f,.06f),i%2==0?coral:slate);
                Box(root,"Bench seat",new Vector3(side*5.5f,.48f,z),new Vector3(2,.16f,.6f),bark,true);
                Box(root,"Bench back",new Vector3(side*5.5f,.84f,z+.25f),new Vector3(2,.65f,.12f),bark);
            }
            Box(root,"Gate pillar",new Vector3(side*4.8f,1.8f,-29),new Vector3(1,3.6f,1),stone,true);
            Box(root,"Gate cap",new Vector3(side*4.8f,3.7f,-29),new Vector3(1.25f,.2f,1.25f),stone);
            for(int i=0;i<16;i++)Box(root,"Fence rail",new Vector3(side*(6+i*.7f),.85f,-29),new Vector3(.07f,1.7f,.07f),iron);
            Box(root,"Fence beam",new Vector3(side*11.3f,1.2f,-29),new Vector3(11,.08f,.08f),iron);
            for(int house=0;house<3;house++)
            {
                float x=side*(25+house*12),z=house%2==0?8:-17;
                Box(root,"Background campus building",new Vector3(x,3,z),new Vector3(9,6,10),stone,true);
                Box(root,"Building cornice",new Vector3(x,6.1f,z),new Vector3(9.5f,.35f,10.5f),coral);
                for(int row=0;row<2;row++)for(int col=-1;col<=1;col++)Box(root,"Building window",new Vector3(x+col*2.4f,1.8f+row*2.5f,z-5.05f),new Vector3(1.15f,1.6f,.08f),glass);
            }
        }
        // Exterior details do not obstruct the existing doorway or elevator.
        for(int floor=0;floor<3;floor++)
        {
            Box(root,"Facade horizontal band",new Vector3(0,floor*4+3.8f,-4.22f),new Vector3(24.4f,.23f,.4f),stone);
            for(int side=-1;side<=1;side+=2)Box(root,"Facade end pier",new Vector3(side*11.7f,floor*4+1.9f,-4.19f),new Vector3(.55f,3.7f,.25f),stone);
        }
        Box(root,"Entrance canopy",new Vector3(0,3,-5.1f),new Vector3(4.5f,.2f,2.2f),coral);
        AddReferenceFacade(root,school);
        RenderSettings.ambientLight=new Color(.75f,.72f,.66f);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Reference campus exterior saved. ClassroomAnimals populates rooms on Play.");
    }
    static void AddReferenceFacade(Transform root,GameObject school)
    {
        foreach(Transform t in school.GetComponentsInChildren<Transform>(true))
            if(t.name.Contains("outer wall") || t.name=="Entrance front wall" || t.name=="Entrance lintel")
            {var r=t.GetComponent<Renderer>();if(r)r.sharedMaterial=stone;}
        // The main entrance remains at x=0; the decorative mural tower sits beside it.
        Box(root,"Mural tower",new Vector3(4.6f,6.5f,-4.5f),new Vector3(4.6f,13,1),stone);
        Material[] colors={coral,Mat("Mural honey",new Color(.95f,.74f,.36f)),Mat("Mural blue",new Color(.40f,.52f,.70f)),Mat("Mural ivory",new Color(.96f,.90f,.78f)),Mat("Mural sage",new Color(.63f,.75f,.70f))};
        var v=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>[colors.Length];for(int i=0;i<indices.Length;i++)indices[i]=new System.Collections.Generic.List<int>();
        for(int row=0;row<7;row++)for(int col=0;col<2;col++)
        {
            float left=2.55f+col*2.05f,right=left+2.05f,bottom=.35f+row*1.75f,top=bottom+1.75f;
            Vector3 a=new Vector3(left,bottom,-5.012f),b=new Vector3(right,bottom,-5.012f),c=new Vector3(right,top,-5.012f),d=new Vector3(left,top,-5.012f);
            Vector3 middle=new Vector3(left+.65f+(row%2)*.6f,bottom+.55f+(col%2)*.5f,-5.012f);
            Vector3[] perimeter={a,b,c,d};for(int face=0;face<4;face++){int k=v.Count;v.Add(middle);v.Add(perimeter[(face+1)%4]);v.Add(perimeter[face]);indices[(row*3+col+face)%colors.Length].AddRange(new[]{k,k+1,k+2});}
        }
        string meshPath="Assets/Campus/Art/Reference geometric mural.asset";Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}mesh.Clear();mesh.SetVertices(v);mesh.subMeshCount=colors.Length;for(int i=0;i<colors.Length;i++)mesh.SetTriangles(indices[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        GameObject mural=new GameObject("Colorful geometric mural");mural.transform.SetParent(root,false);mural.AddComponent<MeshFilter>().sharedMesh=mesh;mural.AddComponent<MeshRenderer>().sharedMaterials=colors;
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*23;
            Box(root,"Long teaching wing",new Vector3(x,5.8f,5),new Vector3(22,11.6f,18),stone,true);
            Box(root,"Dark ground floor facade",new Vector3(x,1.8f,-4.09f),new Vector3(22,3.6f,.12f),slate);
            for(int floor=0;floor<3;floor++)
            {
                Box(root,"Wing cornice",new Vector3(x,floor*4+3.85f,-4.22f),new Vector3(22.2f,.24f,.42f),stone);
                for(int window=0;window<8;window++)
                {
                    float wx=x-9.2f+window*2.6f;
                    Box(root,"Tall recessed window",new Vector3(wx,floor*4+1.8f,-4.18f),new Vector3(1.25f,2,.08f),glass);
                    Box(root,"Window golden lintel",new Vector3(wx,floor*4+2.7f,-4.24f),new Vector3(1.26f,.2f,.06f),colors[1]);
                    Box(root,"Window sill",new Vector3(wx,floor*4+.77f,-4.26f),new Vector3(1.45f,.12f,.20f),stone);
                }
            }
            Box(root,"Wing roof coping",new Vector3(x,11.7f,5),new Vector3(22.6f,.3f,18.6f),stone);
            for(int bush=0;bush<15;bush++)
            {
                GameObject g=new GameObject("Low polygon hedge");g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(side*(4+bush*1.8f),.55f,-6.9f);g.transform.localScale=new Vector3(1.6f,1.3f,1.5f);g.AddComponent<MeshFilter>().sharedMesh=crown;g.AddComponent<MeshRenderer>().sharedMaterial=bush%4==0?gold:green;
            }
        }
        // Open forecourt as in the reference, without a fence across the plaza.
        var remove=new System.Collections.Generic.List<GameObject>();foreach(Transform t in root)if(t.name.StartsWith("Gate ") || t.name.StartsWith("Fence "))remove.Add(t.gameObject);foreach(var g in remove)Object.DestroyImmediate(g);
        Material sky=Mat("Cloud ivory",new Color(.94f,.91f,.88f));
        for(int cloud=0;cloud<5;cloud++)for(int puff=0;puff<3;puff++)
        {
            var g=new GameObject("Faceted cloud");g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(-45+cloud*22+puff*3,23+cloud%2*4,25+cloud%3*12);g.transform.localScale=new Vector3(7,3+puff%2,4);g.AddComponent<MeshFilter>().sharedMesh=crown;g.AddComponent<MeshRenderer>().sharedMaterial=sky;
        }
        for(int mountain=0;mountain<9;mountain++)
        {
            var g=new GameObject("Distant faceted hill");g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(-95+mountain*24,3,68);g.transform.localScale=new Vector3(42,30+mountain%3*9,35);g.AddComponent<MeshFilter>().sharedMesh=crown;g.AddComponent<MeshRenderer>().sharedMaterial=Mat("Mountain "+mountain%2,mountain%2==0?new Color(.43f,.56f,.59f):new Color(.51f,.61f,.52f));
        }
    }
}
