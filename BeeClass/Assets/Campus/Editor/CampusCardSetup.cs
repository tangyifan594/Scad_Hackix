using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class CampusCardSetup
{
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 p,Vector3 scale,Material mat)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    [MenuItem("Tools/Codex/Add NPC Cards")]
    public static void Install()
    {
        if(EditorApplication.isPlaying) { Debug.LogError("Stop Play before installing NPCs.");return; }
        var player=Object.FindFirstObjectByType<CampusWalker>();if(!player)throw new System.Exception("Campus player not found.");
        var inventory=player.GetComponent<CampusInventory>();if(!inventory)inventory=player.gameObject.AddComponent<CampusInventory>();
        var old=GameObject.Find("Campus card NPCs");if(old)Object.DestroyImmediate(old);
        var root=new GameObject("Campus card NPCs").transform;
        var cards=new[]{
            new CampusCard{id="welcome",title="校园欢迎卡",npcName="迎新同学 · 小林",description="第一次相遇的纪念。欢迎来到校园！",color=new Color(.2f,.65f,.9f)},
            new CampusCard{id="library",title="阅读伙伴卡",npcName="图书管理员 · 安老师",description="一本好书，一位新朋友。带着好奇探索世界。",color=new Color(.6f,.4f,.88f)},
            new CampusCard{id="sports",title="活力运动卡",npcName="运动队员 · 阿杰",description="一起出发，保持活力。每一步都是进步！",color=new Color(.95f,.57f,.2f)}
        };
        inventory.catalog=new System.Collections.Generic.List<CampusCard>(cards);
        var positions=new[]{new Vector3(2,0,-17),new Vector3(18,0,7),new Vector3(14,0,-15)};
        var skin=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Skin.mat");var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/Campus/Charcoal.mat");
        for(int i=0;i<cards.Length;i++)
        {
            var npc=new GameObject("NPC - "+cards[i].id);npc.transform.SetParent(root);npc.transform.position=positions[i];
            string path="Assets/Campus/NPC "+cards[i].id+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.color=cards[i].color;
            Shape("Jacket",PrimitiveType.Capsule,npc.transform,new Vector3(0,1.05f,0),new Vector3(.6f,.38f,.4f),mat);
            Shape("Head",PrimitiveType.Sphere,npc.transform,new Vector3(0,1.65f,0),Vector3.one*.43f,skin);
            Shape("Hair",PrimitiveType.Sphere,npc.transform,new Vector3(0,1.82f,-.025f),new Vector3(.45f,.21f,.45f),dark);
            foreach(float x in new[]{-.15f,.15f})Shape("Leg",PrimitiveType.Capsule,npc.transform,new Vector3(x,.4f,0),new Vector3(.22f,.4f,.22f),dark);
            foreach(float x in new[]{-.36f,.36f})Shape("Arm",PrimitiveType.Capsule,npc.transform,new Vector3(x,1.05f,0),new Vector3(.18f,.30f,.18f),mat);
            var marker=Shape("Card marker",PrimitiveType.Cube,npc.transform,new Vector3(0,2.45f,0),new Vector3(.35f,.46f,.06f),mat);
            var label=new GameObject("NPC name");label.transform.SetParent(npc.transform,false);label.transform.localPosition=new Vector3(0,2.1f,0);
            var text=label.AddComponent<TextMesh>();text.text=new[]{"WELCOME\nTouch for a card","LIBRARY\nTouch for a card","SPORTS\nTouch for a card"}[i];text.fontSize=44;text.characterSize=.035f;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
            var trigger=npc.AddComponent<CapsuleCollider>();trigger.isTrigger=true;trigger.center=new Vector3(0,1,0);trigger.radius=.8f;trigger.height=2;
            var rb=npc.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
            var giver=npc.AddComponent<CampusNpcCard>();giver.card=cards[i];giver.indicator=marker.GetComponent<Renderer>();
        }
        EditorUtility.SetDirty(inventory);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("3 NPCs and card backpack installed.");
    }
}
