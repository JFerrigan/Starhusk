using UnityEngine;

public struct BuildingDefinition
{
    public string displayName;
    public BuildingCategory category;
    public Sprite placeholderSprite;
    public Color tint;
    public float visualScale;
    public float colliderRadius;
    public int capacity;
    public int extractAmountPerTick;
    public float extractionInterval;
    public ResourceType? requiredResourceType;
    public ResourceStack[] buildCost;
    public UpgradeId? upgradeId;

    public bool IsUpgrade => upgradeId.HasValue;
    public bool IsExtractor => !upgradeId.HasValue;
}

public static class BuildingCatalog
{
    private static readonly BuildingType[] OrderedPlanetBuildings =
    {
        BuildingType.Mine,
        BuildingType.Ping3Asteroids,
        BuildingType.PingAsteroidResourceType,
        BuildingType.Ping10Asteroids,
        BuildingType.TripleShotProjectiles,
        BuildingType.HomingProjectiles,
        BuildingType.AsteroidAnnihilator,
        BuildingType.AutopilotUnlock,
        BuildingType.ImpactShield,
        BuildingType.AsteroidCarverHull,
        BuildingType.InfiniteRadarRange,
        BuildingType.PersistentRadarDiscovery,
        BuildingType.Condenser,
        BuildingType.Harvester,
        BuildingType.Dredger
    };

    public static System.Collections.Generic.IReadOnlyList<BuildingType> AllPlanetBuildings => OrderedPlanetBuildings;

    public static BuildingDefinition GetDefinition(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Condenser:
                return new BuildingDefinition
                {
                    displayName = "Condenser T1",
                    category = BuildingCategory.Harvest,
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
                    category = BuildingCategory.Harvest,
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
                    category = BuildingCategory.Harvest,
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
            case BuildingType.Ping3Asteroids:
                return new BuildingDefinition
                {
                    displayName = "Ping 3 Asteroids",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(0.56f, 0.92f, 1f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(new ResourceStack(ResourceType.Ore, 75)),
                    upgradeId = UpgradeId.Ping3Asteroids
                };
            case BuildingType.PingAsteroidResourceType:
                return new BuildingDefinition
                {
                    displayName = "Ping Asteroid Resource Type",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(0.82f, 0.58f, 1f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 100),
                        new ResourceStack(ResourceType.Copper, 60)),
                    upgradeId = UpgradeId.PingAsteroidResourceType
                };
            case BuildingType.Ping10Asteroids:
                return new BuildingDefinition
                {
                    displayName = "Ping 10 Asteroids",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(1f, 0.72f, 0.38f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 150),
                        new ResourceStack(ResourceType.Copper, 90),
                        new ResourceStack(ResourceType.Silicate, 75),
                        new ResourceStack(ResourceType.Ice, 50)),
                    upgradeId = UpgradeId.Ping10Asteroids
                };
            case BuildingType.TripleShotProjectiles:
                return new BuildingDefinition
                {
                    displayName = "Triple Shot Projectiles",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(1f, 0.84f, 0.35f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 125),
                        new ResourceStack(ResourceType.Copper, 60)),
                    upgradeId = UpgradeId.TripleShotProjectiles
                };
            case BuildingType.HomingProjectiles:
                return new BuildingDefinition
                {
                    displayName = "Homing Projectiles",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(0.38f, 1f, 0.66f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 175),
                        new ResourceStack(ResourceType.Copper, 100),
                        new ResourceStack(ResourceType.Silicate, 60)),
                    upgradeId = UpgradeId.HomingProjectiles
                };
            case BuildingType.AsteroidAnnihilator:
                return new BuildingDefinition
                {
                    displayName = "Asteroid Annihilator",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(1f, 0.42f, 0.3f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 250),
                        new ResourceStack(ResourceType.Copper, 140),
                        new ResourceStack(ResourceType.Silicate, 120)),
                    upgradeId = UpgradeId.AsteroidAnnihilator
                };
            case BuildingType.AutopilotUnlock:
                return new BuildingDefinition
                {
                    displayName = "Autopilot Unlock",
                    category = BuildingCategory.Power,
                    placeholderSprite = PlaceholderSprites.SatelliteFactory,
                    tint = new Color(0.48f, 0.72f, 1f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 80),
                        new ResourceStack(ResourceType.Copper, 40)),
                    upgradeId = UpgradeId.AutopilotUnlock
                };
            case BuildingType.ImpactShield:
                return new BuildingDefinition
                {
                    displayName = "Impact Shield",
                    category = BuildingCategory.Power,
                    placeholderSprite = PlaceholderSprites.DysonSatellite,
                    tint = new Color(0.38f, 0.86f, 1f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 120),
                        new ResourceStack(ResourceType.Ice, 80)),
                    upgradeId = UpgradeId.ImpactShield
                };
            case BuildingType.AsteroidCarverHull:
                return new BuildingDefinition
                {
                    displayName = "Asteroid Carver Hull",
                    category = BuildingCategory.Power,
                    placeholderSprite = PlaceholderSprites.DysonSatellite,
                    tint = new Color(0.9f, 0.72f, 0.42f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 180),
                        new ResourceStack(ResourceType.Silicate, 120),
                        new ResourceStack(ResourceType.Copper, 80)),
                    upgradeId = UpgradeId.AsteroidCarverHull
                };
            case BuildingType.InfiniteRadarRange:
                return new BuildingDefinition
                {
                    displayName = "Infinite Radar Range",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(0.25f, 0.95f, 1f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 200),
                        new ResourceStack(ResourceType.Copper, 100),
                        new ResourceStack(ResourceType.Ice, 100)),
                    upgradeId = UpgradeId.InfiniteRadarRange
                };
            case BuildingType.PersistentRadarDiscovery:
                return new BuildingDefinition
                {
                    displayName = "Persistent Radar Discovery",
                    category = BuildingCategory.Fabrication,
                    placeholderSprite = PlaceholderSprites.CollectorHub,
                    tint = new Color(0.72f, 1f, 0.52f, 1f),
                    visualScale = 5.5f,
                    colliderRadius = 0.44f,
                    buildCost = Cost(
                        new ResourceStack(ResourceType.Ore, 220),
                        new ResourceStack(ResourceType.Copper, 120),
                        new ResourceStack(ResourceType.Biomass, 80)),
                    upgradeId = UpgradeId.PersistentRadarDiscovery
                };
            case BuildingType.Mine:
            default:
                return new BuildingDefinition
                {
                    displayName = "Mine T1",
                    category = BuildingCategory.Harvest,
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

    BuildingDefinition definition = GetDefinition(buildingType);
    if (definition.IsUpgrade)
    {
        return true;
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
