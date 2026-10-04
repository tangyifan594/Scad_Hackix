using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1000)]
public class Level2ThirdPersonController : MonoBehaviour
{
    public CampusWalker playerSettings;
    public float followSmoothTime = .06f;
    CharacterController controller;
    Camera cameraTarget;
    Transform visual;
    Vector3 spawn, initialOffset, followVelocity;
    float cameraDistance, yaw, pitch, verticalSpeed;
    bool captured, movingCamera;

    void Awake()
    {
        if (!playerSettings || !playerSettings.followCamera)
        {
            Debug.LogError("LEVEL2 player and camera references are required.", this);
            enabled = false;
            return;
        }
        controller = GetComponent<CharacterController>();
        cameraTarget = playerSettings.followCamera;
        visual = playerSettings.visual;
        // Only this scene stops the legacy controller and the old camera override.
        playerSettings.enabled = false;
        var oldCamera = cameraTarget.GetComponent<Level2AuthoredCamera>();
        if (oldCamera) oldCamera.enabled = false;
        spawn = transform.position;
        initialOffset = cameraTarget.transform.position - transform.position;
        Vector3 orbitOffset = initialOffset - Vector3.up * 1.05f;
        cameraDistance = orbitOffset.magnitude;
        yaw = Mathf.Atan2(-orbitOffset.x, -orbitOffset.z) * Mathf.Rad2Deg;
        pitch = Mathf.Atan2(orbitOffset.y, new Vector2(orbitOffset.x, orbitOffset.z).magnitude) * Mathf.Rad2Deg;
        if (visual)
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true)) renderer.forceRenderingOff = false;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null) return;
        var inventory = GetComponent<CampusInventory>();
        if ((inventory && inventory.BackpackOpen) || CampusElevator.MenuOpen || CampusElevator.Travelling)
        {
            ReleaseCursor();
            return;
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) captured = true;
        if (keyboard.escapeKey.wasPressedThisFrame) captured = false;
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
        if (captured && mouse != null)
        {
            Vector2 delta = mouse.delta.ReadValue();
            yaw += delta.x * .10f;
            pitch = Mathf.Clamp(pitch - delta.y * .09f, -10, 65);
            if (delta.sqrMagnitude > .001f) movingCamera = true;
        }
        var input = new Vector2((keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0),
            (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
        input = Vector2.ClampMagnitude(input, 1);
        Vector3 direction = Quaternion.Euler(0, yaw, 0) * new Vector3(input.x, 0, input.y);
        if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
        if (controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame) verticalSpeed = 6;
        verticalSpeed -= 18 * Time.deltaTime;
        controller.Move((direction * (keyboard.leftShiftKey.isPressed ? 5.4f : 2.8f) + Vector3.up * verticalSpeed) * Time.deltaTime);
        if (direction.sqrMagnitude > .01f)
        {
            movingCamera = true;
            if (visual) visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 12);
        }
        if (transform.position.y < -10)
        {
            controller.enabled = false;
            transform.position = spawn;
            controller.enabled = true;
            verticalSpeed = 0;
            followVelocity = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (!cameraTarget) return;
        Vector3 focusOffset = Vector3.up * 1.05f;
        Vector3 focus = transform.position + focusOffset;
        Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * Vector3.back * cameraDistance;
        Vector3 desired = focus + offset;
        if (!movingCamera)
        {
            // Keep the authored opening position, but frame the bee at the center.
            cameraTarget.transform.position = transform.position + initialOffset;
            cameraTarget.transform.LookAt(focus);
            return;
        }
        float clearance = Clearance(focus, offset);
        Vector3 position = Vector3.SmoothDamp(cameraTarget.transform.position, desired, ref followVelocity, followSmoothTime);
        Vector3 smoothedOffset = position - focus;
        float smoothedClearance = Clearance(focus, smoothedOffset);
        if (clearance < offset.magnitude - .01f || smoothedClearance < smoothedOffset.magnitude - .01f)
        {
            // Snap inward for an obstruction; never smooth through a wall or orbit sideways.
            position = focus + offset.normalized * clearance;
            followVelocity = Vector3.zero;
        }
        cameraTarget.transform.position = position;
        cameraTarget.transform.LookAt(focus);
    }

    float Clearance(Vector3 focus, Vector3 offset)
    {
        float distance = offset.magnitude;
        if (distance < .001f) return distance;
        foreach (var hit in Physics.SphereCastAll(focus, .18f, offset / distance, distance, ~0, QueryTriggerInteraction.Ignore))
            if (!hit.transform.IsChildOf(transform)) distance = Mathf.Min(distance, Mathf.Max(.1f, hit.distance - .12f));
        return distance;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var door = hit.collider.GetComponentInParent<CampusDoorTransition>();
        if (!door) return;
        door.Enter(playerSettings);
    }

    void ReleaseCursor()
    {
        captured = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable() { ReleaseCursor(); }

    void OnGUI()
    {
        if (!playerSettings || playerSettings.useCanvasUI) return;
        var style = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.UpperLeft, padding = new RectOffset(16, 16, 12, 12) };
        GUI.Box(new Rect(18, 18, 440, 88), "CAMPUS BEE - THIRD PERSON\nWASD Move   Shift Run   Space Jump\nClick: Look   E: Elevator   B: Backpack   Esc: Cursor", style);
        if (captured) GUI.Box(new Rect(Screen.width / 2f - 2, Screen.height / 2f - 2, 4, 4), "");
    }
}
