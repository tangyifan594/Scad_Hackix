using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class CampusArtPolish
{
    static Mesh rounded,sphere;static Material wood,metal,seat,woodFloor,plant,pot;
    static Material Mat(string name,Color color,float smooth=.25f)
    {
        string path="Assets/Campus/Art/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",smooth);return m;
    }
    static Mesh RoundMesh()
    {
        string path="Assets/Campus/Art/RoundedCube.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old)return old;
        var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();int n=10;
        foreach(var normal in new[]{Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back})
        {
            var u=Vector3.Cross(Mathf.Abs(normal.y)<.9f?Vector3.up:Vector3.forward,normal).normalized;var v=Vector3.Cross(normal,u);int offset=vertices.Count;
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                var p=normal*.5f+u*((float)x/n-.5f)+v*((float)y/n-.5f);var q=new Vector3(Mathf.Clamp(p.x,-.4f,.4f),Mathf.Clamp(p.y,-.4f,.4f),Mathf.Clamp(p.z,-.4f,.4f));var norm=(p-q).normalized;vertices.Add(q+norm*.1f);normals.Add(norm);uv.Add(new Vector2((float)x/n,(float)y/n));
            }
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){int a=offset+y*(n+1)+x;triangles.AddRange(new[]{a,a+1,a+n+2,a,a+n+2,a+n+1});}
        }
        var mesh=new Mesh{name="Rounded furniture mesh"};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static Mesh SphereMesh()
    {
        string path="Assets/Campus/Art/SmoothSphere.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old)return old;
        var verts=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();int lat=32,lon=64;
        for(int y=0;y<=lat;y++)for(int x=0;x<=lon;x++){float a=Mathf.PI*y/lat,b=Mathf.PI*2*x/lon;var n=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));verts.Add(n*.5f);normals.Add(n);uv.Add(new Vector2((float)x/lon,(float)y/lat));}
        for(int y=0;y<lat;y++)for(int x=0;x<lon;x++){int i=y*(lon+1)+x;tris.AddRange(new[]{i,i+1,i+lon+1,i+1,i+lon+2,i+lon+1});}
        var m=new Mesh{name="Smooth bee mesh"};m.SetVertices(verts);m.SetNormals(normals);m.SetUVs(0,uv);m.SetTriangles(tris,0);m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);return m;
    }
    static GameObject Box(string name,Transform parent,Vector3 position,Vector3 scale,Material mat,bool solid=false)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=rounded;g.AddComponent<MeshRenderer>().sharedMaterial=mat;if(solid)g.AddComponent<BoxCollider>();return g;
    }
    static GameObject Ball(string name,Transform parent,Vector3 p,Vector3 scale,Material mat)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=sphere;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;
    }
    static void Line(string name,Transform parent,Vector3[] positions,float width,Material mat)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);var line=g.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=positions.Length;line.SetPositions(positions);line.startWidth=width;line.endWidth=width*.55f;line.numCapVertices=6;line.numCornerVertices=6;line.sharedMaterial=mat;line.shadowCastingMode=ShadowCastingMode.Off;
    }
    static Texture2D Texture(string name,bool timber)
    {
        string path="Assets/Campus/Art/"+name+".png";var texture=new Texture2D(256,256);var random=new System.Random(7);
        for(int y=0;y<256;y++)for(int x=0;x<256;x++)
        {
            Color c;
            if(timber){float grain=Mathf.Sin(y*.15f+Mathf.Sin(x*.08f)*2)*.012f+(float)random.NextDouble()*.008f;float board=(x/64%2)*.008f;c=new Color(.75f+grain+board,.56f+grain+board,.36f+grain);if(x%64<2)c*=.94f;}
            else {float noise=(float)random.NextDouble()*.018f;c=new Color(.78f+noise,.80f+noise,.78f+noise);if(x%64<2||y%64<2)c=new Color(.64f,.68f,.68f);}
            texture.SetPixel(x,y,c);
        }
        texture.Apply();System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Desk(Transform parent,float x,float y,float z,bool teacher=false)
    {
        float w=teacher?2.1f:1.25f,d=teacher?.85f:.65f;
        Box("Rounded oak desktop",parent,new Vector3(x,y+.78f,z),new Vector3(w,.12f,d),wood,true);
        foreach(float dx in new[]{-w*.38f,w*.38f})foreach(float dz in new[]{-d*.32f,d*.32f})Box("Desk metal leg",parent,new Vector3(x+dx,y+.35f,z+dz),new Vector3(.045f,.70f,.045f),metal);
        Box("Under desk shelf",parent,new Vector3(x,y+.56f,z),new Vector3(w*.82f,.045f,d*.75f),metal);
        if(teacher)return;
        float cz=z-.74f;Box("Chair seat",parent,new Vector3(x,y+.46f,cz),new Vector3(.52f,.10f,.50f),seat,true);Box("Chair backrest",parent,new Vector3(x,y+.77f,cz-.23f),new Vector3(.52f,.52f,.08f),seat,true);
        foreach(float dx in new[]{-.19f,.19f})foreach(float dz in new[]{-.18f,.18f})Box("Chair metal leg",parent,new Vector3(x+dx,y+.23f,cz+dz),new Vector3(.04f,.46f,.04f),metal);
        Box("Notebook",parent,new Vector3(x-.18f,y+.856f,z),new Vector3(.26f,.025f,.32f),woodFloor);Box("Book cover",parent,new Vector3(x-.18f,y+.872f,z),new Vector3(.27f,.01f,.33f),seat);
    }
    [MenuItem("Tools/Codex/Polish Campus Art")]
    public static void Polish()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop play before polishing scene.");
        System.IO.Directory.CreateDirectory("Assets/Campus/Art");rounded=RoundMesh();sphere=SphereMesh();
        wood=Mat("Natural oak",new Color(.88f,.72f,.51f));wood.SetTexture("_BaseMap",Texture("Oak grain",true));wood.SetTextureScale("_BaseMap",new Vector2(1,1));metal=Mat("Warm graphite",new Color(.17f,.22f,.25f),.3f);seat=Mat("Muted teal",new Color(.21f,.47f,.51f),.18f);woodFloor=Mat("Classroom timber",Color.white,.12f);woodFloor.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Campus/Art/Oak grain.png"));woodFloor.SetTextureScale("_BaseMap",new Vector2(5,7));plant=Mat("Sage foliage",new Color(.25f,.46f,.32f));pot=Mat("Terracotta",new Color(.65f,.39f,.28f));
        var school=Object.FindFirstObjectByType<CampusElevator>().transform;
        foreach(var t in school.GetComponentsInChildren<Transform>())
        {
            if(!t || !t.name.StartsWith("Classroom ") || t.name.Length!=13)continue;
            int number=int.Parse(t.name.Substring(10));float x=-8+((number%100)-1)*8,y=(number/100-1)*4;
            var previous=t.Find("Polished furniture");if(previous)Object.DestroyImmediate(previous.gameObject);
            var remove=new List<GameObject>();foreach(Transform child in t)if(child.name=="Desk top"||child.name=="Desk legs"||child.name=="Chair"||child.name=="Teacher desk")remove.Add(child.gameObject);foreach(var g in remove)Object.DestroyImmediate(g);
            var group=new GameObject("Polished furniture").transform;group.SetParent(t,false);
            var timber=Box("Timber floor",group,new Vector3(x,y+.014f,8),new Vector3(7.75f,.015f,11.7f),woodFloor);
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);timber.GetComponent<MeshFilter>().sharedMesh=cube.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(cube);
            for(int row=0;row<3;row++)foreach(float dx in new[]{-1.65f,1.65f})Desk(group,x+dx,y,4.6f+row*2);
            Desk(group,x,y,11.6f,true);
            Box("Planter",group,new Vector3(x+3.15f,y+.3f,12.8f),new Vector3(.5f,.6f,.5f),pot);
            for(int i=0;i<5;i++)Ball("Plant leaf",group,new Vector3(x+3.15f+Mathf.Sin(i*2)*.2f,y+.8f+i*.11f,12.8f+Mathf.Cos(i*2)*.15f),new Vector3(.32f,.62f,.23f),plant);
            var label=new GameObject("Floor room number").transform;label.SetParent(group,false);label.localPosition=new Vector3(x,y+.04f,3.1f);label.localRotation=Quaternion.Euler(90,0,0);var text=label.gameObject.AddComponent<TextMesh>();text.text=number.ToString();text.fontSize=100;text.characterSize=.045f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.18f,.28f,.32f);
        }
        var tile=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/School floor.mat");tile.color=Color.white;tile.SetTexture("_BaseMap",Texture("Soft terrazzo tiles",false));tile.SetTextureScale("_BaseMap",new Vector2(10,9));tile.SetFloat("_Smoothness",.12f);
        var wall=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/School warm walls.mat");wall.color=new Color(.88f,.87f,.79f);wall.SetFloat("_Smoothness",.06f);
        PolishBee();
        var sun=Object.FindFirstObjectByType<Light>();foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.type==LightType.Directional){sun=l;break;}
        sun.intensity=1.2f;sun.color=new Color(1,.95f,.86f);sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.6f;sun.shadowBias=.08f;
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.70f,.74f);RenderSettings.fog=false;
        var volumeObject=GameObject.Find("Campus art grading");if(!volumeObject)volumeObject=new GameObject("Campus art grading");var volume=volumeObject.GetComponent<Volume>();if(!volume)volume=volumeObject.AddComponent<Volume>();volume.isGlobal=true;
        string profilePath="Assets/Campus/Art/Soft campus grading.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);}
        if(!profile.TryGet<ColorAdjustments>(out var grading))grading=profile.Add<ColorAdjustments>();grading.saturation.Override(-8);grading.contrast.Override(5);grading.postExposure.Override(-.15f);
        if(!profile.TryGet<Tonemapping>(out var tonemap))tonemap=profile.Add<Tonemapping>();tonemap.mode.Override(TonemappingMode.Neutral);volume.sharedProfile=profile;Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        EditorUtility.SetDirty(profile);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
    }
    static void PolishBee()
    {
        var root=Object.FindFirstObjectByType<CampusWalker>().visual;
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh && filter.sharedMesh.name=="Sphere")filter.sharedMesh=sphere;
        var yellow=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Bee honey yellow.mat");yellow.color=new Color(1,.72f,.13f);yellow.SetFloat("_Smoothness",.28f);
        var black=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Bee charcoal.mat");black.color=new Color(.11f,.085f,.065f);black.SetFloat("_Smoothness",.35f);
        var wing=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Bee pearl wings.mat");wing.color=new Color(.78f,.91f,.92f);wing.SetFloat("_Smoothness",.2f);
        var abdomen=root.Find("Golden abdomen");
        if(abdomen)
        {
            string meshPath="Assets/Campus/Art/BeeStripedBody.asset";
            var bodyMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(!bodyMesh){bodyMesh=Object.Instantiate(sphere);bodyMesh.name="Continuous striped bee abdomen";var points=bodyMesh.vertices;var bodyUV=bodyMesh.uv;for(int i=0;i<points.Length;i++)bodyUV[i].y=points[i].z+.5f;bodyMesh.uv=bodyUV;AssetDatabase.CreateAsset(bodyMesh,meshPath);}
            abdomen.GetComponent<MeshFilter>().sharedMesh=bodyMesh;
            var texture=new Texture2D(64,256);for(int y=0;y<256;y++)for(int x=0;x<64;x++){float v=(float)y/255;bool stripe=(v>.18f && v<.27f)||(v>.43f && v<.53f)||(v>.70f && v<.79f);texture.SetPixel(x,y,stripe?new Color(.12f,.09f,.06f):new Color(1,.72f,.14f));}
            texture.Apply();string texPath="Assets/Campus/Art/Honey stripes.png";System.IO.File.WriteAllBytes(texPath,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texPath);
            var striped=Mat("Honey striped body",Color.white,.28f);striped.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));abdomen.GetComponent<Renderer>().sharedMaterial=striped;
            foreach(Transform child in root)if(child.name=="Black stripe")child.gameObject.SetActive(false);
        }
        var vein=Mat("Wing delicate veins",new Color(.48f,.68f,.70f),.1f);
        foreach(var t in root.GetComponentsInChildren<Transform>())
        {
            if(!t)continue;
            if(t.name.Contains("large wing") || t.name.Contains("small wing"))
            {
                for(int i=t.childCount-1;i>=0;i--)if(t.GetChild(i).name.StartsWith("Wing vein"))Object.DestroyImmediate(t.GetChild(i).gameObject);
                Line("Wing vein spine",t,new[]{new Vector3(-.35f,.51f,0),new Vector3(0,.51f,0),new Vector3(.35f,.51f,0)},.007f,vein);
                foreach(float side in new[]{-1f,1f})Line("Wing vein branching",t,new[]{new Vector3(-.12f,.51f,0),new Vector3(.05f,.43f,side*.25f),new Vector3(.21f,.25f,side*.34f)},.006f,vein);
            }
        }
        var eyes=Mat("Glossy espresso eyes",new Color(.025f,.04f,.05f),.82f);foreach(var r in root.GetComponentsInChildren<Renderer>())if(r.name=="Friendly eye")r.sharedMaterial=eyes;
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Campus/Prefabs/BeePlayerVisual.prefab");
    }
}




