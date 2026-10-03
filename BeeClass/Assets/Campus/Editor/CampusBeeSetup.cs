using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class CampusBeeSetup
{
    static Material Material(string name,Color color)
    {
        string path="Assets/Campus/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.color=color;mat.SetFloat("_Smoothness",.38f);return mat;
    }
    static GameObject Part(string name,Transform parent,Vector3 position,Vector3 size,Material mat,PrimitiveType type=PrimitiveType.Sphere)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    [MenuItem("Tools/Codex/Replace Player With Bee")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before replacing model.");
        var walker=Object.FindFirstObjectByType<CampusWalker>();if(!walker)throw new System.Exception("Campus player missing.");
        var old=walker.transform.Find("Bee visual");if(old)Object.DestroyImmediate(old.gameObject);
        if(walker.visual)walker.visual.gameObject.SetActive(false);
        var yellow=Material("Bee honey yellow",new Color(1f,.68f,.055f));var black=Material("Bee charcoal",new Color(.045f,.035f,.025f));
        var wing=Material("Bee pearl wings",new Color(.72f,.92f,1f));var white=Material("Bee eye glint",Color.white);var blush=Material("Bee cheeks",new Color(1,.34f,.22f));
        var root=new GameObject("Bee visual").transform;root.SetParent(walker.transform,false);
        Part("Golden abdomen",root,new Vector3(0,1.05f,-.23f),new Vector3(.82f,.68f,1.10f),yellow);
        foreach(float z in new[]{-.53f,-.25f,.03f})
        {
            float ratio=(z+.23f)/.55f;float section=Mathf.Sqrt(1-ratio*ratio);
            Part("Black stripe",root,new Vector3(0,1.05f,z),new Vector3(.84f*section,.70f*section,.15f),black);
        }
        Part("Head",root,new Vector3(0,1.36f,.39f),new Vector3(.68f,.62f,.61f),yellow);
        foreach(float x in new[]{-.19f,.19f})
        {
            Part("Friendly eye",root,new Vector3(x,1.42f,.66f),new Vector3(.17f,.23f,.075f),black);
            Part("Eye sparkle",root,new Vector3(x-.025f,1.47f,.697f),new Vector3(.047f,.065f,.025f),white);
            Part("Cheek",root,new Vector3(x*1.3f,1.27f,.637f),new Vector3(.09f,.06f,.022f),blush);
            var antenna=Part("Antenna",root,new Vector3(x,1.80f,.34f),new Vector3(.045f,.18f,.045f),black,PrimitiveType.Capsule);antenna.transform.localRotation=Quaternion.Euler(15,0,-Mathf.Sign(x)*23);
            Part("Antenna tip",root,new Vector3(x*1.55f,1.96f,.39f),Vector3.one*.10f,black);
        }
        Part("Smile",root,new Vector3(0,1.24f,.679f),new Vector3(.10f,.022f,.02f),black);
        for(int i=0;i<3;i++)foreach(float side in new[]{-1f,1f})
        {
            var leg=Part("Bee leg",root,new Vector3(side*.36f,.72f,-.50f+i*.26f),new Vector3(.065f,.18f,.065f),black,PrimitiveType.Capsule);leg.transform.localRotation=Quaternion.Euler(0,0,side*32);
            Part("Foot",root,new Vector3(side*.45f,.57f,-.50f+i*.26f),new Vector3(.13f,.08f,.12f),black);
        }
        var l=new GameObject("Left wing hinge").transform;l.SetParent(root,false);l.localPosition=new Vector3(-.2f,1.32f,-.1f);
        var r=new GameObject("Right wing hinge").transform;r.SetParent(root,false);r.localPosition=new Vector3(.2f,1.32f,-.1f);
        Part("Left large wing",l,new Vector3(-.38f,0,-.12f),new Vector3(.88f,.065f,.50f),wing);
        Part("Right large wing",r,new Vector3(.38f,0,-.12f),new Vector3(.88f,.065f,.50f),wing);
        Part("Left small wing",l,new Vector3(-.24f,-.015f,-.40f),new Vector3(.56f,.055f,.35f),wing);
        Part("Right small wing",r,new Vector3(.24f,-.015f,-.40f),new Vector3(.56f,.055f,.35f),wing);
        l.localRotation=Quaternion.Euler(-12,0,-18);r.localRotation=Quaternion.Euler(-12,0,18);
        var motion=root.gameObject.AddComponent<CampusBeeMotion>();motion.leftWing=l;motion.rightWing=r;
        walker.visual=root;walker.leftLeg=null;walker.rightLeg=null;walker.leftArm=null;walker.rightArm=null;walker.gameObject.name="Player - Bee";
        System.IO.Directory.CreateDirectory("Assets/Campus/Prefabs");PrefabUtility.SaveAsPrefabAsset(root.gameObject,"Assets/Campus/Prefabs/BeePlayerVisual.prefab");
        EditorUtility.SetDirty(walker);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Selection.activeGameObject=walker.gameObject;
    }
}
