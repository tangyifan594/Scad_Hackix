#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time additive creation requested for Level2-shinei only.
public static class AddReferenceRabbitToLevel2
{
    const string ScenePath = "Assets/Scenes/Level2-shinei.unity";
    const string Folder = "Assets/Art/ReferenceRabbit";
    const string Marker = "Library/ReferenceRabbit-Level2.done";
    const string Name = "Reference Rabbit - cream low poly";

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        if(!File.Exists(Marker))EditorApplication.delayCall += Install;
    }

    [MenuItem("Tools/BeeClass/Add Reference Rabbit To Level2")]
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
            Directory.CreateDirectory("Archive/ReferenceRabbit");
            const string backup = "Archive/ReferenceRabbit/Level2-shinei-before-rabbit.unity";
            if(!File.Exists(backup))File.Copy(ScenePath, backup);
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art", "ReferenceRabbit");
            Material fur = MakeMaterial("Cream fur", new Color(.92f,.85f,.74f));
            Material belly = MakeMaterial("Pale belly", new Color(.98f,.93f,.84f));
            Material pink = MakeMaterial("Pink inner ears", new Color(.83f,.43f,.40f));
            Material black = MakeMaterial("Black eyes and mouth", new Color(.025f,.023f,.02f));
            Mesh shape = MakeFacetedMesh();
            var rabbit = new GameObject(Name);
            SceneManager.MoveGameObjectToScene(rabbit,scene);
            rabbit.transform.position = new Vector3(-1.9f,.1f,-3.2f);
            rabbit.transform.localScale = Vector3.one * .65f;
            Part(rabbit.transform,"Body",new Vector3(0,.58f,0),new Vector3(.73f,.98f,.55f),shape,fur);
            Part(rabbit.transform,"Belly",new Vector3(0,.56f,.27f),new Vector3(.44f,.65f,.07f),shape,belly);
            Part(rabbit.transform,"Head",new Vector3(0,1.31f,.03f),new Vector3(1.05f,.89f,.77f),shape,fur);
            for(int side=-1;side<=1;side+=2)
            {
                Part(rabbit.transform,"Foot "+side,new Vector3(side*.23f,.095f,.10f),new Vector3(.30f,.19f,.39f),shape,fur);
                var arm=Part(rabbit.transform,"Arm "+side,new Vector3(side*.44f,.68f,0),new Vector3(.23f,.65f,.25f),shape,fur);
                arm.localRotation=Quaternion.Euler(0,0,side*9);
                var ear=Part(rabbit.transform,"Ear "+side,new Vector3(side*.31f,2.02f,.015f),new Vector3(.32f,1.13f,.23f),shape,fur);
                ear.localRotation=Quaternion.Euler(0,0,-side*13);
                Part(ear,"Pink inset",new Vector3(0,.02f,.50f),new Vector3(.61f,.77f,.045f),shape,pink);
                Part(rabbit.transform,"Eye "+side,new Vector3(side*.245f,1.38f,.407f),new Vector3(.115f,.245f,.025f),shape,black);
            }
            Part(rabbit.transform,"Tail",new Vector3(0,.45f,-.345f),new Vector3(.29f,.28f,.24f),shape,belly);
            var nose=Part(rabbit.transform,"Pink nose",new Vector3(0,1.20f,.444f),new Vector3(.17f,.105f,.05f),shape,pink);
            nose.localRotation=Quaternion.Euler(0,0,180);
            Part(rabbit.transform,"Mouth stem",new Vector3(0,1.12f,.43f),new Vector3(.016f,.09f,.014f),shape,black);
            for(int side=-1;side<=1;side+=2)
            {
                var mouth=Part(rabbit.transform,"Mouth smile "+side,new Vector3(side*.052f,1.075f,.426f),new Vector3(.105f,.016f,.014f),shape,black);
                mouth.localRotation=Quaternion.Euler(0,0,side*18);
            }
            PrefabUtility.SaveAsPrefabAsset(rabbit,Folder+"/Reference Rabbit.prefab");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Marker,"Added one rabbit to "+ScenePath);
            Debug.Log("Added Reference Rabbit to Level2-shinei. Existing objects were not edited.",rabbit);
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

    static Mesh MakeFacetedMesh()
    {
        const string path=Folder+"/Faceted rabbit shape.asset";
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
        var mesh=new Mesh{name="Faceted rabbit shape"};
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }
}
#endif
