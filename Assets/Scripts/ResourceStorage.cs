using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : MonoBehaviour
{
    [SerializeField]
    private int capacity = 500;

    [SerializeField]
    private List<ResourceStack> resources = new List<ResourceStack>();

    public int Capacity => Mathf.Max(1, capacity);
    public int TotalAmount
    {
        get
        {
            int total = 0;
            for (int i = 0; i < resources.Count; i++)
            {
                total += Mathf.Max(0, resources[i].amount);
            }

            return total;
        }
    }

    public int RemainingCapacity => Mathf.Max(0, Capacity - TotalAmount);
    public bool IsEmpty => TotalAmount <= 0;
    public bool IsFull => RemainingCapacity <= 0;

    public void Configure(int maxCapacity)
    {
        capacity = Mathf.Max(1, maxCapacity);
        ClampToCapacity();
    }

    public int GetAmount(ResourceType type)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].type == type)
            {
                return Mathf.Max(0, resources[i].amount);
            }
        }

        return 0;
    }

    public IReadOnlyList<ResourceStack> GetResources()
    {
        return resources;
    }

    public int AddResource(ResourceType type, int amount)
    {
        if (amount <= 0 || IsFull)
        {
            return 0;
        }

        int acceptedAmount = Mathf.Min(amount, RemainingCapacity);
        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];
            if (stack.type == type)
            {
                stack.amount += acceptedAmount;
                resources[i] = stack;
                return acceptedAmount;
            }
        }

        resources.Add(new ResourceStack(type, acceptedAmount));
        return acceptedAmount;
    }

    public int RemoveResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];
            if (stack.type != type)
            {
                continue;
            }

            int removedAmount = Mathf.Min(amount, Mathf.Max(0, stack.amount));
            stack.amount -= removedAmount;

            if (stack.amount <= 0)
            {
                resources.RemoveAt(i);
            }
            else
            {
                resources[i] = stack;
            }

            return removedAmount;
        }

        return 0;
    }

    public int TransferAllTo(ResourceStorage destination)
    {
        if (destination == null || destination.IsFull || IsEmpty)
        {
            return 0;
        }

        int transferredTotal = 0;
        for (int i = resources.Count - 1; i >= 0; i--)
        {
            ResourceStack stack = resources[i];
            int acceptedAmount = destination.AddResource(stack.type, stack.amount);
            if (acceptedAmount <= 0)
            {
                continue;
            }

            stack.amount -= acceptedAmount;
            transferredTotal += acceptedAmount;

            if (stack.amount <= 0)
            {
                resources.RemoveAt(i);
            }
            else
            {
                resources[i] = stack;
            }
        }

        return transferredTotal;
    }

    private void ClampToCapacity()
    {
        int overflow = TotalAmount - Capacity;
        for (int i = resources.Count - 1; i >= 0 && overflow > 0; i--)
        {
            ResourceStack stack = resources[i];
            int removedAmount = Mathf.Min(stack.amount, overflow);
            stack.amount -= removedAmount;
            overflow -= removedAmount;

            if (stack.amount <= 0)
            {
                resources.RemoveAt(i);
            }
            else
            {
                resources[i] = stack;
            }
        }
    }
}
