using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public sealed class PlayableDirectorTimeEvent : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    [SerializeField, Min(0f)] double triggerTime;
    [SerializeField] UnityEvent onTimeReached = new UnityEvent();

    bool triggered;

    public UnityEvent OnTimeReached => onTimeReached;

    void Awake()
    {
        if(!director)director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        if(triggered || !director || director.state != PlayState.Playing)return;
        if(director.time + 0.0001d < triggerTime)return;

        triggered = true;
        onTimeReached.Invoke();
    }

    public void Configure(PlayableDirector targetDirector, double time)
    {
        director = targetDirector;
        triggerTime = Mathf.Max(0f, (float)time);
    }

    public void ResetTrigger()
    {
        triggered = false;
    }
}
