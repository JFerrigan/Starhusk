using NUnit.Framework;
using UnityEngine;

public class AutomatonSystemTests
{
    [Test]
    public void MixedResourceStorageRespectsCapacity()
    {
        GameObject storageObject = new GameObject("Storage");

        try
        {
            ResourceStorage storage = storageObject.AddComponent<ResourceStorage>();
            storage.Configure(10);

            Assert.That(storage.AddResource(ResourceType.Ore, 7), Is.EqualTo(7));
            Assert.That(storage.AddResource(ResourceType.Ice, 7), Is.EqualTo(3));
            Assert.That(storage.TotalAmount, Is.EqualTo(10));
            Assert.That(storage.GetAmount(ResourceType.Ore), Is.EqualTo(7));
            Assert.That(storage.GetAmount(ResourceType.Ice), Is.EqualTo(3));
        }
        finally
        {
            Object.DestroyImmediate(storageObject);
        }
    }

    [Test]
    public void BuildingStorageWithdrawsPartialResources()
    {
        GameObject buildingObject = new GameObject("Building");

        try
        {
            BuildingStorage storage = buildingObject.AddComponent<BuildingStorage>();
            storage.Configure(ResourceType.Silicate, 250);
            storage.AddResource(ResourceType.Silicate, 25);

            Assert.That(storage.RemoveResource(10), Is.EqualTo(10));
            Assert.That(storage.CurrentAmount, Is.EqualTo(15));
            Assert.That(storage.RemoveResource(50), Is.EqualTo(15));
            Assert.That(storage.CurrentAmount, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(buildingObject);
        }
    }

    [Test]
    public void CollectorChoosesLargestNearbyBuildingStockpile()
    {
        GameObject lowBuilding = CreateExtractor("Low", new Vector2(2f, 0f), 20);
        GameObject highBuilding = CreateExtractor("High", new Vector2(4f, 0f), 80);
        GameObject collectorObject = CreateCollector(Vector2.zero);

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.scanRange = 20f;

            Assert.That(collector.FindBestBuilding(), Is.EqualTo(highBuilding.GetComponent<PlanetMine>()));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(lowBuilding);
            Object.DestroyImmediate(highBuilding);
        }
    }

    [Test]
    public void CollectorDepositsMixedCargoIntoHub()
    {
        GameObject collectorObject = CreateCollector(Vector2.zero);
        GameObject hubObject = CreateHub(new Vector2(4f, 0f));

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            CollectorHub hub = hubObject.GetComponent<CollectorHub>();

            collector.Cargo.AddResource(ResourceType.Ore, 100);
            collector.Cargo.AddResource(ResourceType.Ice, 50);

            int transferred = collector.Cargo.TransferAllTo(hub.Storage);

            Assert.That(transferred, Is.EqualTo(150));
            Assert.That(collector.Cargo.TotalAmount, Is.EqualTo(0));
            Assert.That(hub.Storage.GetAmount(ResourceType.Ore), Is.EqualTo(100));
            Assert.That(hub.Storage.GetAmount(ResourceType.Ice), Is.EqualTo(50));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(hubObject);
        }
    }

    [Test]
    public void CollectorIdlesWhenNoHubExists()
    {
        GameObject building = CreateExtractor("Stocked", new Vector2(3f, 0f), 80);
        GameObject collectorObject = CreateCollector(Vector2.zero);

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.Cargo.AddResource(ResourceType.Ore, 50);

            collector.EvaluateGoal();

            Assert.That(collector.State, Is.EqualTo(CollectorState.Idle));
            Assert.IsNull(collector.TargetHub);
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(building);
        }
    }

    [Test]
    public void CollectorInteractionRangeIncludesColliderRadii()
    {
        GameObject collectorObject = CreateCollector(Vector2.zero);
        GameObject hubObject = CreateHub(new Vector2(7f, 0f));

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();

            float range = collector.InteractionRangeFor(hubObject.transform);

            Assert.That(range, Is.GreaterThan(collector.interactionDistance));
            Assert.IsTrue(collector.IsWithinInteractionRange(hubObject.transform));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(hubObject);
        }
    }

    [Test]
    public void CollectorStartsTimedCollectionWhenColliderAwareRangeIsReached()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject building = CreateExtractor("Stocked", new Vector2(7f, 0f), 80);
        GameObject collectorObject = CreateCollector(new Vector2(16f, 0f));

        try
        {
            building.GetComponent<PlanetResourceExtractorBuilding>().Initialize(planetObject.GetComponent<ResourceDeposit>(), planetObject.transform, Vector2.right);
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.scanRange = 20f;

            collector.EvaluateGoal();

            Assert.That(collector.State, Is.EqualTo(CollectorState.MovingToBuilding));
            Assert.IsTrue(collector.TryInteractWithCurrentDestination());
            Assert.That(collector.State, Is.EqualTo(CollectorState.CollectingFromBuilding));
            Assert.That(collector.Cargo.GetAmount(ResourceType.Ore), Is.EqualTo(0));

            collector.CompleteCollectionNow();

            Assert.That(collector.Cargo.GetAmount(ResourceType.Ore), Is.EqualTo(80));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(building);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void CollectorDarkensWhileCollectingThenRestoresColor()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject building = CreateExtractor("Stocked", new Vector2(7f, 0f), 80);
        GameObject collectorObject = CreateCollector(new Vector2(16f, 0f));

        try
        {
            building.GetComponent<PlanetResourceExtractorBuilding>().Initialize(planetObject.GetComponent<ResourceDeposit>(), planetObject.transform, Vector2.right);
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            SpriteRenderer renderer = collectorObject.GetComponent<SpriteRenderer>();
            Color baseColor = renderer.color;
            collector.scanRange = 20f;

            collector.EvaluateGoal();
            collector.TryInteractWithCurrentDestination();

            Assert.That(renderer.color.r, Is.LessThan(baseColor.r));
            Assert.That(renderer.color.g, Is.LessThan(baseColor.g));
            Assert.That(renderer.color.b, Is.LessThan(baseColor.b));

            collector.CompleteCollectionNow();

            Assert.That(renderer.color, Is.EqualTo(baseColor));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(building);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void CollectorHasNoMovementDirectionWithoutRoute()
    {
        GameObject collectorObject = CreateCollector(Vector2.zero);
        GameObject hubObject = CreateHub(new Vector2(20f, 0f));

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();

            Vector2 direction = collector.DirectionForDestination(hubObject.transform);

            Assert.That(direction.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(direction.y, Is.EqualTo(0f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(hubObject);
        }
    }

    [Test]
    public void CollectorIdlesWhenNoRouteNetworkExists()
    {
        GameObject building = CreateExtractor("Stocked", new Vector2(3f, 0f), 80);
        GameObject collectorObject = CreateCollector(Vector2.zero);

        try
        {
            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();

            collector.EvaluateGoal();

            Assert.That(collector.State, Is.EqualTo(CollectorState.Idle));
            Assert.IsNull(collector.AssignedNetwork);
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(building);
        }
    }

    [Test]
    public void NetworkRoutePathUsesStoredRouteSegments()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject rightHub = CreateHub(new Vector2(20f, 0f));
        GameObject upHub = CreateHub(new Vector2(0f, 20f));

        try
        {
            PlanetLogisticsNetwork network = planetObject.GetComponent<PlanetLogisticsNetwork>();
            network.Rebuild();

            System.Collections.Generic.IReadOnlyList<Vector2> path = network.GetRoutePath(new Vector2(25f, 0f), upHub.transform);

            Assert.That(path.Count, Is.GreaterThan(1));
            for (int i = 0; i < path.Count - 1; i++)
            {
                Assert.IsTrue(HasRouteSegment(network, path[i], path[i + 1]));
            }
        }
        finally
        {
            Object.DestroyImmediate(rightHub);
            Object.DestroyImmediate(upHub);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void ExtractorRegistersToAnchoredPlanetNetwork()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject buildingObject = CreateExtractor("Mine", new Vector2(10f, 0f), 25);

        try
        {
            ResourceDeposit deposit = planetObject.GetComponent<ResourceDeposit>();
            PlanetLogisticsNetwork network = planetObject.GetComponent<PlanetLogisticsNetwork>();
            PlanetResourceExtractorBuilding extractor = buildingObject.GetComponent<PlanetResourceExtractorBuilding>();

            extractor.Initialize(deposit, planetObject.transform, Vector2.right);

            LogisticsNode node = buildingObject.GetComponent<LogisticsNode>();
            Assert.IsNotNull(node);
            Assert.That(node.Network, Is.EqualTo(network));
            Assert.Contains(node, (System.Collections.ICollection)network.Nodes);
        }
        finally
        {
            Object.DestroyImmediate(buildingObject);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void CargoHubRegistersToNearestPlanetNetwork()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject hubObject = CreateHub(new Vector2(12f, 0f));

        try
        {
            PlanetLogisticsNetwork network = planetObject.GetComponent<PlanetLogisticsNetwork>();
            LogisticsNode node = hubObject.GetComponent<LogisticsNode>();

            Assert.IsNotNull(node);
            Assert.That(node.Network, Is.EqualTo(network));
        }
        finally
        {
            Object.DestroyImmediate(hubObject);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void PlanetNetworkOrdersNodesDeterministicallyByAngle()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject rightHub = CreateHub(new Vector2(20f, 0f));
        GameObject upHub = CreateHub(new Vector2(0f, 20f));
        GameObject leftHub = CreateHub(new Vector2(-20f, 0f));

        try
        {
            PlanetLogisticsNetwork network = planetObject.GetComponent<PlanetLogisticsNetwork>();
            network.Rebuild();

            Assert.That(network.Nodes[0].transform, Is.EqualTo(rightHub.transform));
            Assert.That(network.Nodes[1].transform, Is.EqualTo(upHub.transform));
            Assert.That(network.Nodes[2].transform, Is.EqualTo(leftHub.transform));
        }
        finally
        {
            Object.DestroyImmediate(rightHub);
            Object.DestroyImmediate(upHub);
            Object.DestroyImmediate(leftHub);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void RoutePlannerInsertsWaypointAroundBlockingCollider()
    {
        GameObject blocker = new GameObject("Blocker");

        try
        {
            blocker.transform.position = Vector3.zero;
            CircleCollider2D collider = blocker.AddComponent<CircleCollider2D>();
            collider.radius = 2f;
            Physics2D.SyncTransforms();

            System.Collections.Generic.List<Vector2> path = LogisticsRoutePlanner.BuildSegmentPath(
                new Vector2(-10f, 0f),
                new Vector2(10f, 0f),
                null,
                1f,
                4f,
                null,
                null);

            Assert.That(path.Count, Is.EqualTo(3));
            Assert.That(Mathf.Abs(path[1].y), Is.GreaterThan(0.1f));
        }
        finally
        {
            Object.DestroyImmediate(blocker);
        }
    }

    [Test]
    public void CollectorPrefersAssignedPlanetNetworkTargets()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject networkBuilding = CreateExtractor("NetworkMine", new Vector2(16f, 0f), 30);
        GameObject globalBuilding = CreateExtractor("GlobalMine", new Vector2(3f, 0f), 100);
        GameObject collectorObject = CreateCollector(new Vector2(12f, 0f));

        try
        {
            PlanetResourceExtractorBuilding networkExtractor = networkBuilding.GetComponent<PlanetResourceExtractorBuilding>();
            networkExtractor.Initialize(planetObject.GetComponent<ResourceDeposit>(), planetObject.transform, Vector2.right);

            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.scanRange = 40f;

            collector.EvaluateGoal();

            Assert.That(collector.AssignedNetwork, Is.EqualTo(planetObject.GetComponent<PlanetLogisticsNetwork>()));
            Assert.That(collector.TargetBuilding, Is.EqualTo(networkExtractor));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(networkBuilding);
            Object.DestroyImmediate(globalBuilding);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void CollectorMovesTowardFirstNetworkRouteWaypoint()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject buildingObject = CreateExtractor("NetworkMine", new Vector2(16f, 0f), 30);
        GameObject hubObject = CreateHub(new Vector2(-16f, 0f));
        GameObject collectorObject = CreateCollector(new Vector2(0f, 18f));

        try
        {
            PlanetResourceExtractorBuilding extractor = buildingObject.GetComponent<PlanetResourceExtractorBuilding>();
            extractor.Initialize(planetObject.GetComponent<ResourceDeposit>(), planetObject.transform, Vector2.right);

            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.scanRange = 80f;

            Assert.IsTrue(collector.TryBuildSharedRouteTo(hubObject.transform));

            Vector2 direction = collector.DirectionForDestination(hubObject.transform);
            Vector2 routeDirection = (collector.RoutePath[0] - (Vector2)collector.transform.position).normalized;

            Assert.That(collector.RoutePath.Count, Is.GreaterThan(0));
            Assert.That(Vector2.Dot(direction, routeDirection), Is.GreaterThan(0.99f));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(hubObject);
            Object.DestroyImmediate(buildingObject);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void CollectorImmediatelyLeavesHubAfterDepositWhenExtractorHasResources()
    {
        GameObject planetObject = CreatePlanet("Planet", Vector2.zero);
        GameObject buildingObject = CreateExtractor("NetworkMine", new Vector2(16f, 0f), 30);
        GameObject hubObject = CreateHub(new Vector2(-16f, 0f));
        GameObject collectorObject = CreateCollector(new Vector2(-16f, 0f));

        try
        {
            PlanetResourceExtractorBuilding extractor = buildingObject.GetComponent<PlanetResourceExtractorBuilding>();
            extractor.Initialize(planetObject.GetComponent<ResourceDeposit>(), planetObject.transform, Vector2.right);

            CollectorAutomaton collector = collectorObject.GetComponent<CollectorAutomaton>();
            collector.scanRange = 80f;
            collector.Cargo.AddResource(ResourceType.Ore, 20);
            collector.EvaluateGoal();

            Assert.That(collector.State, Is.EqualTo(CollectorState.DeliveringToHub));
            Assert.IsTrue(collector.TryInteractWithCurrentDestination());

            Assert.That(collector.Cargo.TotalAmount, Is.EqualTo(0));
            Assert.That(collector.State, Is.EqualTo(CollectorState.MovingToBuilding));
            Assert.That(collector.TargetBuilding, Is.EqualTo(extractor));
            Assert.That(collector.RoutePath.Count, Is.GreaterThan(0));
        }
        finally
        {
            Object.DestroyImmediate(collectorObject);
            Object.DestroyImmediate(hubObject);
            Object.DestroyImmediate(buildingObject);
            Object.DestroyImmediate(planetObject);
        }
    }

    [Test]
    public void HiddenRoutingVisibilityCanBeToggledByState()
    {
        HiddenRoutingDisplayController.SetRoutesVisible(false);
        Assert.IsFalse(HiddenRoutingDisplayController.RoutesVisible);

        HiddenRoutingDisplayController.SetRoutesVisible(true);
        Assert.IsTrue(HiddenRoutingDisplayController.RoutesVisible);

        HiddenRoutingDisplayController.SetRoutesVisible(false);
    }

    [Test]
    public void PlacementBlockedByNonTriggerColliderOnly()
    {
        GameObject blocker = new GameObject("Blocker");
        GameObject trigger = new GameObject("Trigger");

        try
        {
            blocker.transform.position = Vector3.zero;
            blocker.AddComponent<CircleCollider2D>().radius = 1f;

            trigger.transform.position = new Vector3(6f, 0f, 0f);
            CircleCollider2D triggerCollider = trigger.AddComponent<CircleCollider2D>();
            triggerCollider.radius = 1f;
            triggerCollider.isTrigger = true;

            Assert.IsTrue(AutomatonPlacementController.IsBlockedAt(Vector2.zero, 1f));
            Assert.IsFalse(AutomatonPlacementController.IsBlockedAt(new Vector2(6f, 0f), 1f));
            Assert.IsFalse(AutomatonPlacementController.IsBlockedAt(new Vector2(12f, 0f), 1f));
        }
        finally
        {
            Object.DestroyImmediate(blocker);
            Object.DestroyImmediate(trigger);
        }
    }

    [Test]
    public void SatelliteFactoryRequiresFullRecipeBeforeStartingBuild()
    {
        GameObject factoryObject = new GameObject("Factory");

        try
        {
            SatelliteFactory factory = factoryObject.AddComponent<SatelliteFactory>();
            factory.Storage.AddResource(ResourceType.Ore, SatelliteFactory.OreCost);
            factory.Storage.AddResource(ResourceType.Copper, SatelliteFactory.CopperCost);

            Assert.IsFalse(factory.CanStartBuild());
            Assert.IsFalse(factory.TryStartBuild());
            Assert.IsFalse(factory.IsBuilding);
        }
        finally
        {
            Object.DestroyImmediate(factoryObject);
        }
    }

    [Test]
    public void SatelliteFactoryConsumesRecipeAndSpawnsDynamicReflector()
    {
        GameObject factoryObject = new GameObject("Factory");
        factoryObject.transform.position = new Vector3(100f, 0f, 0f);

        try
        {
            SatelliteFactory factory = factoryObject.AddComponent<SatelliteFactory>();
            factory.Storage.AddResource(ResourceType.Ore, SatelliteFactory.OreCost);
            factory.Storage.AddResource(ResourceType.Copper, SatelliteFactory.CopperCost);
            factory.Storage.AddResource(ResourceType.Silicate, SatelliteFactory.SilicateCost);

            DysonSatellite satellite = factory.CompleteBuildNow();

            Assert.IsNotNull(satellite);
            Assert.That(factory.Storage.GetAmount(ResourceType.Ore), Is.EqualTo(0));
            Assert.That(factory.Storage.GetAmount(ResourceType.Copper), Is.EqualTo(0));
            Assert.That(factory.Storage.GetAmount(ResourceType.Silicate), Is.EqualTo(0));
            Assert.That(factory.ProducedCount, Is.EqualTo(1));
            Assert.That(satellite.mode, Is.EqualTo(DysonSatelliteMode.Dynamic));
            Assert.That(satellite.orbitRadius, Is.EqualTo(103f).Within(0.001f));
            Assert.That(satellite.orbitSpeedDegrees, Is.EqualTo(SatelliteFactory.DefaultOrbitSpeedDegrees).Within(0.001f));

            Object.DestroyImmediate(satellite.gameObject);
        }
        finally
        {
            Object.DestroyImmediate(factoryObject);
        }
    }

    [Test]
    public void SatelliteFactoryPlacementRequiresNearSunAnnulus()
    {
        Assert.IsFalse(AutomatonPlacementController.IsValidSatelliteFactoryPosition(new Vector2(5f, 0f)));
        Assert.IsTrue(AutomatonPlacementController.IsValidSatelliteFactoryPosition(new Vector2(100f, 0f)));
        Assert.IsFalse(AutomatonPlacementController.IsValidSatelliteFactoryPosition(new Vector2(220f, 0f)));
    }

    [Test]
    public void FreighterPriorityLoadsOnlySelectedResource()
    {
        GameObject sourceObject = new GameObject("Source");
        GameObject destinationObject = new GameObject("Destination");
        GameObject freighterObject = new GameObject("Freighter");

        try
        {
            ResourceStorage source = sourceObject.AddComponent<ResourceStorage>();
            source.Configure(1000);
            source.AddResource(ResourceType.Ore, 100);
            source.AddResource(ResourceType.Copper, 100);

            ResourceStorage destination = destinationObject.AddComponent<ResourceStorage>();
            destination.Configure(1000);

            ResourceStorage cargo = freighterObject.AddComponent<ResourceStorage>();
            cargo.Configure(1000);
            FreighterAutomaton freighter = freighterObject.AddComponent<FreighterAutomaton>();
            freighter.transferAmountPerTrip = 60;
            freighter.AssignEndpoints(source, destination);
            freighter.SetCargoPriority(FreighterCargoPriority.Copper);

            freighter.CompleteLoadingForTests();

            Assert.That(freighter.Cargo.GetAmount(ResourceType.Copper), Is.EqualTo(60));
            Assert.That(freighter.Cargo.GetAmount(ResourceType.Ore), Is.EqualTo(0));
            Assert.That(source.GetAmount(ResourceType.Copper), Is.EqualTo(40));
            Assert.That(source.GetAmount(ResourceType.Ore), Is.EqualTo(100));
        }
        finally
        {
            Object.DestroyImmediate(freighterObject);
            Object.DestroyImmediate(destinationObject);
            Object.DestroyImmediate(sourceObject);
        }
    }

    [Test]
    public void FreighterMixedPriorityLoadsEvenShareOfAvailableResources()
    {
        GameObject sourceObject = new GameObject("Source");
        GameObject destinationObject = new GameObject("Destination");
        GameObject freighterObject = new GameObject("Freighter");

        try
        {
            ResourceStorage source = sourceObject.AddComponent<ResourceStorage>();
            source.Configure(1000);
            source.AddResource(ResourceType.Ore, 100);
            source.AddResource(ResourceType.Copper, 100);
            source.AddResource(ResourceType.Silicate, 100);

            ResourceStorage destination = destinationObject.AddComponent<ResourceStorage>();
            destination.Configure(1000);

            ResourceStorage cargo = freighterObject.AddComponent<ResourceStorage>();
            cargo.Configure(1000);
            FreighterAutomaton freighter = freighterObject.AddComponent<FreighterAutomaton>();
            freighter.transferAmountPerTrip = 60;
            freighter.AssignEndpoints(source, destination);
            freighter.SetCargoPriority(FreighterCargoPriority.Mixed);

            freighter.CompleteLoadingForTests();

            Assert.That(freighter.Cargo.GetAmount(ResourceType.Ore), Is.EqualTo(20));
            Assert.That(freighter.Cargo.GetAmount(ResourceType.Copper), Is.EqualTo(20));
            Assert.That(freighter.Cargo.GetAmount(ResourceType.Silicate), Is.EqualTo(20));
        }
        finally
        {
            Object.DestroyImmediate(freighterObject);
            Object.DestroyImmediate(destinationObject);
            Object.DestroyImmediate(sourceObject);
        }
    }

    private static GameObject CreateExtractor(string name, Vector2 position, int storedAmount)
    {
        GameObject building = new GameObject(name);
        building.transform.position = position;
        building.AddComponent<SpriteRenderer>();
        building.AddComponent<CircleCollider2D>();
        building.AddComponent<PlanetSurfaceAnchor>();
        BuildingStorage storage = building.AddComponent<BuildingStorage>();
        PlanetMine mine = building.AddComponent<PlanetMine>();

        storage.Configure(ResourceType.Ore, 250);
        storage.AddResource(ResourceType.Ore, storedAmount);

        return building;
    }

    private static GameObject CreateCollector(Vector2 position)
    {
        GameObject collector = new GameObject("Collector");
        collector.transform.position = position;
        collector.AddComponent<SpriteRenderer>();
        collector.AddComponent<CircleCollider2D>();
        collector.AddComponent<Rigidbody2D>();
        collector.AddComponent<ResourceStorage>();
        collector.AddComponent<CollectorAutomaton>();
        return collector;
    }

    private static GameObject CreateHub(Vector2 position)
    {
        GameObject hub = new GameObject("Hub");
        hub.transform.position = position;
        hub.AddComponent<CircleCollider2D>();
        hub.AddComponent<ResourceStorage>();
        hub.AddComponent<CollectorHub>();
        return hub;
    }

    private static GameObject CreatePlanet(string name, Vector2 position)
    {
        GameObject planet = new GameObject(name);
        planet.transform.position = position;
        CircleCollider2D collider = planet.AddComponent<CircleCollider2D>();
        collider.radius = 10f;
        ResourceDeposit deposit = planet.AddComponent<ResourceDeposit>();
        deposit.resourceType = ResourceType.Ore;
        deposit.maxAmount = 1000;
        deposit.remainingAmount = 1000;
        planet.AddComponent<PlanetLogisticsNetwork>();
        return planet;
    }

    private static bool HasRouteSegment(PlanetLogisticsNetwork network, Vector2 start, Vector2 end)
    {
        for (int i = 0; i < network.RouteSegments.Count; i++)
        {
            LogisticsRouteSegment segment = network.RouteSegments[i];
            bool forward = Vector2.Distance(segment.start, start) <= 0.05f && Vector2.Distance(segment.end, end) <= 0.05f;
            bool reverse = Vector2.Distance(segment.start, end) <= 0.05f && Vector2.Distance(segment.end, start) <= 0.05f;
            if (forward || reverse)
            {
                return true;
            }
        }

        return false;
    }
}
