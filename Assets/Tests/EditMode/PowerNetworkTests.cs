#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PowerNetworkTests
{
    [Test]
    public void ActiveAlignedBeamPowersNearbyConsumer()
    {
        GameObject dynamicObject = CreateSatellite("Dynamic", DysonSatelliteMode.Dynamic, new Vector2(90f, 0f));
        GameObject receiverObject = CreateSatellite("Receiver", DysonSatelliteMode.Stationary, new Vector2(100f, 0f));
        GameObject consumerObject = CreateFactory("Factory", new Vector2(120f, 0f));
        GameObject controllerObject = new GameObject("PowerNetworkController");

        try
        {
            PowerNetworkController controller = controllerObject.AddComponent<PowerNetworkController>();
            controller.RebuildNow();

            SatelliteFactory factory = consumerObject.GetComponent<SatelliteFactory>();
            Assert.That(controller.TotalGeneration, Is.EqualTo(PowerNetworkController.PowerPerActiveBeam));
            Assert.IsTrue(factory.IsPowered);
        }
        finally
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(consumerObject);
            Object.DestroyImmediate(receiverObject);
            Object.DestroyImmediate(dynamicObject);
        }
    }

    [Test]
    public void NoAlignedBeamLeavesConsumerUnpowered()
    {
        GameObject dynamicObject = CreateSatellite("Dynamic", DysonSatelliteMode.Dynamic, new Vector2(0f, 90f));
        GameObject receiverObject = CreateSatellite("Receiver", DysonSatelliteMode.Stationary, new Vector2(100f, 0f));
        GameObject consumerObject = CreateFactory("Factory", new Vector2(120f, 0f));
        GameObject controllerObject = new GameObject("PowerNetworkController");

        try
        {
            PowerNetworkController controller = controllerObject.AddComponent<PowerNetworkController>();
            controller.RebuildNow();

            Assert.That(controller.TotalGeneration, Is.EqualTo(0));
            Assert.IsFalse(consumerObject.GetComponent<SatelliteFactory>().IsPowered);
        }
        finally
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(consumerObject);
            Object.DestroyImmediate(receiverObject);
            Object.DestroyImmediate(dynamicObject);
        }
    }

    [Test]
    public void RelayChainCarriesPowerToDistantConsumer()
    {
        GameObject dynamicObject = CreateSatellite("Dynamic", DysonSatelliteMode.Dynamic, new Vector2(90f, 0f));
        GameObject receiverObject = CreateSatellite("Receiver", DysonSatelliteMode.Stationary, new Vector2(100f, 0f));
        GameObject relayAObject = CreateRelay("Relay A", new Vector2(340f, 0f));
        GameObject relayBObject = CreateRelay("Relay B", new Vector2(580f, 0f));
        GameObject consumerObject = CreateFactory("Factory", new Vector2(780f, 0f));
        GameObject controllerObject = new GameObject("PowerNetworkController");

        try
        {
            PowerNetworkController controller = controllerObject.AddComponent<PowerNetworkController>();
            controller.RebuildNow();

            Assert.IsTrue(relayAObject.GetComponent<PowerRelay>().IsConnected);
            Assert.IsTrue(relayBObject.GetComponent<PowerRelay>().IsConnected);
            Assert.IsTrue(consumerObject.GetComponent<SatelliteFactory>().IsPowered);
        }
        finally
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(consumerObject);
            Object.DestroyImmediate(relayBObject);
            Object.DestroyImmediate(relayAObject);
            Object.DestroyImmediate(receiverObject);
            Object.DestroyImmediate(dynamicObject);
        }
    }

    [Test]
    public void OverloadedNetworkPowersConsumersInStableNameOrder()
    {
        GameObject dynamicObject = CreateSatellite("Dynamic", DysonSatelliteMode.Dynamic, new Vector2(90f, 0f));
        GameObject receiverObject = CreateSatellite("Receiver", DysonSatelliteMode.Stationary, new Vector2(100f, 0f));
        GameObject firstObject = CreateFactory("A Factory", new Vector2(115f, 0f));
        GameObject secondObject = CreateFactory("B Factory", new Vector2(125f, 0f));
        GameObject thirdObject = CreateFactory("C Factory", new Vector2(135f, 0f));
        GameObject fourthObject = CreateFactory("D Factory", new Vector2(145f, 0f));
        GameObject controllerObject = new GameObject("PowerNetworkController");

        try
        {
            PowerNetworkController controller = controllerObject.AddComponent<PowerNetworkController>();
            controller.RebuildNow();

            Assert.IsTrue(firstObject.GetComponent<SatelliteFactory>().IsPowered);
            Assert.IsTrue(secondObject.GetComponent<SatelliteFactory>().IsPowered);
            Assert.IsTrue(thirdObject.GetComponent<SatelliteFactory>().IsPowered);
            Assert.IsFalse(fourthObject.GetComponent<SatelliteFactory>().IsPowered);
            Assert.That(controller.PoweredDemand, Is.EqualTo(90));
        }
        finally
        {
            Object.DestroyImmediate(controllerObject);
            Object.DestroyImmediate(fourthObject);
            Object.DestroyImmediate(thirdObject);
            Object.DestroyImmediate(secondObject);
            Object.DestroyImmediate(firstObject);
            Object.DestroyImmediate(receiverObject);
            Object.DestroyImmediate(dynamicObject);
        }
    }

    [Test]
    public void UnpoweredFactoryDoesNotStartBuild()
    {
        GameObject factoryObject = CreateFactory("Factory", Vector2.zero);

        try
        {
            SatelliteFactory factory = factoryObject.GetComponent<SatelliteFactory>();
            factory.Storage.AddResource(ResourceType.Ore, SatelliteFactory.OreCost);
            factory.Storage.AddResource(ResourceType.Silicate, SatelliteFactory.SilicateCost);
            factory.SetPowered(false);

            Assert.IsFalse(factory.TryStartBuild());
            Assert.IsFalse(factory.IsBuilding);
        }
        finally
        {
            Object.DestroyImmediate(factoryObject);
        }
    }

    [Test]
    public void UnpoweredFreighterDoesNotLoadCargo()
    {
        GameObject sourceObject = new GameObject("Source");
        GameObject destinationObject = new GameObject("Destination");
        GameObject freighterObject = new GameObject("Freighter");

        try
        {
            ResourceStorage source = sourceObject.AddComponent<ResourceStorage>();
            source.Configure(1000);
            source.AddResource(ResourceType.Ore, 100);

            ResourceStorage destination = destinationObject.AddComponent<ResourceStorage>();
            destination.Configure(1000);

            ResourceStorage cargo = freighterObject.AddComponent<ResourceStorage>();
            cargo.Configure(1000);
            FreighterAutomaton freighter = freighterObject.AddComponent<FreighterAutomaton>();
            freighter.AssignEndpoints(source, destination);
            freighter.SetPowered(false);

            freighter.CompleteLoadingForTests();

            Assert.That(freighter.Cargo.TotalAmount, Is.EqualTo(0));
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
    public void UnpoweredCompanionDoesNotCollectPickup()
    {
        GameObject companionObject = new GameObject("Companion");
        GameObject pickupObject = new GameObject("Pickup");

        try
        {
            CompanionAutomaton companion = companionObject.AddComponent<CompanionAutomaton>();
            ResourcePickup pickup = pickupObject.AddComponent<ResourcePickup>();
            pickup.Initialize(ResourceType.Ore, 12, Vector2.zero, ResourceVisuals.ColorFor(ResourceType.Ore));
            companion.SetPowered(false);

            Assert.IsFalse(companion.TryCollectPickup(pickup));
            Assert.That(companion.Cargo.TotalAmount, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(pickupObject);
            Object.DestroyImmediate(companionObject);
        }
    }

    private static GameObject CreateSatellite(string name, DysonSatelliteMode mode, Vector2 position)
    {
        GameObject satelliteObject = new GameObject(name);
        satelliteObject.transform.position = position;
        DysonSatellite satellite = satelliteObject.AddComponent<DysonSatellite>();
        satellite.mode = mode;
        satellite.sunPosition = Vector2.zero;
        if (!satellite.IsDynamic)
        {
            satellite.SetStationaryPosition(position);
        }

        return satelliteObject;
    }

    private static GameObject CreateFactory(string name, Vector2 position)
    {
        GameObject factoryObject = new GameObject(name);
        factoryObject.transform.position = position;
        factoryObject.AddComponent<SpriteRenderer>();
        factoryObject.AddComponent<ResourceStorage>();
        factoryObject.AddComponent<SatelliteFactory>();
        return factoryObject;
    }

    private static GameObject CreateRelay(string name, Vector2 position)
    {
        GameObject relayObject = new GameObject(name);
        relayObject.transform.position = position;
        relayObject.AddComponent<SpriteRenderer>();
        relayObject.AddComponent<PowerRelay>();
        return relayObject;
    }
}
#endif
