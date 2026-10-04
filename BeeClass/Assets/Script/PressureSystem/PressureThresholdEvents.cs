using UnityEngine;
using UnityEngine.Events;

public class PressureThresholdEvents : MonoBehaviour
{
    enum ThresholdRegion
    {
        Unknown,
        Below,
        Equal,
        Above
    }

    [SerializeField] PressureSystem pressureSystem;
    [SerializeField] float threshold = 50f;

    [Header("Threshold Events")]
    [Tooltip("Invoked once when pressure enters the region below the threshold.")]
    [SerializeField] UnityEvent onPressureBelow = new UnityEvent();
    [Tooltip("Invoked once when pressure enters the region above the threshold.")]
    [SerializeField] UnityEvent onPressureAbove = new UnityEvent();

    ThresholdRegion currentRegion = ThresholdRegion.Unknown;

    public float Threshold => threshold;
    public UnityEvent OnPressureBelow => onPressureBelow;
    public UnityEvent OnPressureAbove => onPressureAbove;

    void Awake()
    {
        FindPressureSystemIfMissing();
    }

    void OnEnable()
    {
        FindPressureSystemIfMissing();
        if(pressureSystem)pressureSystem.OnPressureChanged += HandlePressureChanged;
        EvaluatePressure(true);
    }

    void OnDisable()
    {
        if(pressureSystem)pressureSystem.OnPressureChanged -= HandlePressureChanged;
        currentRegion = ThresholdRegion.Unknown;
    }

    void HandlePressureChanged(float normalizedPressure)
    {
        EvaluatePressure(false);
    }

    public void EvaluateNow()
    {
        EvaluatePressure(true);
    }

    public void SetThreshold(float value)
    {
        threshold = value;
        EvaluatePressure(true);
    }

    public void Configure(PressureSystem system, float newThreshold)
    {
        if(pressureSystem)pressureSystem.OnPressureChanged -= HandlePressureChanged;
        pressureSystem = system;
        threshold = newThreshold;
        currentRegion = ThresholdRegion.Unknown;
        if(isActiveAndEnabled && pressureSystem)pressureSystem.OnPressureChanged += HandlePressureChanged;
        if(Application.isPlaying)EvaluatePressure(true);
    }

    void EvaluatePressure(bool force)
    {
        if(!pressureSystem)return;

        float value = pressureSystem.CurrentPressure;
        ThresholdRegion nextRegion = value < threshold
            ? ThresholdRegion.Below
            : value > threshold
                ? ThresholdRegion.Above
                : ThresholdRegion.Equal;

        if(!force && nextRegion == currentRegion)return;
        currentRegion = nextRegion;

        if(nextRegion == ThresholdRegion.Below)onPressureBelow.Invoke();
        else if(nextRegion == ThresholdRegion.Above)onPressureAbove.Invoke();
    }

    void FindPressureSystemIfMissing()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
    }
}
