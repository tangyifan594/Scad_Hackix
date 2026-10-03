using UnityEngine;

public class PressureChangeTrigger : MonoBehaviour
{
    public enum Direction
    {
        Higher,
        Lower
    }

    [SerializeField] PressureSystem pressureSystem;
    [SerializeField] Direction direction = Direction.Higher;
    [Min(0f)] [SerializeField] float amount = 10f;

    public Direction ChangeDirection => direction;
    public float Amount => amount;

    void Awake()
    {
        if(!pressureSystem)pressureSystem = FindFirstObjectByType<PressureSystem>();
    }

    public void Trigger()
    {
        if(!pressureSystem)
        {
            Debug.LogWarning($"{name}: PressureSystem reference is missing.", this);
            return;
        }

        if(direction == Direction.Higher)pressureSystem.PressureHigher(amount);
        else pressureSystem.PressureLower(amount);
    }

    public void Configure(PressureSystem system, Direction newDirection, float newAmount)
    {
        pressureSystem = system;
        direction = newDirection;
        amount = Mathf.Max(0f, newAmount);
    }
}
