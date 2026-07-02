using NUnit.Framework;
using UnityEngine;

public class BuildingSystemTests
{
    [Test]
    public void PlacementTargetRequiresDiscoveredPlanet()
    {
        GameObject planet = new GameObject("Planet");

        try
        {
            planet.AddComponent<CircleCollider2D>();
            ResourceDeposit deposit = planet.AddComponent<ResourceDeposit>();
            MapMarker marker = planet.AddComponent<MapMarker>();
            DiscoveryState discovery = planet.AddComponent<DiscoveryState>();
            marker.markerType = MapMarkerType.Planet;

            discovery.SetDiscovered(false);
            Assert.IsFalse(BuildingPlacementController.IsValidMinePlacementTarget(deposit));

            discovery.SetDiscovered(true);
            Assert.IsTrue(BuildingPlacementController.IsValidMinePlacementTarget(deposit));
        }
        finally
        {
            Object.DestroyImmediate(planet);
        }
    }

    [Test]
    public void SurfaceAnchorSnapsBuildingOutsidePlanetRadius()
    {
        GameObject planet = new GameObject("Planet");
        GameObject mineObject = new GameObject("Mine");

        try
        {
            planet.transform.localScale = Vector3.one * 30f;
            CircleCollider2D planetCollider = planet.AddComponent<CircleCollider2D>();
            planetCollider.radius = 0.5f;

            mineObject.transform.localScale = Vector3.one * 6f;
            CircleCollider2D mineCollider = mineObject.AddComponent<CircleCollider2D>();
            mineCollider.radius = 0.46f;

            PlanetSurfaceAnchor anchor = mineObject.AddComponent<PlanetSurfaceAnchor>();
            anchor.Bind(planet.transform, Vector2.right);
            anchor.SnapToSurface();

            float snappedDistance = Vector2.Distance(planet.transform.position, mineObject.transform.position);
            float expectedMinimum = PlanetSurfaceAnchor.SurfaceRadiusFor(planet.transform);

            Assert.That(snappedDistance, Is.GreaterThan(expectedMinimum));
        }
        finally
        {
            Object.DestroyImmediate(mineObject);
            Object.DestroyImmediate(planet);
        }
    }

    [Test]
    public void MineAdoptsPlanetResourceTypeAndExtractsIntoStorage()
    {
        GameObject planet = CreatePlanet("Ore Planet", ResourceType.Copper, 40, true);
        GameObject mineObject = CreateMine();

        try
        {
            PlanetMine mine = mineObject.GetComponent<PlanetMine>();
            ResourceDeposit deposit = planet.GetComponent<ResourceDeposit>();

            mine.Initialize(deposit, planet.transform, Vector2.up);
            int extracted = mine.ExtractOnce();

            Assert.That(extracted, Is.EqualTo(PlanetMine.Tier1ExtractAmount));
            Assert.That(mine.Storage.ResourceType, Is.EqualTo(ResourceType.Copper));
            Assert.That(mine.Storage.CurrentAmount, Is.EqualTo(PlanetMine.Tier1ExtractAmount));
            Assert.That(deposit.remainingAmount, Is.EqualTo(35));
        }
        finally
        {
            Object.DestroyImmediate(mineObject);
            Object.DestroyImmediate(planet);
        }
    }

    [Test]
    public void MineStopsWhenStorageIsFull()
    {
        GameObject planet = CreatePlanet("Full Planet", ResourceType.Ore, 500, true);
        GameObject mineObject = CreateMine();

        try
        {
            PlanetMine mine = mineObject.GetComponent<PlanetMine>();
            mine.Initialize(planet.GetComponent<ResourceDeposit>(), planet.transform, Vector2.up);

            int totalExtracted = 0;
            while (!mine.Storage.IsFull)
            {
                totalExtracted += mine.ExtractOnce();
            }

            int afterFull = mine.ExtractOnce();

            Assert.That(totalExtracted, Is.EqualTo(mine.Storage.Capacity));
            Assert.That(afterFull, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(mineObject);
            Object.DestroyImmediate(planet);
        }
    }

    [Test]
    public void SpecializedBuildingsOnlyAcceptMatchingPlanetResources()
    {
        GameObject icePlanet = CreatePlanet("Ice Planet", ResourceType.Ice, 300, true);
        GameObject biomassPlanet = CreatePlanet("Biomass Planet", ResourceType.Biomass, 300, true);
        GameObject silicatePlanet = CreatePlanet("Silicate Planet", ResourceType.Silicate, 300, true);
        GameObject condenserObject = CreateExtractor(BuildingType.Condenser);
        GameObject harvesterObject = CreateExtractor(BuildingType.Harvester);
        GameObject dredgerObject = CreateExtractor(BuildingType.Dredger);

        try
        {
            PlanetCondenser condenser = condenserObject.GetComponent<PlanetCondenser>();
            PlanetHarvester harvester = harvesterObject.GetComponent<PlanetHarvester>();
            PlanetDredger dredger = dredgerObject.GetComponent<PlanetDredger>();

            condenser.Initialize(icePlanet.GetComponent<ResourceDeposit>(), icePlanet.transform, Vector2.up);
            harvester.Initialize(biomassPlanet.GetComponent<ResourceDeposit>(), biomassPlanet.transform, Vector2.up);
            dredger.Initialize(silicatePlanet.GetComponent<ResourceDeposit>(), silicatePlanet.transform, Vector2.up);

            Assert.IsTrue(BuildingCatalog.IsValidPlacementTarget(BuildingType.Condenser, biomassPlanet.GetComponent<ResourceDeposit>()));
            Assert.IsTrue(BuildingCatalog.IsValidPlacementTarget(BuildingType.Harvester, silicatePlanet.GetComponent<ResourceDeposit>()));
            Assert.IsTrue(BuildingCatalog.IsValidPlacementTarget(BuildingType.Dredger, icePlanet.GetComponent<ResourceDeposit>()));
            Assert.That(condenser.Storage.ResourceType, Is.EqualTo(ResourceType.Ice));
            Assert.That(harvester.Storage.ResourceType, Is.EqualTo(ResourceType.Biomass));
            Assert.That(dredger.Storage.ResourceType, Is.EqualTo(ResourceType.Silicate));

            Assert.IsTrue(condenser.CanRelocateTo(icePlanet.GetComponent<ResourceDeposit>()));
            Assert.IsFalse(condenser.CanRelocateTo(biomassPlanet.GetComponent<ResourceDeposit>()));
            Assert.IsTrue(harvester.CanRelocateTo(biomassPlanet.GetComponent<ResourceDeposit>()));
            Assert.IsFalse(harvester.CanRelocateTo(silicatePlanet.GetComponent<ResourceDeposit>()));
            Assert.IsTrue(dredger.CanRelocateTo(silicatePlanet.GetComponent<ResourceDeposit>()));
            Assert.IsFalse(dredger.CanRelocateTo(icePlanet.GetComponent<ResourceDeposit>()));

            Assert.That(condenser.ExtractOnce(), Is.EqualTo(PlanetMine.Tier1ExtractAmount));
            Assert.That(harvester.ExtractOnce(), Is.EqualTo(PlanetMine.Tier1ExtractAmount));
            Assert.That(dredger.ExtractOnce(), Is.EqualTo(PlanetMine.Tier1ExtractAmount));
        }
        finally
        {
            Object.DestroyImmediate(condenserObject);
            Object.DestroyImmediate(harvesterObject);
            Object.DestroyImmediate(dredgerObject);
            Object.DestroyImmediate(icePlanet);
            Object.DestroyImmediate(biomassPlanet);
            Object.DestroyImmediate(silicatePlanet);
        }
    }

    [Test]
    public void MoveKeepsStoredResourcesAndRejectsDifferentResourcePlanet()
    {
        GameObject orePlanet = CreatePlanet("Ore Planet", ResourceType.Ore, 300, true);
        GameObject copperPlanet = CreatePlanet("Copper Planet", ResourceType.Copper, 300, true);
        GameObject mineObject = CreateMine();

        try
        {
            PlanetMine mine = mineObject.GetComponent<PlanetMine>();
            mine.Initialize(orePlanet.GetComponent<ResourceDeposit>(), orePlanet.transform, Vector2.up);
            mine.ExtractOnce();

            Assert.IsFalse(mine.CanRelocateTo(copperPlanet.GetComponent<ResourceDeposit>()));
            Assert.IsTrue(mine.CanRelocateTo(orePlanet.GetComponent<ResourceDeposit>()));
            Assert.That(mine.Storage.CurrentAmount, Is.EqualTo(PlanetMine.Tier1ExtractAmount));
        }
        finally
        {
            Object.DestroyImmediate(mineObject);
            Object.DestroyImmediate(orePlanet);
            Object.DestroyImmediate(copperPlanet);
        }
    }

    private static GameObject CreatePlanet(string name, ResourceType resourceType, int amount, bool discovered)
    {
        GameObject planet = new GameObject(name);
        planet.transform.localScale = Vector3.one * 30f;
        CircleCollider2D collider = planet.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;

        ResourceDeposit deposit = planet.AddComponent<ResourceDeposit>();
        deposit.resourceType = resourceType;
        deposit.maxAmount = amount;
        deposit.remainingAmount = amount;
        deposit.destroyWhenDepleted = false;

        MapMarker marker = planet.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Planet;

        DiscoveryState discovery = planet.AddComponent<DiscoveryState>();
        discovery.SetDiscovered(discovered);

        return planet;
    }

    private static GameObject CreateMine()
    {
        GameObject mineObject = new GameObject("Mine");
        mineObject.transform.localScale = Vector3.one * 6f;
        mineObject.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = mineObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.46f;
        mineObject.AddComponent<PlanetSurfaceAnchor>();
        mineObject.AddComponent<BuildingStorage>();
        mineObject.AddComponent<PlanetMine>();
        return mineObject;
    }

    private static GameObject CreateExtractor(BuildingType buildingType)
    {
        GameObject buildingObject = new GameObject(buildingType + " Building");
        buildingObject.transform.localScale = Vector3.one * 6f;
        buildingObject.AddComponent<SpriteRenderer>();
        CircleCollider2D collider = buildingObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.46f;
        buildingObject.AddComponent<PlanetSurfaceAnchor>();
        buildingObject.AddComponent<BuildingStorage>();

        switch (buildingType)
        {
            case BuildingType.Condenser:
                buildingObject.AddComponent<PlanetCondenser>();
                break;
            case BuildingType.Harvester:
                buildingObject.AddComponent<PlanetHarvester>();
                break;
            case BuildingType.Dredger:
                buildingObject.AddComponent<PlanetDredger>();
                break;
            case BuildingType.Mine:
            default:
                buildingObject.AddComponent<PlanetMine>();
                break;
        }

        return buildingObject;
    }
}
