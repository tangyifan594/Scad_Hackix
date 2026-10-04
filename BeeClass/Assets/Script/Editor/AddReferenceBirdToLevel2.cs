#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time additive creation requested for Level2-shinei only.
public static class AddReferenceBirdToLevel2
{
    const string ScenePath = "Assets/Scenes/Level2-shinei.unity";
    const string Folder = "Assets/Art/ReferenceBird";
    const string Marker = "Library/ReferenceBird-Level2.done";
    const string Name = "Reference Bird - blue low poly";

    [InitializeOnLoadMethod]
    static void Schedule()
    {
        if(!File.Exists(Marker))EditorApplication.delayCall += Install;
    }

    [MenuItem("Tools/BeeClass/Add Reference Bird To Level2")]
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
            Directory.CreateDirectory("Archive/ReferenceBird");
            const string backup = "Archive/ReferenceBird/Level2-shinei-before-bird.unity";
            if(!File.Exists(backup))File.Copy(ScenePath, backup);
            if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art", "ReferenceBird");
            Material blue = MakeMaterial("Blue grey feathers", new Color(.43f,.49f,.68f));
            Material wing = MakeMaterial("Dark blue wings", new Color(.31f,.35f,.51f));
            Material cream = MakeMaterial("Cream face and belly", new Color(.96f,.89f,.79f));
            Material yellow = MakeMaterial("Golden beak", new Color(1f,.67f,.16f));
            Material dark = MakeMaterial("Charcoal eyes and feet", new Color(.075f,.075f,.072f));
            Mesh shape = MakeFacetedMesh();
            var rabbit = new GameObject(Name);
            SceneManager.MoveGameObjectToScene(rabbit,scene);
            rabbit.transform.position = new Vector3(-1.9f,.1f,-1.6f);
            rabbit.transform.localScale = Vector3.one * .72f;
            Part(rabbit.transform,"Body",new Vector3(0,.64f,0),new Vector3(.85f,.98f,.67f),shape,blue);
            Part(rabbit.transform,"Cream belly",new Vector3(0,.59f,.285f),new Vector3(.66f,.78f,.20f),shape,cream);
            Part(rabbit.transform,"Head",new Vector3(0,1.24f,.045f),new Vector3(1.0f,.92f,.82f),shape,blue);
            Mesh face = MakeDetailMesh("Cream face patch",new[]{new Vector3(-.45f,.07f,.365f),new Vector3(-.34f,.34f,.335f),new Vector3(-.20f,.13f,.418f),new Vector3(0,.065f,.44f),new Vector3(.20f,.13f,.418f),new Vector3(.34f,.34f,.335f),new Vector3(.45f,.07f,.365f),new Vector3(.32f,-.30f,.34f),new Vector3(0,-.43f,.28f),new Vector3(-.32f,-.30f,.34f),new Vector3(0,-.15f,.45f)},new[]{10,1,0,10,2,1,10,3,2,10,4,3,10,5,4,10,6,5,10,7,6,10,8,7,10,9,8,10,0,9});
            Part(rabbit.transform,"Cream face",new Vector3(0,1.24f,.045f),Vector3.one,face,cream);
            Mesh pointed = MakeDetailMesh("Golden diamond beak",new[]{new Vector3(-.5f,0,0),new Vector3(0,.40f,0),new Vector3(.5f,0,0),new Vector3(0,-.40f,0),new Vector3(0,0,1)},new[]{0,4,1,1,4,2,2,4,3,3,4,0,0,1,2,0,2,3});
            Part(rabbit.transform,"Beak",new Vector3(0,1.19f,.49f),new Vector3(.30f,.32f,.28f),pointed,yellow);
            Mesh crest = MakeDetailMesh("Pointed crown",new[]{new Vector3(-.19f,0,-.16f),new Vector3(.19f,0,-.16f),new Vector3(.19f,0,.16f),new Vector3(-.19f,0,.16f),new Vector3(0,.25f,-.03f)},new[]{0,4,1,1,4,2,2,4,3,3,4,0,0,1,2,0,2,3});
            Part(rabbit.transform,"Crown feather",new Vector3(0,1.66f,.045f),Vector3.one,crest,blue);
            for(int side=-1;side<=1;side+=2)
            {
                Part(rabbit.transform,"Eye "+side,new Vector3(side*.245f,1.38f,.464f),new Vector3(.125f,.225f,.025f),shape,dark);
                var fin=Part(rabbit.transform,"Wing "+side,new Vector3(side*.49f,.72f,-.01f),new Vector3(.30f,.72f,.43f),shape,wing);
                fin.localRotation=Quaternion.Euler(-12,0,side*18);
                Part(rabbit.transform,"Leg "+side,new Vector3(side*.20f,.18f,0),new Vector3(.11f,.28f,.12f),shape,dark);
                Part(rabbit.transform,"Foot "+side,new Vector3(side*.20f,.055f,.095f),new Vector3(.28f,.11f,.30f),shape,dark);
            }
            var tail=Part(rabbit.transform,"Short tail",new Vector3(0,.47f,-.48f),new Vector3(.33f,.17f,.52f),shape,blue);
            tail.localRotation=Quaternion.Euler(24,0,0);
            PrefabUtility.SaveAsPrefabAsset(rabbit,Folder+"/Reference Bird.prefab");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Marker,"Added one bird to "+ScenePath);
            Debug.Log("Added Reference Bird to Level2-shinei. Existing objects were not edited.",rabbit);
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

    static Mesh MakeDetailMesh(string name,Vector3[] vertices,int[] triangles)
    {
        string path=Folder+"/"+name+".asset";
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing)return existing;
        var flatVertices=new Vector3[triangles.Length];
        var flatTriangles=new int[triangles.Length];
        for(int i=0;i<triangles.Length;i++){flatVertices[i]=vertices[triangles[i]];flatTriangles[i]=i;}
        var mesh=new Mesh{name=name};
        mesh.vertices=flatVertices;mesh.triangles=flatTriangles;
        mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }

    static Mesh MakeFacetedMesh()
    {
        const string path=Folder+"/Faceted bird shape.asset";
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
        var mesh=new Mesh{name="Faceted bird shape"};
        mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }
}
#endif
