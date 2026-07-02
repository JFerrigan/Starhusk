using System.Collections.Generic;
using UnityEngine;

public class ResourceDeposit : MonoBehaviour
{
    [Header("Legacy / Primary Resource")]
    public ResourceType resourceType = ResourceType.Ore;
    public int maxAmount = 100;
    public int remainingAmount = 100;
    public int mineAmountPerInteraction = 10;
    public bool destroyWhenDepleted = true;

    [Header("Resources Available Here")]
    [SerializeField]
    private List<ResourceStack> resources = new List<ResourceStack>();

    public IReadOnlyList<ResourceStack> Resources => resources;

    public int TotalRemainingAmount
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

    public bool IsDepleted => TotalRemainingAmount <= 0;

    private void Awake()
    {
        if (resources.Count <= 0 && remainingAmount > 0)
        {
            resources.Add(new ResourceStack(resourceType, Mathf.Max(1, remainingAmount)));
        }

        mineAmountPerInteraction = Mathf.Max(1, mineAmountPerInteraction);
        SyncLegacyFields();
    }

    public void ConfigureSingleResource(ResourceType type, int amount, int mineAmount = 10)
    {
        resourceType = type;
        mineAmountPerInteraction = Mathf.Max(1, mineAmount);

        resources.Clear();
        resources.Add(new ResourceStack(type, Mathf.Max(1, amount)));

        SyncLegacyFields();
    }

    public void ConfigureResources(IReadOnlyList<ResourceStack> resourceStacks, int mineAmount = 10)
    {
        mineAmountPerInteraction = Mathf.Max(1, mineAmount);
        resources.Clear();

        if (resourceStacks != null)
        {
            for (int i = 0; i < resourceStacks.Count; i++)
            {
                ResourceStack stack = resourceStacks[i];
                AddOrMergeResource(stack.type, stack.amount);
            }
        }

        if (resources.Count <= 0)
        {
            resources.Add(new ResourceStack(ResourceType.Ore, 1));
        }

        resourceType = resources[0].type;
        SyncLegacyFields();
    }

    public bool HasResource(ResourceType type)
    {
        return GetAmount(type) > 0;
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

            SyncLegacyFields();
            return removedAmount;
        }

        return 0;
    }

    public int Mine(ResourceInventory inventory)
    {
        if (inventory == null || IsDepleted)
        {
            return 0;
        }

        int minedAmount = RemoveResource(resourceType, mineAmountPerInteraction);
        if (minedAmount <= 0)
        {
            return 0;
        }

        inventory.AddResource(resourceType, minedAmount);

        if (IsDepleted && destroyWhenDepleted)
        {
            Destroy(gameObject);
        }

        return minedAmount;
    }

    private void AddOrMergeResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];
            if (stack.type != type)
            {
                continue;
            }

            stack.amount += amount;
            resources[i] = stack;
            return;
        }

        resources.Add(new ResourceStack(type, amount));
    }

    private void SyncLegacyFields()
    {
        if (resources.Count > 0)
        {
            bool primaryStillExists = false;
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].type == resourceType)
                {
                    primaryStillExists = true;
                    break;
                }
            }

            if (!primaryStillExists)
            {
                resourceType = resources[0].type;
            }
        }

        remainingAmount = GetAmount(resourceType);
        maxAmount = Mathf.Max(maxAmount, TotalRemainingAmount, remainingAmount);
    }
}