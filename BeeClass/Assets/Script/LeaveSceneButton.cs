using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaveSceneButton : MonoBehaviour
{
    public string destinationScene = "Level2-shinei";

    public void Leave()
    {
        SceneManager.LoadScene(destinationScene);
    }
}