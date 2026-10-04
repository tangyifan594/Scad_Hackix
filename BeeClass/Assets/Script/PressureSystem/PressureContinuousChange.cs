using UnityEngine;

public class PressureContinuousChange : MonoBehaviour
{
    [Header("Pressure Change")]
    [SerializeField] PressureSystem pressureSystem;

    [Tooltip("Pressure changed per second. Positive values increase pressure; negative values lower it.")]
    [SerializeField] float changePerSecond = 5f;

    [Tooltip("Start changing pressure automatically whenever this component is enabled.")]
    [SerializeField] bool startOnEnable;

    [Tooltip("Continue changing pressure even when Time.timeScale is zero.")]
    [SerializeField] bool useUnscaledTime;

    bool isChanging;

    public bool IsChanging => isChanging;
    public float ChangePerSecond => changePerSecond;

    void Awake()
    {
        FindPressureSystemIfMissing();
    }

    void OnEnable()
    {
        if(startOnEnable)StartChanging();
    }

    void OnDisable()
    {
        StopChanging();
    }

    void Update()
    {
        if(!isChanging)return;
        FindPressureSystemIfMissing();
        if(!pressureSystem)return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        pressureSystem.ChangePressure(changePerSecond * deltaTime);
    }

    public void StartChanging()
    {
        FindPressureSystemIfMissing();
        if(!pressureSystem)
        {
            Debug.LogWarning($"{name}: PressureSystem reference is missing.", this);
            return;
        }

        isChanging = true;
    }

    public void StopChanging()
    {
        isChanging = false;
    }

    public void SetChangePerSecond(float value)
    {
        changePerSecond = value;
    }

    public void Configure(PressureSystem system, float amountPerSecond)
    {
        pressureSystem = system;
        changePerSecond = amountPerSecond;
    }

    void FindPressureSystemIfMissing()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
    }
}
