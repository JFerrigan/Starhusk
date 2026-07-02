using System.Collections.Generic;
using UnityEngine;

public static class BuildResourcePool
{
    public const float BuildRange = 220f;

    private static readonly ResourceType[] ResourceTypes =
    {
        ResourceType.Ore,
        ResourceType.Ice,
        ResourceType.Silicate,
        ResourceType.Copper,
        ResourceType.Biomass
    };

    public static int GetAvailable(ResourceType type)
    {
        ResourceInventory inventory = FindInventory();
        return GetAvailable(type, inventory, BuildRange);
    }

    public static int GetAvailable(ResourceType type, ResourceInventory inventory, float range)
    {
        if (inventory == null)
        {
            return 0;
        }

        Vector2 center = inventory.transform.position;
        float rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range);
        int total = inventory.GetAmount(type);

        ResourceStorage[] resourceStorages = Object.FindObjectsByType<ResourceStorage>(FindObjectsSortMode.None);
        for (int i = 0; i < resourceStorages.Length; i++)
        {
            ResourceStorage storage = resourceStorages[i];
            if (storage == null || !IsWithinRange(center, storage.transform.position, rangeSquared))
            {
                continue;
            }

            total += storage.GetAmount(type);
        }

        BuildingStorage[] buildingStorages = Object.FindObjectsByType<BuildingStorage>(FindObjectsSortMode.None);
        for (int i = 0; i < buildingStorages.Length; i++)
        {
            BuildingStorage storage = buildingStorages[i];
            if (storage == null || storage.ResourceType != type || !IsWithinRange(center, storage.transform.position, rangeSquared))
            {
                continue;
            }

            total += storage.CurrentAmount;
        }

        return total;
    }

    public static bool CanAfford(ResourceStack[] cost)
    {
        ResourceInventory inventory = FindInventory();
        return CanAfford(cost, inventory, BuildRange);
    }

    public static bool CanAfford(ResourceStack[] cost, ResourceInventory inventory, float range)
    {
        if (IsFree(cost))
        {
            return true;
        }

        if (inventory == null)
        {
            return false;
        }

        for (int i = 0; i < cost.Length; i++)
        {
            ResourceStack requirement = cost[i];
            if (requirement.amount > 0 && GetAvailable(requirement.type, inventory, range) < requirement.amount)
            {
                return false;
            }
        }

        return true;
    }

    public static bool Spend(ResourceStack[] cost)
    {
        ResourceInventory inventory = FindInventory();
        return Spend(cost, inventory, BuildRange);
    }

    public static bool Spend(ResourceStack[] cost, ResourceInventory inventory, float range)
    {
        if (IsFree(cost))
        {
            return true;
        }

        if (!CanAfford(cost, inventory, range))
        {
            return false;
        }

        Vector2 center = inventory.transform.position;
        for (int i = 0; i < cost.Length; i++)
        {
            ResourceStack requirement = cost[i];
            int remaining = Mathf.Max(0, requirement.amount);
            if (remaining <= 0)
            {
                continue;
            }

            remaining -= inventory.RemoveResource(requirement.type, remaining);

            List<StorageSpendSource> sources = GatherSpendSources(requirement.type, center, range);
            for (int sourceIndex = 0; sourceIndex < sources.Count && remaining > 0; sourceIndex++)
            {
                remaining -= sources[sourceIndex].Remove(requirement.type, remaining);
            }
        }

        return true;
    }

    public static int[] GetTotals(ResourceInventory inventory, float range)
    {
        int[] totals = new int[ResourceTypes.Length];
        for (int i = 0; i < ResourceTypes.Length; i++)
        {
            totals[i] = GetAvailable(ResourceTypes[i], inventory, range);
        }

        return totals;
    }

    private static List<StorageSpendSource> GatherSpendSources(ResourceType type, Vector2 center, float range)
    {
        float rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range);
        List<StorageSpendSource> sources = new List<StorageSpendSource>();

        ResourceStorage[] resourceStorages = Object.FindObjectsByType<ResourceStorage>(FindObjectsSortMode.None);
        for (int i = 0; i < resourceStorages.Length; i++)
        {
            ResourceStorage storage = resourceStorages[i];
            if (storage == null || storage.GetAmount(type) <= 0)
            {
                continue;
            }

            float distanceSquared = DistanceSquared(center, storage.transform.position);
            if (distanceSquared <= rangeSquared)
            {
                sources.Add(new StorageSpendSource(storage, distanceSquared));
            }
        }

        BuildingStorage[] buildingStorages = Object.FindObjectsByType<BuildingStorage>(FindObjectsSortMode.None);
        for (int i = 0; i < buildingStorages.Length; i++)
        {
            BuildingStorage storage = buildingStorages[i];
            if (storage == null || storage.ResourceType != type || storage.CurrentAmount <= 0)
            {
                continue;
            }

            float distanceSquared = DistanceSquared(center, storage.transform.position);
            if (distanceSquared <= rangeSquared)
            {
                sources.Add(new StorageSpendSource(storage, distanceSquared));
            }
        }

        sources.Sort((left, right) => left.DistanceSquared.CompareTo(right.DistanceSquared));
        return sources;
    }

    private static bool IsFree(ResourceStack[] cost)
    {
        return cost == null || cost.Length == 0;
    }

    private static ResourceInventory FindInventory()
    {
        return Object.FindFirstObjectByType<ResourceInventory>();
    }

    private static bool IsWithinRange(Vector2 center, Vector3 position, float rangeSquared)
    {
        return DistanceSquared(center, position) <= rangeSquared;
    }

    private static float DistanceSquared(Vector2 center, Vector3 position)
    {
        return Vector2.SqrMagnitude(center - new Vector2(position.x, position.y));
    }

    private sealed class StorageSpendSource
    {
        private readonly ResourceStorage resourceStorage;
        private readonly BuildingStorage buildingStorage;

        public float DistanceSquared { get; }

        public StorageSpendSource(ResourceStorage storage, float distanceSquared)
        {
            resourceStorage = storage;
            DistanceSquared = distanceSquared;
        }

        public StorageSpendSource(BuildingStorage storage, float distanceSquared)
        {
            buildingStorage = storage;
            DistanceSquared = distanceSquared;
        }

        public int Remove(ResourceType type, int amount)
        {
            if (resourceStorage != null)
            {
                return resourceStorage.RemoveResource(type, amount);
            }

            return buildingStorage == null || buildingStorage.ResourceType != type ? 0 : buildingStorage.RemoveResource(amount);
        }
    }
}
