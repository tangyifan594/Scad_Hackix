using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class CampusElevatorFacingFix
{
    [MenuItem("Tools/Codex/Fix Elevator Facing")]
    public static void Fix()
    {
        if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before saving elevator facing.");
        var e=Object.FindFirstObjectByType<CampusElevator>();e.cabin.localRotation=Quaternion.Euler(0,90,0);
        foreach(var t in e.GetComponentsInChildren<Transform>())
        {
            float y=t.localPosition.y;
            if(t.name=="Elevator left shaft"){t.localPosition=new Vector3(-10,y,-2.85f);t.localScale=new Vector3(4,3.8f,.15f);}
            if(t.name=="Elevator right shaft"){t.localPosition=new Vector3(-10,y,.85f);t.localScale=new Vector3(4,3.8f,.15f);}
            if(t.name=="Elevator back shaft"){t.localPosition=new Vector3(-11.85f,y,-1);t.localScale=new Vector3(.15f,3.8f,3.7f);}
            if(t.name=="Elevator landing sign"){t.localPosition=new Vector3(-8.03f,y,-1);t.localScale=new Vector3(.07f,.5f,3);}
            if(t.name=="Elevator label"){t.localPosition=new Vector3(-7.98f,y,-1);t.localRotation=Quaternion.Euler(0,-90,0);}
        }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
