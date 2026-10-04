using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class WorldSpaceBackground : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField, Min(0.1f)] float distanceFromCamera = 500f;
    [SerializeField, Min(1f)] float overscan = 1.02f;

    public void Configure(Camera camera, float distance = 500f)
    {
        targetCamera = camera;
        distanceFromCamera = distance;
        UpdateLayout();
    }

    void OnEnable()
    {
        UpdateLayout();
    }

    void LateUpdate()
    {
        UpdateLayout();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        UpdateLayout();
    }
#endif

    void UpdateLayout()
    {
        if(!targetCamera)targetCamera = Camera.main;
        if(!targetCamera)return;

        if(transform.parent != targetCamera.transform)
            transform.SetParent(targetCamera.transform, false);

        float distance = Mathf.Clamp(
            distanceFromCamera,
            targetCamera.nearClipPlane + 0.1f,
            targetCamera.farClipPlane - 1f);

        transform.localPosition = new Vector3(0f, 0f, distance);
        transform.localRotation = Quaternion.identity;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(!spriteRenderer || !spriteRenderer.sprite)return;

        float viewHeight = targetCamera.orthographic
            ? targetCamera.orthographicSize * 2f
            : 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float viewWidth = viewHeight * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        if(spriteSize.x <= 0f || spriteSize.y <= 0f)return;

        float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y) * overscan;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
