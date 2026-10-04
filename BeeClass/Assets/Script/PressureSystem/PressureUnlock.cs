using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PressureUnlock : MonoBehaviour
{
    [Header("Pressure Condition")]
    [SerializeField] PressureSystem pressureSystem;
    [Tooltip("Unlocks when the current pressure is strictly lower than this value.")]
    [SerializeField] float unlockBelowPressure = 20f;

    [Header("UI")]
    [SerializeField] Button actionButton;
    [SerializeField] GameObject lockVisual;
    [SerializeField] Text buttonLabel;

    [Header("Events")]
    [SerializeField] UnityEvent onUnlocked = new UnityEvent();
    [SerializeField] UnityEvent onActivated = new UnityEvent();

    bool isUnlocked;
    bool isActivated;

    public bool IsUnlocked => isUnlocked;
    public bool IsActivated => isActivated;
    public float UnlockBelowPressure => unlockBelowPressure;
    public UnityEvent OnUnlocked => onUnlocked;
    public UnityEvent OnActivated => onActivated;

    void Awake()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
    }

    void OnEnable()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
        if(pressureSystem)pressureSystem.OnPressureChanged += HandlePressureChanged;
        BindButton();
        RefreshState();
    }

    void OnDisable()
    {
        if(pressureSystem)pressureSystem.OnPressureChanged -= HandlePressureChanged;
        if(actionButton)actionButton.onClick.RemoveListener(Activate);
    }

    void HandlePressureChanged(float normalizedPressure)
    {
        RefreshState();
    }

    void RefreshState()
    {
        if(!isUnlocked && pressureSystem && pressureSystem.CurrentPressure < unlockBelowPressure)
        {
            isUnlocked = true;
            if(Application.isPlaying)onUnlocked.Invoke();
        }

        if(actionButton)actionButton.interactable = isUnlocked && !isActivated;
        if(lockVisual)lockVisual.SetActive(!isUnlocked);
        if(buttonLabel)
        {
            buttonLabel.gameObject.SetActive(isUnlocked);
            buttonLabel.text = isActivated ? "已激活" : "点击激活";
        }
    }

    public void Activate()
    {
        if(!isUnlocked || isActivated)return;
        isActivated = true;
        RefreshState();
        onActivated.Invoke();
    }

    public void Configure(PressureSystem system, float threshold, Button button, GameObject lockObject, Text label)
    {
        if(pressureSystem)pressureSystem.OnPressureChanged -= HandlePressureChanged;
        pressureSystem = system;
        unlockBelowPressure = threshold;
        actionButton = button;
        lockVisual = lockObject;
        buttonLabel = label;
        if(actionButton)actionButton.transition = Selectable.Transition.None;
        if(isActiveAndEnabled && pressureSystem)pressureSystem.OnPressureChanged += HandlePressureChanged;
        BindButton();
        RefreshState();
    }

    void BindButton()
    {
        if(!actionButton)return;
        actionButton.onClick.RemoveListener(Activate);
        actionButton.onClick.AddListener(Activate);
    }
}
