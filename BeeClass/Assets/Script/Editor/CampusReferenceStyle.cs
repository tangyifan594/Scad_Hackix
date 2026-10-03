using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class CampusReferenceStyle
{
    static Mesh sphere,rounded;static Material honey,cocoa,cream,blush,wood,coral,sage,plaster,water;
    static Material Mat(string name,Color color)
    {
        string p="Assets/Campus/Art/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.color=color;m.SetFloat("_Smoothness",.12f);return m;
    }
    static GameObject Shape(string name,Transform parent,Vector3 pos,Vector3 size,Material mat,bool ball=true,bool solid=false)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.AddComponent<MeshFilter>().sharedMesh=ball?sphere:rounded;g.AddComponent<MeshRenderer>().sharedMaterial=mat;if(solid)g.AddComponent<BoxCollider>();return g;
    }
    static Mesh RoofMesh()
    {
        string p="Assets/Campus/Art/Town gable roof.asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(m)return m;
        Vector3 a=new Vector3(-.5f,-.5f,-.5f),b=new Vector3(.5f,-.5f,-.5f),c=new Vector3(0,.5f,-.5f),d=new Vector3(-.5f,-.5f,.5f),e=new Vector3(.5f,-.5f,.5f),f=new Vector3(0,.5f,.5f);
        var vs=new[]{a,c,b,d,e,f,a,d,f,a,f,c,b,c,f,b,f,e,a,b,e,a,e,d};var ids=new int[vs.Length];for(int i=0;i<ids.Length;i++)ids[i]=i;m=new Mesh{name="Warm town pitched roof"};m.vertices=vs;m.triangles=ids;m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,p);return m;
    }
    static void Roof(string name,Transform root,Vector3 pos,Vector3 size){var g=Shape(name,root,pos,size,coral,false);g.GetComponent<MeshFilter>().sharedMesh=RoofMesh();}
    static void House(Transform root,Vector3 p,int index)
    {
        var h=new GameObject("Town house "+index).transform;h.SetParent(root,false);h.localPosition=p;
        Shape("Warm plaster",h,new Vector3(0,2,0),new Vector3(5,4,5),plaster,false,true);Roof("Red pitched roof",h,new Vector3(0,4.7f,0),new Vector3(5.5f,1.8f,5.6f));
        Shape("Timber door",h,new Vector3(0,1.1f,-2.56f),new Vector3(.9f,2.2f,.12f),wood,false);
        foreach(float x in new[]{-1.5f,1.5f}){Shape("Window frame",h,new Vector3(x,2.2f,-2.57f),new Vector3(1.1f,1.5f,.12f),cream,false);Shape("Dusty blue glass",h,new Vector3(x,2.2f,-2.65f),new Vector3(.85f,1.25f,.04f),water,false);}
        Shape("Awning",h,new Vector3(0,2.5f,-3),new Vector3(3,.12f,1.1f),coral,false);
    }
    [MenuItem("Tools/Codex/Apply Reference Bee And Town")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before applying reference style.");
        sphere=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Campus/Art/SmoothSphere.asset");rounded=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Campus/Art/RoundedCube.asset");
        honey=Mat("Reference honey",new Color(.91f,.62f,.23f));cocoa=Mat("Reference cocoa",new Color(.27f,.18f,.13f));cream=Mat("Reference cream",new Color(.96f,.88f,.69f));blush=Mat("Reference blush",new Color(.79f,.40f,.32f));wood=Mat("Reference timber",new Color(.54f,.36f,.23f));coral=Mat("Reference terracotta",new Color(.64f,.32f,.24f));sage=Mat("Reference sage",new Color(.43f,.55f,.43f));plaster=Mat("Reference plaster",new Color(.83f,.73f,.57f));water=Mat("Reference water blue",new Color(.39f,.57f,.62f));
        var player=Object.FindFirstObjectByType<CampusWalker>();if(player.visual)Object.DestroyImmediate(player.visual.gameObject);
        var bee=new GameObject("Bee visual").transform;bee.SetParent(player.transform,false);player.visual=bee;player.leftArm=null;player.rightArm=null;player.leftLeg=null;player.rightLeg=null;
        var body=Shape("Round honey body",bee,new Vector3(0,.71f,-.08f),new Vector3(1.05f,.92f,1.3f),honey);
        string bodyPath="Assets/Campus/Art/Reference bee body.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(bodyPath);
        if(!mesh){mesh=Object.Instantiate(sphere);mesh.name="Plump bee with soft flat base";var v=mesh.vertices;var uv=mesh.uv;for(int i=0;i<v.Length;i++){v[i].y=Mathf.Max(v[i].y,-.42f);uv[i].y=v[i].z+.5f;}mesh.vertices=v;mesh.uv=uv;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,bodyPath);}
        body.GetComponent<MeshFilter>().sharedMesh=mesh;
        var texture=new Texture2D(64,256);for(int y=0;y<256;y++)for(int x=0;x<64;x++){float t=(float)y/255;bool stripe=(t>.12f&&t<.28f)||(t>.46f&&t<.67f);float grain=(Mathf.Sin(x*1.7f+y*.8f)+Mathf.Cos(y*1.3f))*.002f;Color color=stripe?cocoa.color:honey.color;texture.SetPixel(x,y,color+new Color(grain,grain,grain,0));}
        texture.Apply();string texPath="Assets/Campus/Art/Reference bee stripes.png";System.IO.File.WriteAllBytes(texPath,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texPath);var bands=Mat("Reference soft stripes",Color.white);bands.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));body.GetComponent<Renderer>().sharedMaterial=bands;
        Shape("Round face",bee,new Vector3(0,.70f,.49f),new Vector3(.86f,.74f,.28f),honey);
        foreach(float x in new[]{-.20f,.20f})
        {
            Shape("Dot eye",bee,new Vector3(x,.73f,.632f),new Vector3(.095f,.14f,.04f),cocoa);
            Shape("Rosy cheek",bee,new Vector3(x*1.5f,.58f,.619f),new Vector3(.13f,.13f,.025f),blush);
            var antenna=Shape("Short antenna",bee,new Vector3(x,1.25f,.33f),new Vector3(.05f,.35f,.05f),cocoa);antenna.transform.localRotation=Quaternion.Euler(0,0,-Mathf.Sign(x)*15);
            Shape("Antenna round tip",bee,new Vector3(x*1.3f,1.43f,.33f),Vector3.one*.13f,cocoa);
        }
        var left=new GameObject("Left cream wing").transform;left.SetParent(bee,false);left.localPosition=new Vector3(-.21f,1.08f,-.21f);left.localRotation=Quaternion.Euler(-25,-18,-28);
        var right=new GameObject("Right cream wing").transform;right.SetParent(bee,false);right.localPosition=new Vector3(.21f,1.08f,-.21f);right.localRotation=Quaternion.Euler(-25,18,28);
        Shape("Left petal wing",left,new Vector3(0,.2f,0),new Vector3(.29f,.58f,.075f),cream);Shape("Right petal wing",right,new Vector3(0,.2f,0),new Vector3(.29f,.58f,.075f),cream);
        Shape("Small tail",bee,new Vector3(0,.66f,-.76f),new Vector3(.16f,.16f,.22f),cocoa);
        var motion=bee.gameObject.AddComponent<CampusBeeMotion>();motion.leftWing=left;motion.rightWing=right;PrefabUtility.SaveAsPrefabAsset(bee.gameObject,"Assets/Campus/Prefabs/BeePlayerVisual.prefab");
        var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/School warm walls.mat");wall.color=plaster.color;
        var blue=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/School blue.mat");blue.color=new Color(.37f,.47f,.52f);
        var chair=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Art/Muted teal.mat");chair.color=sage.color;
        var building=Object.FindFirstObjectByType<CampusElevator>().transform;foreach(var t in building.GetComponentsInChildren<Transform>())if(t.name=="Roof"){t.localPosition=new Vector3(0,13.2f,5);t.localScale=new Vector3(25,2.6f,19);t.GetComponent<MeshFilter>().sharedMesh=RoofMesh();t.GetComponent<Renderer>().sharedMaterial=coral;}
        var old=GameObject.Find("Harbor ambience");if(old)Object.DestroyImmediate(old);var town=new GameObject("Harbor ambience").transform;
        Shape("Calm canal",town,new Vector3(34,-.25f,3),new Vector3(20,.12f,52),water,false);
        Shape("Canal bank",town,new Vector3(24,-.08f,3),new Vector3(.5f,.35f,52),wood,false,true);
        for(int i=0;i<22;i++)Shape("Boardwalk plank",town,new Vector3(21+i*.48f,.02f,-10),new Vector3(.46f,.18f,5),wood,false,true);
        for(int i=0;i<6;i++){Shape("Dock post",town,new Vector3(22+i*1.8f,.4f,-12.2f),new Vector3(.3f,.95f,.3f),wood,false,true);Shape("Dock post",town,new Vector3(22+i*1.8f,.4f,-7.8f),new Vector3(.3f,.95f,.3f),wood,false,true);}
        House(town,new Vector3(-20,0,18),1);House(town,new Vector3(20,0,19),2);House(town,new Vector3(-20,0,-9),3);
        for(int i=0;i<4;i++){Shape("Dock crate",town,new Vector3(22+(i%2)*.85f,.5f,-9+(i/2)*.9f),new Vector3(.75f,.9f,.75f),wood,false,true);}
        var grass=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/School lawn.mat");grass.color=new Color(.49f,.57f,.42f);
        foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Directional){l.color=new Color(1,.88f,.69f);l.intensity=1.15f;l.shadowStrength=.45f;l.transform.rotation=Quaternion.Euler(48,-32,0);}
        RenderSettings.ambientLight=new Color(.73f,.71f,.65f);Camera.main.backgroundColor=new Color(.72f,.81f,.80f);
        EditorUtility.SetDirty(player);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
    }
}

