using UnityEngine;
using UnityEngine.SceneManagement;

public class CampusDoorTransition : MonoBehaviour
{
    [SerializeField] string destinationScene = "Assets/Scenes/Level0.5-corridor.unity";
    bool loading;

    public void Enter(CampusWalker player)
    {
        if (!player || loading || player.gameObject.scene != gameObject.scene) return;
        if (!Application.CanStreamedLevelBeLoaded(destinationScene))
        {
            Debug.LogError("Door destination is missing from Build Settings: " + destinationScene, this);
            return;
        }

        loading = true;
        player.enabled = false;
        SceneManager.LoadSceneAsync(destinationScene, LoadSceneMode.Single);
    }

    void OnTriggerEnter(Collider other)
    {
        Enter(other.GetComponentInParent<CampusWalker>());
    }
}
