using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public sealed class OnStartEvent : MonoBehaviour
{
    [Min(0f)]
    [SerializeField] float delay;
    [SerializeField] bool useUnscaledTime;
    [SerializeField] UnityEvent onStart = new UnityEvent();

    bool triggered;

    public UnityEvent Event => onStart;

    void Start()
    {
        if(delay <= 0f)
        {
            Trigger();
            return;
        }

        StartCoroutine(TriggerAfterDelay());
    }

    IEnumerator TriggerAfterDelay()
    {
        if(useUnscaledTime)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);

        Trigger();
    }

    public void Trigger()
    {
        if(triggered)return;
        triggered = true;
        onStart.Invoke();
    }
}
