using NUnit.Framework;
using UnityEngine;

public class StarSystemGeneratorTests
{
    [Test]
    public void SameSeedProducesSameLayout()
    {
        StarSystemLayout first = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);
        StarSystemLayout second = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);

        Assert.AreEqual(first.starType, second.starType);
        Assert.AreEqual(first.planets.Count, second.planets.Count);
        Assert.AreEqual(first.asteroids.Count, second.asteroids.Count);
        Assert.AreEqual(first.dysonSatellites.Count, second.dysonSatellites.Count);

        for (int i = 0; i < first.planets.Count; i++)
        {
            Assert.That(Vector2.Distance(first.planets[i].position, second.planets[i].position), Is.LessThan(0.001f));
            Assert.AreEqual(first.planets[i].primaryResource, second.planets[i].primaryResource);
        }

        for (int i = 0; i < first.asteroids.Count; i++)
        {
            Assert.That(Vector2.Distance(first.asteroids[i].position, second.asteroids[i].position), Is.LessThan(0.001f));
            Assert.AreEqual(first.asteroids[i].resourceAmount, second.asteroids[i].resourceAmount);
        }

        for (int i = 0; i < first.dysonSatellites.Count; i++)
        {
            Assert.That(Vector2.Distance(first.dysonSatellites[i].position, second.dysonSatellites[i].position), Is.LessThan(0.001f));
            Assert.AreEqual(first.dysonSatellites[i].mode, second.dysonSatellites[i].mode);
        }
    }

    [Test]
    public void DifferentSeedsProduceDifferentLayouts()
    {
        StarSystemLayout first = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);
        StarSystemLayout second = StarSystemGenerator.GenerateLayout(43, 80f, 3, 5, 3, 8);

        Assert.AreNotEqual(first.planets[0].position, second.planets[0].position);
    }

    [Test]
    public void GeneratedBodiesStayInsideSystemBoundsAndHaveFiniteResources()
    {
        const float systemRadius = 80f;
        StarSystemLayout layout = StarSystemGenerator.GenerateLayout(1107, systemRadius, 3, 5, 3, 16);

        Assert.That(layout.planets.Count, Is.InRange(3, 5));
        Assert.That(layout.asteroids.Count, Is.EqualTo(48));

        foreach (CelestialBodyDefinition planet in layout.planets)
        {
            Assert.That(planet.position.magnitude, Is.LessThanOrEqualTo(systemRadius));
            Assert.That(planet.resourceAmount, Is.GreaterThan(0));
        }

        foreach (CelestialBodyDefinition asteroid in layout.asteroids)
        {
            Assert.That(asteroid.position.magnitude, Is.LessThanOrEqualTo(systemRadius));
            Assert.That(asteroid.resourceAmount, Is.GreaterThan(0));
        }
    }

    [Test]
    public void AsteroidsAreDistributedAcrossSystemRadius()
    {
        const float systemRadius = 80f;
        StarSystemLayout layout = StarSystemGenerator.GenerateLayout(1107, systemRadius, 3, 5, 3, 16);

        bool hasInnerAsteroid = false;
        bool hasOuterAsteroid = false;

        foreach (CelestialBodyDefinition asteroid in layout.asteroids)
        {
            if (asteroid.position.magnitude < systemRadius * 0.45f)
            {
                hasInnerAsteroid = true;
            }

            if (asteroid.position.magnitude > systemRadius * 0.75f)
            {
                hasOuterAsteroid = true;
            }
        }

        Assert.IsTrue(hasInnerAsteroid);
        Assert.IsTrue(hasOuterAsteroid);
    }

    [Test]
    public void ColliderRadiiMatchGeneratedSpriteEdges()
    {
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Planet), Is.EqualTo(0.48f).Within(0.001f));
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Star), Is.EqualTo(0.47f).Within(0.001f));
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Asteroid), Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void MineableAsteroidAddsInteractionTriggerWithoutReplacingSolidCollider()
    {
        GameObject asteroidObject = new GameObject("Asteroid");

        try
        {
            CircleCollider2D solidCollider = asteroidObject.AddComponent<CircleCollider2D>();
            solidCollider.isTrigger = false;

            asteroidObject.AddComponent<MineableAsteroid>();

            CircleCollider2D[] colliders = asteroidObject.GetComponents<CircleCollider2D>();
            bool hasSolidCollider = false;
            bool hasTriggerCollider = false;

            for (int i = 0; i < colliders.Length; i++)
            {
                hasSolidCollider |= !colliders[i].isTrigger;
                hasTriggerCollider |= colliders[i].isTrigger;
            }

            Assert.IsTrue(hasSolidCollider);
            Assert.IsTrue(hasTriggerCollider);
        }
        finally
        {
            Object.DestroyImmediate(asteroidObject);
        }
    }

    [Test]
    public void DefaultLayoutContainsDysonSatelliteReceiversAndReflectors()
    {
        StarSystemLayout layout = StarSystemGenerator.GenerateLayout(1107, 80f, 3, 5, 3, 16);
        int stationaryCount = 0;
        int dynamicCount = 0;

        for (int i = 0; i < layout.dysonSatellites.Count; i++)
        {
            if (layout.dysonSatellites[i].mode == DysonSatelliteMode.Stationary)
            {
                stationaryCount++;
            }
            else if (layout.dysonSatellites[i].mode == DysonSatelliteMode.Dynamic)
            {
                dynamicCount++;
            }
        }

        Assert.That(stationaryCount, Is.EqualTo(2));
        Assert.That(dynamicCount, Is.EqualTo(6));
    }

    [Test]
    public void DynamicSatelliteOrbitPositionChangesOverTime()
    {
        Vector2 first = DysonSatellite.OrbitPositionAtTime(Vector2.zero, 20f, 0f, 10f, 0f);
        Vector2 second = DysonSatellite.OrbitPositionAtTime(Vector2.zero, 20f, 0f, 10f, 3f);

        Assert.That(Vector2.Distance(first, second), Is.GreaterThan(0.1f));
    }

    [Test]
    public void StationarySatellitePositionDoesNotChangeWithZeroOrbitSpeed()
    {
        Vector2 first = DysonSatellite.OrbitPositionAtTime(Vector2.zero, 20f, 45f, 0f, 0f);
        Vector2 second = DysonSatellite.OrbitPositionAtTime(Vector2.zero, 20f, 45f, 0f, 30f);

        Assert.That(Vector2.Distance(first, second), Is.LessThan(0.001f));
    }

    [Test]
    public void DysonBeamAlignmentUsesAngularTolerance()
    {
        Vector2 dynamicPosition = new Vector2(10f, 0f);
        Vector2 alignedStation = new Vector2(20f, 1f);
        Vector2 unalignedStation = new Vector2(0f, 20f);

        Assert.IsTrue(DysonBeamNetwork.AreAligned(Vector2.zero, dynamicPosition, alignedStation, 8f));
        Assert.IsFalse(DysonBeamNetwork.AreAligned(Vector2.zero, dynamicPosition, unalignedStation, 8f));
    }
}
