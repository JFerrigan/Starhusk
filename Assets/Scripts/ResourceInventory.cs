using UnityEngine;
using System.Collections.Generic;

public class ResourceInventory : MonoBehaviour
{
    [SerializeField]
    private List<ResourceStack> resources = new List<ResourceStack>();

    [Header("Legacy")]
    public int ore = 0;

    private void Awake()
    {
        if (ore > 0 && GetAmount(ResourceType.Ore) == 0)
        {
            AddResource(ResourceType.Ore, ore);
        }

        SyncLegacyFields();
    }

    public void AddOre(int amount)
    {
        AddResource(ResourceType.Ore, amount);
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];

            if (stack.type == type)
            {
                stack.amount += amount;
                resources[i] = stack;
                SyncLegacyFields();
                Debug.Log(type + ": " + stack.amount);
                return;
            }
        }

        resources.Add(new ResourceStack(type, amount));
        SyncLegacyFields();
        Debug.Log(type + ": " + amount);
    }

    public int GetAmount(ResourceType type)
    {
        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].type == type)
            {
                return resources[i].amount;
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

    public IReadOnlyList<ResourceStack> GetResources()
    {
        return resources;
    }

    private void SyncLegacyFields()
    {
        ore = GetAmount(ResourceType.Ore);
    }
}
