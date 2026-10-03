using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

[InitializeOnLoad]
public static class CampusFacetedBee
{
    const string Marker="Library/CodexFacetedBeeV1.done";
    static Material honey,dark,cream,pink; static Mesh bevel;
    static CampusFacetedBee(){}
    static void AutoApply()
    {
        if(File.Exists(Marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=AutoApply;return;}
        Scene previous=SceneManager.GetActiveScene(),target=SceneManager.GetSceneByPath("Assets/Scenes/Campus.unity");bool opened=!target.isLoaded;
        if(opened)target=EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity",OpenSceneMode.Additive);
        SceneManager.SetActiveScene(target);
        try{Build();File.WriteAllText(Marker,"Reference faceted bee saved");}
        finally{if(opened)EditorSceneManager.CloseScene(target,true);if(previous.isLoaded)SceneManager.SetActiveScene(previous);}
    }
    static Material Mat(string name,Color color)
    {
        string path="Assets/Campus/Art/Faceted bee "+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",.05f);return m;
    }
    static Mesh Bevel()
    {
        string path="Assets/Campus/Art/Faceted bee bevel.asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m)return m;
        // Octagonal cross-section with inset end caps, and separate face normals.
        var vertices=new System.Collections.Generic.List<Vector3>();var triangles=new System.Collections.Generic.List<int>();
        Vector2[] ring={new Vector2(-.32f,.5f),new Vector2(.32f,.5f),new Vector2(.5f,.32f),new Vector2(.5f,-.32f),new Vector2(.32f,-.5f),new Vector2(-.32f,-.5f),new Vector2(-.5f,-.32f),new Vector2(-.5f,.32f)};
        float[] z={-.5f,-.34f,.34f,.5f};float[] s={.74f,1,1,.74f};
        for(int layer=0;layer<3;layer++)for(int i=0;i<8;i++)
        {
            int j=(i+1)%8;Vector3 a=new Vector3(ring[i].x*s[layer],ring[i].y*s[layer],z[layer]),b=new Vector3(ring[j].x*s[layer],ring[j].y*s[layer],z[layer]),c=new Vector3(ring[j].x*s[layer+1],ring[j].y*s[layer+1],z[layer+1]),d=new Vector3(ring[i].x*s[layer+1],ring[i].y*s[layer+1],z[layer+1]);
            int k=vertices.Count;vertices.AddRange(new[]{a,b,c,d});triangles.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});
        }
        for(int end=0;end<2;end++)for(int i=0;i<8;i++)
        {
            int layer=end==0?0:3,j=(i+1)%8,k=vertices.Count;vertices.Add(new Vector3(0,0,z[layer]));vertices.Add(new Vector3(ring[i].x*s[layer],ring[i].y*s[layer],z[layer]));vertices.Add(new Vector3(ring[j].x*s[layer],ring[j].y*s[layer],z[layer]));triangles.AddRange(end==0?new[]{k,k+1,k+2}:new[]{k,k+2,k+1});
        }
        m=new Mesh{name="Chamfered low polygon bee"};m.SetVertices(vertices);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
    }
    static Transform Part(Transform parent,string name,Vector3 position,Vector3 scale,Material material,bool chamfer=true)
    {
        GameObject g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;
        g.AddComponent<MeshFilter>().sharedMesh=bevel;g.AddComponent<MeshRenderer>().sharedMaterial=material;return g.transform;
    }
    static void Rod(Transform parent,string name,Vector3 start,Vector3 end,float width,Material material)
    {
        Transform rod=Part(parent,name,(start+end)*.5f,new Vector3(width,width,(end-start).magnitude),material);rod.localRotation=Quaternion.LookRotation(end-start);
    }
    [MenuItem("Tools/Codex/Apply Faceted Reference Bee")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)return;
        Scene scene=SceneManager.GetActiveScene();CampusWalker player=null;
        foreach(var root in scene.GetRootGameObjects()){player=root.GetComponentInChildren<CampusWalker>(true);if(player)break;}
        if(!player)throw new System.Exception("Bee player missing in the active scene.");
        honey=Mat("Honey",new Color(.96f,.70f,.25f));dark=Mat("Charcoal",new Color(.23f,.20f,.18f));cream=Mat("Ivory wings",new Color(.91f,.89f,.85f));pink=Mat("Cheeks",new Color(.90f,.56f,.33f));bevel=Bevel();
        if(player.visual)Object.DestroyImmediate(player.visual.gameObject);
        Transform bee=new GameObject("Bee visual - faceted aviator").transform;bee.SetParent(player.transform,false);player.visual=bee;
        player.leftLeg=null;player.rightLeg=null;player.leftArm=null;player.rightArm=null;
        Part(bee,"Honey body",new Vector3(0,.69f,-.13f),new Vector3(.84f,.82f,1.03f),honey);
        Part(bee,"Dark body stripe",new Vector3(0,.66f,-.27f),new Vector3(.86f,.80f,.25f),dark);
        Part(bee,"Yellow face",new Vector3(0,.83f,.38f),new Vector3(.91f,.82f,.47f),honey);
        Part(bee,"Tail cap",new Vector3(0,.57f,-.62f),new Vector3(.51f,.48f,.23f),dark);
        Rod(bee,"Stinger",new Vector3(0,.52f,-.67f),new Vector3(0,.48f,-.87f),.09f,dark);
        Part(bee,"Goggle strap",new Vector3(0,1.17f,.36f),new Vector3(.95f,.13f,.46f),dark);
        for(int side=-1;side<=1;side+=2)
        {
            Part(bee,"Square eye",new Vector3(side*.22f,.86f,.621f),new Vector3(.085f,.18f,.025f),dark);
            Part(bee,"Cheek",new Vector3(side*.29f,.68f,.608f),new Vector3(.16f,.10f,.025f),pink);
            Transform goggles=Part(bee,"Goggle frame",new Vector3(side*.23f,1.19f,.53f),new Vector3(.34f,.25f,.13f),dark);goggles.localRotation=Quaternion.Euler(-12,0,0);
            Part(goggles,"Ivory lens",new Vector3(0,0,.53f),new Vector3(.72f,.66f,.20f),cream);
            Vector3 basePoint=new Vector3(side*.25f,1.22f,.10f),joint=new Vector3(side*.35f,1.58f,.02f),tip=new Vector3(side*.49f,1.63f,.04f);
            Rod(bee,"Antenna stem",basePoint,joint,.055f,dark);Rod(bee,"Antenna elbow",joint,tip,.065f,dark);Part(bee,"Antenna tip",tip,new Vector3(.15f,.12f,.13f),dark);
            Transform wing=new GameObject(side<0?"Left wing":"Right wing").transform;wing.SetParent(bee,false);wing.localPosition=new Vector3(side*.39f,.84f,-.08f);wing.localRotation=Quaternion.Euler(12,side*15,side*-30);
            Part(wing,"Upper wing",new Vector3(side*.25f,.27f,0),new Vector3(.44f,.68f,.10f),cream);
            Part(wing,"Lower wing",new Vector3(side*.22f,-.15f,-.12f),new Vector3(.37f,.39f,.10f),cream);
            Rod(bee,"Little arm",new Vector3(side*.36f,.50f,.15f),new Vector3(side*.57f,.43f,.21f),.095f,dark);
            Part(bee,"Hand",new Vector3(side*.57f,.43f,.21f),Vector3.one*.16f,dark);
            Rod(bee,"Little foot",new Vector3(side*.20f,.36f,-.15f),new Vector3(side*.27f,.18f,-.20f),.11f,dark);
        }
        var motion=bee.gameObject.AddComponent<CampusBeeMotion>();motion.leftWing=bee.Find("Left wing");motion.rightWing=bee.Find("Right wing");
        PrefabUtility.SaveAsPrefabAsset(bee.gameObject,"Assets/Campus/Prefabs/FacetedAviatorBee.prefab");
        EditorUtility.SetDirty(player);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Faceted aviator bee saved.");
    }
}
