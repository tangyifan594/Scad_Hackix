using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public sealed class DelayedEventTrigger : MonoBehaviour
{
    [SerializeField, Min(0f)] float delay = 1f;
    [SerializeField] bool useUnscaledTime;
    [SerializeField] bool restartDelayWhenTriggeredAgain = true;
    [SerializeField] UnityEvent onDelayFinished = new UnityEvent();

    Coroutine delayCoroutine;

    public bool IsWaiting => delayCoroutine != null;
    public UnityEvent OnDelayFinished => onDelayFinished;

    public void Trigger()
    {
        StartDelay(delay);
    }

    public void TriggerWithDelay(float seconds)
    {
        StartDelay(Mathf.Max(0f, seconds));
    }

    public void Cancel()
    {
        if(delayCoroutine == null)return;

        StopCoroutine(delayCoroutine);
        delayCoroutine = null;
    }

    void OnDisable()
    {
        Cancel();
    }

    void StartDelay(float seconds)
    {
        if(delayCoroutine != null)
        {
            if(!restartDelayWhenTriggeredAgain)return;
            StopCoroutine(delayCoroutine);
        }

        delayCoroutine = StartCoroutine(WaitAndInvoke(seconds));
    }

    IEnumerator WaitAndInvoke(float seconds)
    {
        if(seconds > 0f)
        {
            if(useUnscaledTime)
                yield return new WaitForSecondsRealtime(seconds);
            else
                yield return new WaitForSeconds(seconds);
        }

        delayCoroutine = null;
        onDelayFinished.Invoke();
    }
}
