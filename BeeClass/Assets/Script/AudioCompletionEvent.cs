using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public sealed class AudioCompletionEvent : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] UnityEvent onAudioFinished = new UnityEvent();
    [SerializeField, Range(0.01f, 0.25f)] float completionTolerance = 0.05f;

    bool wasPlaying;
    bool waitingForCompletion;
    AudioClip observedClip;
    int lastTimeSamples;

    public UnityEvent OnAudioFinished => onAudioFinished;

    void Awake()
    {
        if(!audioSource)audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        if(!audioSource)audioSource = GetComponent<AudioSource>();

        wasPlaying = audioSource && audioSource.isPlaying;
        waitingForCompletion = wasPlaying;
        observedClip = wasPlaying ? audioSource.clip : null;
        lastTimeSamples = wasPlaying ? audioSource.timeSamples : 0;
    }

    void Update()
    {
        if(!audioSource)return;

        bool isPlaying = audioSource.isPlaying;

        if(isPlaying)
        {
            if(!wasPlaying)
            {
                waitingForCompletion = true;
                observedClip = audioSource.clip;
            }

            lastTimeSamples = audioSource.timeSamples;
        }
        else if(wasPlaying && waitingForCompletion && ReachedNaturalEnd())
        {
            waitingForCompletion = false;
            onAudioFinished.Invoke();
        }

        wasPlaying = isPlaying;
    }

    bool ReachedNaturalEnd()
    {
        if(!observedClip || observedClip.samples <= 0)return true;

        int toleranceSamples = Mathf.CeilToInt(
            observedClip.frequency * completionTolerance * Mathf.Max(1f, Mathf.Abs(audioSource.pitch)));

        return lastTimeSamples >= observedClip.samples - toleranceSamples;
    }
}
