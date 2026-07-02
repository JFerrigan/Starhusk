using UnityEngine;

public class BuildingStorage : MonoBehaviour
{
    [SerializeField]
    private ResourceType resourceType = ResourceType.Ore;

    [SerializeField]
    private int currentAmount;

    [SerializeField]
    private int capacity = 250;

    public ResourceType ResourceType => resourceType;
    public int CurrentAmount => currentAmount;
    public int Capacity => Mathf.Max(1, capacity);
    public bool IsFull => currentAmount >= Capacity;
    public bool IsEmpty => currentAmount <= 0;

    public void Configure(ResourceType type, int maxCapacity, int startingAmount = 0)
    {
        resourceType = type;
        capacity = Mathf.Max(1, maxCapacity);
        currentAmount = Mathf.Clamp(startingAmount, 0, Capacity);
    }

    public void SetResourceType(ResourceType type)
    {
        resourceType = type;
    }

    public bool CanStore(ResourceType type)
    {
        return IsEmpty || resourceType == type;
    }

    public int AddResource(ResourceType type, int amount)
    {
        if (amount <= 0 || !CanStore(type))
        {
            return 0;
        }

        if (IsEmpty)
        {
            resourceType = type;
        }

        int acceptedAmount = Mathf.Min(amount, Capacity - currentAmount);
        currentAmount += acceptedAmount;
        return acceptedAmount;
    }

    public int RemoveResource(int amount)
    {
        if (amount <= 0 || currentAmount <= 0)
        {
            return 0;
        }

        int removedAmount = Mathf.Min(amount, currentAmount);
        currentAmount -= removedAmount;
        return removedAmount;
    }
}
