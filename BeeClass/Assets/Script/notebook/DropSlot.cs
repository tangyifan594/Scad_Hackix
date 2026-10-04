using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string requiredIconId;
    [SerializeField] RectTransform snapTarget;
    [SerializeField] bool lockIconAfterSnap = true;
    [SerializeField] DragPuzzleChecker puzzleChecker;
    [Header("Optional hover display")]
    [SerializeField] Image highlightImage;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color validHoverColor = new Color(0.55f, 1f, 0.55f, 1f);
    [SerializeField] Color invalidHoverColor = new Color(1f, 0.55f, 0.55f, 1f);
    [SerializeField] Color filledColor = new Color(0.75f, 1f, 0.75f, 1f);
    [SerializeField] UnityEvent onCorrectIconPlaced = new UnityEvent();
    [SerializeField] UnityEvent onWrongIconDropped = new UnityEvent();

    DraggableIcon currentIcon;

    public bool IsCorrect => currentIcon && currentIcon.Matches(requiredIconId);
    public bool IsOccupied => currentIcon;
    public string RequiredIconId => requiredIconId;

    void Awake()
    {
        if(!snapTarget)snapTarget = transform as RectTransform;
        if(!puzzleChecker)puzzleChecker = GetComponentInParent<DragPuzzleChecker>();
        SetHighlight(normalColor);
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableIcon icon = eventData.pointerDrag
            ? eventData.pointerDrag.GetComponent<DraggableIcon>()
            : null;

        if(!icon || currentIcon || !icon.Matches(requiredIconId))
        {
            onWrongIconDropped.Invoke();
            SetHighlight(currentIcon ? filledColor : normalColor);
            return;
        }

        currentIcon = icon;
        icon.SnapTo(this, snapTarget, lockIconAfterSnap);
        SetHighlight(filledColor);
        onCorrectIconPlaced.Invoke();
        if(puzzleChecker)puzzleChecker.NotifySlotChanged();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(currentIcon)return;

        DraggableIcon icon = eventData.pointerDrag
            ? eventData.pointerDrag.GetComponent<DraggableIcon>()
            : null;
        if(!icon)return;

        SetHighlight(icon.Matches(requiredIconId) ? validHoverColor : invalidHoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(currentIcon ? filledColor : normalColor);
    }

    public void Clear()
    {
        if(!currentIcon)return;
        DraggableIcon icon = currentIcon;
        currentIcon = null;
        icon.ClearSlotReference(this);
        SetHighlight(normalColor);
        if(puzzleChecker)puzzleChecker.NotifySlotChanged();
    }

    internal void Release(DraggableIcon icon)
    {
        if(currentIcon != icon)return;
        currentIcon = null;
        icon.ClearSlotReference(this);
        SetHighlight(normalColor);
        if(puzzleChecker)puzzleChecker.NotifySlotChanged();
    }

    internal void RestoreAfterFailedDrag(DraggableIcon icon)
    {
        if(currentIcon)return;
        currentIcon = icon;
        icon.SnapTo(this, snapTarget, lockIconAfterSnap);
        SetHighlight(filledColor);
        if(puzzleChecker)puzzleChecker.NotifySlotChanged();
    }

    void SetHighlight(Color color)
    {
        if(highlightImage)highlightImage.color = color;
    }
}
