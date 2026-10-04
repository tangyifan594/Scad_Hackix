using UnityEngine;

// Applies after CampusWalker, and only to the camera explicitly configured in LEVEL2.
[DefaultExecutionOrder(1000)]
[RequireComponent(typeof(Camera))]
public class Level2AuthoredCamera : MonoBehaviour
{
    public CampusWalker player;
    public float focusHeight = 1.05f;
    Vector3 authoredOffset;
    Quaternion authoredRotation;
    Quaternion initialControllerRotation;
    bool initialized;

    void Awake()
    {
        if (!player || player.followCamera != GetComponent<Camera>())
        {
            Debug.LogError("LEVEL2 camera must reference its player and follow camera.", this);
            enabled = false;
            return;
        }
        authoredOffset = transform.position - (player.transform.position + Vector3.up * focusHeight);
        authoredRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (!player || !player.enabled) return;
        // Read the orbit orientation computed by the controller, without using its
        // replacement position or collision-shortened follow distance.
        Quaternion controllerRotation = transform.rotation;
        if (!initialized)
        {
            initialControllerRotation = controllerRotation;
            initialized = true;
        }
        Quaternion orbitChange = controllerRotation * Quaternion.Inverse(initialControllerRotation);
        Vector3 focus = player.transform.position + Vector3.up * focusHeight;
        transform.SetPositionAndRotation(focus + orbitChange * authoredOffset, orbitChange * authoredRotation);
    }
}
