using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
[InitializeOnLoad]
public static class CampusApplyAll
{
    static CampusApplyAll(){}
    static void TryAutoApply()
    {
        if(File.Exists("Library/CodexCampusApplyAll.done"))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode){EditorApplication.delayCall+=TryAutoApply;return;}
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Campus.unity");
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)return;
        try{Apply();}catch(System.Exception e){File.WriteAllText("Library/CodexCampusApplyAll.error.txt",e.ToString());UnityEngine.Debug.LogException(e);}
    }
    [MenuItem("Tools/Codex/Open And Apply Bee Campus")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying){EditorApplication.isPlaying=false;EditorApplication.delayCall+=Apply;return;}
        EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity",OpenSceneMode.Single);
        CampusExteriorReference.Build();
        CampusFacetedBee.Build();
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach(var root in scene.GetRootGameObjects())
        {
            var player=root.GetComponentInChildren<CampusWalker>(true);
            if(player){player.firstPerson=false;player.topDown=false;EditorUtility.SetDirty(player);}
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Campus.unity",true)};
        File.WriteAllText("Library/CodexCampusApplyAll.done",scene.path);
    }
}
