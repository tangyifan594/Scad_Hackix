using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public sealed class DragPuzzleChecker : MonoBehaviour
{
    [SerializeField] List<DropSlot> slots = new List<DropSlot>();
    [SerializeField] bool autoFindChildSlots = true;
    [SerializeField] bool triggerOnlyOnce = true;
    [SerializeField] UnityEvent onPuzzleCompleted = new UnityEvent();

    bool completed;

    public bool IsCompleted => completed;
    public UnityEvent OnPuzzleCompleted => onPuzzleCompleted;

    void Awake()
    {
        if(autoFindChildSlots)RefreshSlots();
    }

    public void RefreshSlots()
    {
        slots.Clear();
        slots.AddRange(GetComponentsInChildren<DropSlot>(true));
    }

    public void NotifySlotChanged()
    {
        bool allCorrect = slots.Count > 0;
        foreach(DropSlot slot in slots)
        {
            if(!slot || !slot.IsCorrect)
            {
                allCorrect = false;
                break;
            }
        }

        if(!allCorrect)
        {
            if(!triggerOnlyOnce)completed = false;
            return;
        }

        if(completed && triggerOnlyOnce)return;
        completed = true;
        onPuzzleCompleted.Invoke();
    }

    public void ResetCompletionState()
    {
        completed = false;
    }
}
