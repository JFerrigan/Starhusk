#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class CircularDestructibleAsteroidTests
{
    [SetUp]
    public void SetUp()
    {
        CleanupGeneratedObjects();
    }

    [TearDown]
    public void TearDown()
    {
        CleanupGeneratedObjects();
    }

    [Test]
    public void CircularCutReducesAreaAndColliderShape()
    {
        GameObject asteroidObject = CreateAsteroid("Cut Test Asteroid", 100);
        CircularDestructibleAsteroid asteroid = asteroidObject.GetComponent<CircularDestructibleAsteroid>();
        PolygonCollider2D collider = asteroidObject.GetComponent<PolygonCollider2D>();

        float areaBefore = asteroid.CurrentArea;
        Assert.That(collider.pathCount, Is.LessThan(asteroid.CellCount));

        bool cutApplied = asteroid.ApplyCircularCut(new Vector2(5f, 0f), 2.5f, Vector2.right);

        Assert.IsTrue(cutApplied);
        Assert.That(asteroid.CurrentArea, Is.LessThan(areaBefore));
        Assert.That(asteroidObject.GetComponent<PolygonCollider2D>().pathCount, Is.LessThan(asteroid.CellCount));
    }

    [Test]
    public void CircularCutConservesResourcesAcrossAsteroidAndPickups()
    {
        CreateAsteroid("Conservation Asteroid", 120);
        CircularDestructibleAsteroid asteroid = Object.FindFirstObjectByType<CircularDestructibleAsteroid>();

        bool cutApplied = asteroid.ApplyCircularCut(new Vector2(5f, 0f), 3f, Vector2.right);

        Assert.IsTrue(cutApplied);
        Assert.That(TotalAsteroidResources() + TotalPickupResources(), Is.EqualTo(120));
        Assert.That(TotalPickupResources(), Is.GreaterThan(0));
    }

    [Test]
    public void AsteroidHiddenFromMapAndRadarAtDefaultDestroyedThreshold()
    {
        GameObject asteroidObject = new GameObject("Mostly Destroyed Asteroid");
        MapMarker marker = asteroidObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Asteroid;
        ResourceDeposit deposit = asteroidObject.AddComponent<ResourceDeposit>();
        deposit.ConfigureSingleResource(ResourceType.Ore, 100, 12);
        CircularDestructibleAsteroid asteroid = asteroidObject.AddComponent<CircularDestructibleAsteroid>();

        asteroid.InitializeFromCells(
            new List<Vector2Int> { Vector2Int.zero },
            1f,
            Color.white,
            new List<ResourceStack> { new ResourceStack(ResourceType.Ore, 25) },
            12,
            4f
        );

        Assert.That(asteroid.destroyedMapHideThreshold, Is.EqualTo(0.75f).Within(0.001f));
        Assert.That(asteroid.DestroyedFraction, Is.EqualTo(0.75f).Within(0.001f));
        Assert.IsFalse(asteroid.ShouldAppearOnMapAndRadar);
        Assert.IsTrue(marker.hiddenFromMapAndRadar);
        Assert.IsFalse(marker.IsVisible);
    }

    [Test]
    public void ResourcePickupCollectsIntoInventory()
    {
        GameObject player = new GameObject("Pickup Test Player");
        player.AddComponent<BoxCollider2D>();
        ResourceInventory inventory = player.AddComponent<ResourceInventory>();

        GameObject pickupObject = new GameObject("Ore Pickup");
        pickupObject.transform.position = player.transform.position;
        ResourcePickup pickup = pickupObject.AddComponent<ResourcePickup>();
        pickup.Initialize(ResourceType.Ore, 7, Vector2.zero, ResourceVisuals.ColorFor(ResourceType.Ore));

        pickupObject.SendMessage("Update");

        Assert.That(inventory.GetAmount(ResourceType.Ore), Is.EqualTo(7));

        Object.DestroyImmediate(player);
    }

    private static GameObject CreateAsteroid(string name, int resourceAmount)
    {
        GameObject asteroidObject = new GameObject(name);
        asteroidObject.transform.localScale = Vector3.one * 10f;

        ResourceDeposit deposit = asteroidObject.AddComponent<ResourceDeposit>();
        deposit.ConfigureSingleResource(ResourceType.Ore, resourceAmount, 12);
        deposit.destroyWhenDepleted = false;

        CircularDestructibleAsteroid asteroid = asteroidObject.AddComponent<CircularDestructibleAsteroid>();
        asteroid.SetTint(ResourceVisuals.ColorFor(ResourceType.Ore));
        return asteroidObject;
    }

    private static int TotalAsteroidResources()
    {
        int total = 0;
        ResourceDeposit[] deposits = Object.FindObjectsByType<ResourceDeposit>(FindObjectsSortMode.None);

        for (int i = 0; i < deposits.Length; i++)
        {
            if (deposits[i] != null)
            {
                total += deposits[i].TotalRemainingAmount;
            }
        }

        return total;
    }

    private static int TotalPickupResources()
    {
        int total = 0;
        ResourcePickup[] pickups = Object.FindObjectsByType<ResourcePickup>(FindObjectsSortMode.None);

        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i] != null)
            {
                total += pickups[i].amount;
            }
        }

        return total;
    }

    private static void CleanupGeneratedObjects()
    {
        CircularDestructibleAsteroid[] asteroids = Object.FindObjectsByType<CircularDestructibleAsteroid>(FindObjectsSortMode.None);
        for (int i = 0; i < asteroids.Length; i++)
        {
            if (asteroids[i] != null)
            {
                Object.DestroyImmediate(asteroids[i].gameObject);
            }
        }

        ResourcePickup[] pickups = Object.FindObjectsByType<ResourcePickup>(FindObjectsSortMode.None);
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i] != null)
            {
                Object.DestroyImmediate(pickups[i].gameObject);
            }
        }

        ResourceInventory[] inventories = Object.FindObjectsByType<ResourceInventory>(FindObjectsSortMode.None);
        for (int i = 0; i < inventories.Length; i++)
        {
            if (inventories[i] != null && inventories[i].gameObject.name.StartsWith("Pickup Test"))
            {
                Object.DestroyImmediate(inventories[i].gameObject);
            }
        }
    }
}
#endif
