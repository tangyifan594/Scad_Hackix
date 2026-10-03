using System;
using UnityEngine;

public class PressureSystem : MonoBehaviour
{
    [Header("Pressure Range")]
    [SerializeField] float minimumPressure = 0f;
    [SerializeField] float maximumPressure = 100f;
    [SerializeField] float currentPressure = 50f;

    public float CurrentPressure => currentPressure;
    public float MinimumPressure => minimumPressure;
    public float MaximumPressure => maximumPressure;
    public float NormalizedPressure => Mathf.InverseLerp(minimumPressure, maximumPressure, currentPressure);

    public event Action<float> OnPressureChanged;

    void Awake()
    {
        ClampPressure();
    }

    void Start()
    {
        OnPressureChanged?.Invoke(NormalizedPressure);
    }

    void OnValidate()
    {
        if(maximumPressure <= minimumPressure)maximumPressure = minimumPressure + 1f;
        ClampPressure();
    }

    public void PressureHigher(float amount)
    {
        ChangePressure(Mathf.Abs(amount));
    }

    public void PressureLower(float amount)
    {
        ChangePressure(-Mathf.Abs(amount));
    }

    public void SetPressure(float value)
    {
        ApplyPressure(value);
    }

    public void ChangePressure(float amount)
    {
        ApplyPressure(currentPressure + amount);
    }

    void ApplyPressure(float value)
    {
        float next = Mathf.Clamp(value, minimumPressure, maximumPressure);
        if(Mathf.Approximately(next, currentPressure))return;
        currentPressure = next;
        OnPressureChanged?.Invoke(NormalizedPressure);
    }

    void ClampPressure()
    {
        currentPressure = Mathf.Clamp(currentPressure, minimumPressure, maximumPressure);
    }
}
