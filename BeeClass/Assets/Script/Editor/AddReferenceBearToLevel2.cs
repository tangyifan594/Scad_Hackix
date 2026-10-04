#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time additive creation requested for Level2-shinei only.
public static class AddReferenceBearToLevel2
{
    const string ScenePath = "Assets/Scenes/Level2-shinei.unity";
    const string Folder = "Assets/Art/ReferenceBear";
    const string Marker = "Library/ReferenceBear-Level2.done";
    const string Name = "Reference Bear - brown low poly";

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        if(!File.Exists(Marker))EditorApplication.delayCall += Install;
    }

    [MenuItem("Tools/BeeClass/Add Reference Bear To Level2")]
    static void Install()
    {
        if(File.Exists(Marker))return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Install;
            return;
        }
        if(!File.Exists(ScenePath))return;
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool openedHere = !scene.IsValid() || !scene.isLoaded;
        if(openedHere)scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        try
        {
            foreach(var existing in scene.GetRootGameObjects())
                if(existing.name == Name){File.WriteAllText(Marker, "Already added");return;}
            Directory.CreateDirectory("Archive/ReferenceBear");
            const string backup = "Archive/ReferenceBear/Level2-shinei-before-bear.unity";
            if(!File.Exists(backup))File.Copy(ScenePath, backup);
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art", "ReferenceBear");
            Material fur = MakeMaterial("Warm brown fur", new Color(.48f,.27f,.16f));
            Material cream = MakeMaterial("Cream muzzle and belly", new Color(.83f,.71f,.55f));
            Material paw = MakeMaterial("Dark brown paws", new Color(.32f,.17f,.095f));
            Material dark = MakeMaterial("Black eyes nose and mouth", new Color(.025f,.022f,.018f));
            Mesh shape = MakeFacetedMesh();
            var rabbit = new GameObject(Name);
            SceneManager.MoveGameObjectToScene(rabbit,scene);
            rabbit.transform.position = new Vector3(-1.9f,.1f,0f);
            rabbit.transform.localScale = Vector3.one * .72f;
            Part(rabbit.transform,"Body",new Vector3(0,.64f,0),new Vector3(.86f,1.08f,.65f),shape,fur);
            Part(rabbit.transform,"Cream belly",new Vector3(0,.61f,.315f),new Vector3(.48f,.78f,.10f),shape,cream);
            Part(rabbit.transform,"Head",new Vector3(0,1.42f,.01f),new Vector3(1.12f,.95f,.87f),shape,fur);
            Part(rabbit.transform,"Cream muzzle",new Vector3(0,1.25f,.445f),new Vector3(.46f,.34f,.26f),shape,cream);
            Part(rabbit.transform,"Black nose",new Vector3(0,1.335f,.587f),new Vector3(.22f,.125f,.075f),shape,dark);
            Part(rabbit.transform,"Mouth stem",new Vector3(0,1.235f,.585f),new Vector3(.016f,.105f,.014f),shape,dark);
            Part(rabbit.transform,"Mouth line",new Vector3(0,1.185f,.582f),new Vector3(.17f,.016f,.014f),shape,dark);
            Mesh ears = MakeEarMesh();
            for(int side=-1;side<=1;side+=2)
            {
                Part(rabbit.transform,"Eye "+side,new Vector3(side*.245f,1.49f,.454f),new Vector3(.105f,.19f,.025f),shape,dark);
                var ear=Part(rabbit.transform,"Round ear "+side,new Vector3(side*.43f,1.87f,0),new Vector3(.37f,.37f,.20f),ears,fur);
                ear.localRotation=Quaternion.Euler(0,0,side*12);
                Part(ear,"Pale inner ear",new Vector3(0,0,.51f),new Vector3(.57f,.57f,.06f),ears,cream);
                var arm=Part(rabbit.transform,"Arm "+side,new Vector3(side*.49f,.72f,0),new Vector3(.31f,.70f,.34f),shape,fur);
                arm.localRotation=Quaternion.Euler(0,0,side*18);
                Part(rabbit.transform,"Paw "+side,new Vector3(side*.60f,.405f,.015f),new Vector3(.27f,.20f,.29f),shape,paw);
                Part(rabbit.transform,"Leg "+side,new Vector3(side*.235f,.205f,.02f),new Vector3(.32f,.41f,.39f),shape,fur);
                Part(rabbit.transform,"Foot "+side,new Vector3(side*.235f,.075f,.15f),new Vector3(.30f,.15f,.32f),shape,paw);
            }
            Part(rabbit.transform,"Short tail",new Vector3(0,.43f,-.375f),new Vector3(.23f,.24f,.24f),shape,fur);
            PrefabUtility.SaveAsPrefabAsset(rabbit,Folder+"/Reference Bear.prefab");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Marker,"Added one bear to "+ScenePath);
            Debug.Log("Added Reference Bear to Level2-shinei. Existing objects were not edited.",rabbit);
        }
        finally
        {
            if(openedHere && scene.IsValid() && scene.isLoaded)EditorSceneManager.CloseScene(scene,true);
        }
    }

    static Transform Part(Transform parent,string name,Vector3 position,Vector3 scale,Mesh mesh,Material material)
    {
        var part=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
        part.transform.SetParent(parent,false);
        part.transform.localPosition=position;
        part.transform.localScale=scale;
        part.GetComponent<MeshFilter>().sharedMesh=mesh;
        part.GetComponent<MeshRenderer>().sharedMaterial=material;
        return part.transform;
    }

    static Material MakeMaterial(string name,Color color)
    {
        string path=Folder+"/"+name+".mat";
        var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(existing)return existing;
        Shader shader=Shader.Find("Universal Render Pipeline/Lit");
        if(!shader)shader=Shader.Find("Standard");
        var material=new Material(shader){name=name,color=color};
        if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.05f);
        AssetDatabase.CreateAsset(material,path);
        return material;
    }

    static Mesh MakeEarMesh()
    {
        const string path=Folder+"/Octagonal bear ear.asset";
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing)return existing;
        var vertices=new List<Vector3>();
        var triangles=new List<int>();
        System.Action<Vector3,Vector3,Vector3> tri=(a,b,c)=>{int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);};
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4f,b=(i+1)*Mathf.PI/4f;
            Vector3 frontA=new Vector3(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f,.5f);
            Vector3 frontB=new Vector3(Mathf.Cos(b)*.5f,Mathf.Sin(b)*.5f,.5f);
            Vector3 backA=new Vector3(frontA.x,frontA.y,-.5f),backB=new Vector3(frontB.x,frontB.y,-.5f);
            tri(new Vector3(0,0,.5f),frontA,frontB);
            tri(new Vector3(0,0,-.5f),backB,backA);
            tri(frontA,backA,frontB);tri(frontB,backA,backB);
        }
        var mesh=new Mesh{name="Octagonal bear ear"};
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }

    static Mesh MakeFacetedMesh()
    {
        const string path=Folder+"/Faceted bear shape.asset";
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing)return existing;
        // Octagonal bevelled rings with flat triangle normals, matching the reference's facets.
        var vertices=new List<Vector3>();
        var triangles=new List<int>();
        Vector2[] ring={new Vector2(-.34f,-.5f),new Vector2(.34f,-.5f),new Vector2(.5f,-.34f),new Vector2(.5f,.34f),new Vector2(.34f,.5f),new Vector2(-.34f,.5f),new Vector2(-.5f,.34f),new Vector2(-.5f,-.34f)};
        float[] heights={-.5f,-.30f,.30f,.5f};
        float[] widths={.65f,1f,1f,.65f};
        System.Action<Vector3,Vector3,Vector3> tri=(a,b,c)=>{int index=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(index);triangles.Add(index+1);triangles.Add(index+2);};
        for(int layer=0;layer<3;layer++)
            for(int i=0;i<8;i++)
            {
                int j=(i+1)%8;
                Vector3 a=new Vector3(ring[i].x*widths[layer],heights[layer],ring[i].y*widths[layer]);
                Vector3 b=new Vector3(ring[j].x*widths[layer],heights[layer],ring[j].y*widths[layer]);
                Vector3 c=new Vector3(ring[i].x*widths[layer+1],heights[layer+1],ring[i].y*widths[layer+1]);
                Vector3 d=new Vector3(ring[j].x*widths[layer+1],heights[layer+1],ring[j].y*widths[layer+1]);
                tri(a,c,b);tri(b,c,d);
            }
        for(int i=0;i<8;i++)
        {
            int j=(i+1)%8;
            tri(new Vector3(0,-.5f,0),new Vector3(ring[i].x*.65f,-.5f,ring[i].y*.65f),new Vector3(ring[j].x*.65f,-.5f,ring[j].y*.65f));
            tri(new Vector3(0,.5f,0),new Vector3(ring[j].x*.65f,.5f,ring[j].y*.65f),new Vector3(ring[i].x*.65f,.5f,ring[i].y*.65f));
        }
        var mesh=new Mesh{name="Faceted bear shape"};
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }
}
#endif
