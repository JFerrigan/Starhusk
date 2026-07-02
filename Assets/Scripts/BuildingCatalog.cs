using UnityEngine;

public struct BuildingDefinition
{
    public string displayName;
    public Sprite placeholderSprite;
    public Color tint;
    public float visualScale;
    public float colliderRadius;
    public int capacity;
    public int extractAmountPerTick;
    public float extractionInterval;
    public ResourceType? requiredResourceType;
    public ResourceStack[] buildCost;
}

public static class BuildingCatalog
{
    public static BuildingDefinition GetDefinition(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Condenser:
                return new BuildingDefinition
                {
                    displayName = "Condenser T1",
                    placeholderSprite = PlaceholderSprites.Condenser,
                    tint = new Color(0.46f, 0.86f, 1f, 1f),
                    visualScale = 6f,
                    colliderRadius = 0.46f,
                    capacity = 250,
                    extractAmountPerTick = 5,
                    extractionInterval = 1f,
                    requiredResourceType = ResourceType.Ice,
                    buildCost = Cost(new ResourceStack(ResourceType.Ore, 5))
                };
            case BuildingType.Harvester:
                return new BuildingDefinition
                {
                    displayName = "Harvester T1",
                    placeholderSprite = PlaceholderSprites.Harvester,
                    tint = new Color(0.46f, 1f, 0.7f, 1f),
                    visualScale = 6f,
                    colliderRadius = 0.46f,
                    capacity = 250,
                    extractAmountPerTick = 5,
                    extractionInterval = 1f,
                    requiredResourceType = ResourceType.Biomass,
                    buildCost = Cost(new ResourceStack(ResourceType.Ore, 5))
                };
            case BuildingType.Dredger:
                return new BuildingDefinition
                {
                    displayName = "Dredger T1",
                    placeholderSprite = PlaceholderSprites.Dredger,
                    tint = new Color(0.98f, 0.78f, 0.42f, 1f),
                    visualScale = 6f,
                    colliderRadius = 0.46f,
                    capacity = 250,
                    extractAmountPerTick = 5,
                    extractionInterval = 1f,
                    requiredResourceType = ResourceType.Silicate,
                    buildCost = Cost(new ResourceStack(ResourceType.Ore, 5))
                };
            case BuildingType.Mine:
            default:
                return new BuildingDefinition
                {
                    displayName = "Mine T1",
                    placeholderSprite = PlaceholderSprites.Mine,
                    tint = new Color(0.78f, 0.86f, 0.94f, 1f),
                    visualScale = 6f,
                    colliderRadius = 0.46f,
                    capacity = 250,
                    extractAmountPerTick = 5,
                    extractionInterval = 1f,
                    requiredResourceType = ResourceType.Ore,
                    buildCost = Cost(new ResourceStack(ResourceType.Ore, 5))
                };
        }
    }

    public static bool IsValidPlacementTarget(BuildingType buildingType, ResourceDeposit deposit)
    {
        if (deposit == null)
        {
            return false;
        }

        MapMarker marker = deposit.GetComponent<MapMarker>();
        if (marker == null || marker.markerType != MapMarkerType.Planet)
        {
            return false;
        }

        DiscoveryState discovery = deposit.GetComponent<DiscoveryState>();
        if (discovery != null && !discovery.discovered)
        {
            return false;
        }

        return true;
    }

    public static bool CanHoldResource(BuildingType buildingType, ResourceType resourceType)
{
    return RequiredResourceFor(buildingType) == resourceType;
}

public static bool CanPlaceOnDeposit(BuildingType buildingType, ResourceDeposit deposit)
{
    if (deposit == null)
    {
        return false;
    }

    return deposit.HasResource(RequiredResourceFor(buildingType));
}

public static ResourceType RequiredResourceFor(BuildingType buildingType)
{
    ResourceType? requiredResourceType = GetDefinition(buildingType).requiredResourceType;
    return requiredResourceType.HasValue ? requiredResourceType.Value : ResourceType.Ore;
}

    public static string GetDisplayName(BuildingType buildingType, BuildingTier tier)
    {
        return GetDefinition(buildingType).displayName;
    }

    private static ResourceStack[] Cost(params ResourceStack[] cost)
    {
        return cost;
    }
}
