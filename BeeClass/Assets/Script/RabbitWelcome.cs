using System.Collections.Generic;
using UnityEngine;

// Proximity greeting for the reference animals in Level2-shinei.
public class RabbitWelcome : MonoBehaviour
{
    [SerializeField, Min(.1f)] float welcomeRadius = 1.9f;
    [SerializeField] Vector3 triggerCenter = new Vector3(0, 1, 0);
    [SerializeField] Vector3 smileCenter = new Vector3(0, 1.055f, .47f);
    [SerializeField] bool showSmileMouth = true;
    readonly HashSet<Collider> visitors = new HashSet<Collider>();
    readonly List<Pose> parts = new List<Pose>();
    float blend;
    float time;
    LineRenderer smile;
    readonly List<LineRenderer> crescentEyes = new List<LineRenderer>();
    Material smileMaterial;
    class Pose
    {
        public Transform part;
        public Vector3 position, scale;
        public Quaternion rotation;
        public int side;
        public string kind;
    }
    void Awake()
    {
        // Birds express happiness through their eyes; keep the original beak.
        // Also handles an already-open scene that still has the old serialized setting.
        if (transform.Find("Beak")) showSmileMouth = false;
        var trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.center = triggerCenter;
        trigger.radius = welcomeRadius;
        var body = GetComponent<Rigidbody>();
        if (!body) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        foreach (Transform part in transform)
        {
            string kind = part.name;
            if (!(kind.StartsWith("Arm ") || kind.StartsWith("Wing ") || kind.StartsWith("Paw ") || kind.StartsWith("Eye ") || kind.StartsWith("Mouth smile ") || kind == "Mouth stem" || kind == "Mouth line")) continue;
            parts.Add(new Pose { part = part, position = part.localPosition, rotation = part.localRotation,
                scale = part.localScale, side = kind.EndsWith("-1") ? -1 : 1, kind = kind });
        }
        // A separate curved mouth is much clearer than rotating the tiny original segments.
        var mouth = new GameObject("Welcome smile");
        mouth.transform.SetParent(transform, false);
        smile = mouth.AddComponent<LineRenderer>();
        smile.useWorldSpace = false;
        smile.positionCount = 17;
        smile.numCapVertices = 4;
        smile.numCornerVertices = 3;
        smile.widthMultiplier = .018f;
        smileMaterial = new Material(Shader.Find("Sprites/Default"));
        smile.sharedMaterial = smileMaterial;
        smile.startColor = smile.endColor = new Color(.035f, .02f, .018f);
        smile.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        smile.receiveShadows = false;
        for (int i = 0; i < 17; i++)
        {
            float x = Mathf.Lerp(-.12f, .12f, i / 16f);
            smile.SetPosition(i, smileCenter + new Vector3(x, .03f * (x * x / (.12f * .12f)), 0));
        }
        smile.enabled = false;
        foreach (var pose in parts)
        {
            if (!pose.kind.StartsWith("Eye ")) continue;
            var eyeObject = new GameObject("Welcome crescent " + pose.side);
            eyeObject.transform.SetParent(transform, false);
            var eye = eyeObject.AddComponent<LineRenderer>();
            eye.useWorldSpace = false;
            eye.positionCount = 17;
            eye.sharedMaterial = smileMaterial;
            eye.startColor = eye.endColor = smile.startColor;
            eye.numCapVertices = 4;
            eye.numCornerVertices = 3;
            // Tapered ends and a thicker middle create crescent-shaped closed smile eyes.
            eye.widthCurve = new AnimationCurve(new Keyframe(0, .12f), new Keyframe(.5f, 1), new Keyframe(1, .12f));
            eye.widthMultiplier = .026f;
            eye.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            eye.receiveShadows = false;
            for (int i = 0; i < 17; i++)
            {
                float x = Mathf.Lerp(-.085f, .085f, i / 16f);
                eye.SetPosition(i, pose.position + new Vector3(x, .04f - .065f * (x * x / (.085f * .085f)), .04f));
            }
            eye.enabled = false;
            crescentEyes.Add(eye);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        var bee = other.GetComponentInParent<CampusWalker>();
        if (bee && bee.gameObject.scene == gameObject.scene && visitors.Add(other) && visitors.Count == 1) time = 0;
    }
    void OnTriggerExit(Collider other) { visitors.Remove(other); }
    void LateUpdate()
    {
        visitors.RemoveWhere(c => !c || !c.gameObject.activeInHierarchy);
        blend = Mathf.MoveTowards(blend, visitors.Count > 0 ? 1 : 0, Time.deltaTime * 4);
        if (blend > 0) time += Time.deltaTime;
        smile.enabled = showSmileMouth && blend > .01f;
        smile.widthMultiplier = .018f * blend;
        foreach (var eye in crescentEyes)
        {
            eye.enabled = blend > .01f;
            eye.widthMultiplier = .026f * blend;
        }
        foreach (var pose in parts)
        {
            if (!pose.part) continue;
            Vector3 position = pose.position, scale = pose.scale;
            Quaternion rotation = pose.rotation;
            if (pose.kind.StartsWith("Arm ") || pose.kind.StartsWith("Wing ") || pose.kind.StartsWith("Paw "))
            {
                // Fix the upper end at the shoulder and rotate the arm outward around it.
                // Only the right paw waves; the other arm stays in its resting pose.
                if (pose.side == 1)
                {
                    var arm = pose.kind.StartsWith("Paw ") ? parts.Find(p => p.kind == "Arm 1") : pose;
                    if (arm == null) continue;
                    Vector3 shoulder = arm.position + arm.rotation * Vector3.up * (arm.scale.y * .42f);
                    Quaternion turn = Quaternion.Euler(0, 0, (110 + 12 * Mathf.Sin(time * 5)) * blend);
                    pose.part.localRotation = turn * pose.rotation;
                    pose.part.localPosition = shoulder + turn * (pose.position - shoulder);
                    pose.part.localScale = pose.scale;
                    continue;
                }
            }
            else if (pose.kind.StartsWith("Eye "))
            {
                scale = Vector3.zero;
            }
            else if (pose.kind.StartsWith("Mouth smile ") || pose.kind == "Mouth stem" || pose.kind == "Mouth line")
            {
                scale = Vector3.zero;
            }
            pose.part.localPosition = Vector3.Lerp(pose.position, position, blend);
            pose.part.localScale = Vector3.Lerp(pose.scale, scale, blend);
            pose.part.localRotation = Quaternion.Slerp(pose.rotation, rotation, blend);
        }
    }
    void OnDisable()
    {
        visitors.Clear(); blend = 0;
        if (smile) smile.enabled = false;
        foreach (var eye in crescentEyes) if (eye) eye.enabled = false;
        foreach (var pose in parts)
            if (pose.part) { pose.part.localPosition = pose.position; pose.part.localRotation = pose.rotation; pose.part.localScale = pose.scale; }
    }
    void OnDestroy() { if (smileMaterial) Destroy(smileMaterial); }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(.3f, 1, .5f, .6f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(triggerCenter, welcomeRadius);
    }
}
