using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public sealed class DraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] string iconId;
    [SerializeField] RectTransform dragLayer;

    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    Canvas canvas;

    Transform homeParent;
    int homeSiblingIndex;
    Vector2 homePosition;
    Quaternion homeRotation;
    Vector3 homeScale;

    Transform returnParent;
    int returnSiblingIndex;
    Vector2 returnPosition;
    Quaternion returnRotation;
    Vector3 returnScale;

    bool dropAccepted;
    bool locked;
    DropSlot returnSlot;

    public string IconId => iconId;
    public DropSlot CurrentSlot { get; private set; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        homeParent = transform.parent;
        homeSiblingIndex = transform.GetSiblingIndex();
        homePosition = rectTransform.anchoredPosition;
        homeRotation = rectTransform.localRotation;
        homeScale = rectTransform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(locked)return;

        returnParent = transform.parent;
        returnSiblingIndex = transform.GetSiblingIndex();
        returnPosition = rectTransform.anchoredPosition;
        returnRotation = rectTransform.localRotation;
        returnScale = rectTransform.localScale;
        dropAccepted = false;

        returnSlot = CurrentSlot;
        if(CurrentSlot)CurrentSlot.Release(this);

        RectTransform targetLayer = dragLayer;
        if(!targetLayer && canvas)targetLayer = canvas.transform as RectTransform;
        if(targetLayer)transform.SetParent(targetLayer, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(locked || !canvas)return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(locked)return;

        canvasGroup.blocksRaycasts = true;
        if(!dropAccepted)ReturnToDragOrigin();
    }

    public bool Matches(string requiredId)
    {
        return string.Equals(iconId.Trim(), requiredId.Trim(), System.StringComparison.OrdinalIgnoreCase);
    }

    public void ResetToHome()
    {
        if(CurrentSlot)CurrentSlot.Release(this);
        locked = false;
        dropAccepted = false;
        canvasGroup.blocksRaycasts = true;
        RestoreTransform(homeParent, homeSiblingIndex, homePosition, homeRotation, homeScale);
    }

    internal void SnapTo(DropSlot slot, RectTransform snapTarget, bool lockAfterSnap)
    {
        CurrentSlot = slot;
        returnSlot = null;
        dropAccepted = true;
        locked = lockAfterSnap;
        transform.SetParent(snapTarget, false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        canvasGroup.blocksRaycasts = true;
    }

    internal void ClearSlotReference(DropSlot slot)
    {
        if(CurrentSlot == slot)CurrentSlot = null;
    }

    void ReturnToDragOrigin()
    {
        if(returnSlot)
        {
            DropSlot previousSlot = returnSlot;
            returnSlot = null;
            previousSlot.RestoreAfterFailedDrag(this);
            return;
        }

        RestoreTransform(returnParent, returnSiblingIndex, returnPosition, returnRotation, returnScale);
    }

    void RestoreTransform(Transform parent, int siblingIndex, Vector2 position, Quaternion rotation, Vector3 scale)
    {
        transform.SetParent(parent, false);
        transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, transform.parent.childCount - 1));
        rectTransform.anchoredPosition = position;
        rectTransform.localRotation = rotation;
        rectTransform.localScale = scale;
    }
}
